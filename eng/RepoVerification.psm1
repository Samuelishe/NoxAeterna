Set-StrictMode -Version Latest

function Get-RepositoryRoot {
    param([string]$RequestedRoot)

    if (-not [string]::IsNullOrWhiteSpace($RequestedRoot)) {
        return (Resolve-Path -LiteralPath $RequestedRoot).Path
    }

    $resolved = (& git rev-parse --show-toplevel 2>$null)
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($resolved)) {
        throw 'Unable to determine the repository root. Pass -Root explicitly.'
    }

    return (Resolve-Path -LiteralPath $resolved.Trim()).Path
}

function Get-RelativeRepositoryPath {
    param(
        [string]$Root,
        [string]$Path
    )

    [IO.Path]::GetRelativePath($Root, $Path).Replace('\', '/')
}

function Read-TestRouteRegistry {
    param(
        [string]$Root,
        [string]$RegistryPath
    )

    $absolutePath = if ([IO.Path]::IsPathRooted($RegistryPath)) {
        $RegistryPath
    }
    else {
        Join-Path $Root $RegistryPath
    }

    if (-not (Test-Path -LiteralPath $absolutePath -PathType Leaf)) {
        throw "Test route registry does not exist: $RegistryPath"
    }

    try {
        $registry = Get-Content -LiteralPath $absolutePath -Raw | ConvertFrom-Json
    }
    catch {
        throw "Test route registry is not valid JSON: $($_.Exception.Message)"
    }

    if ($registry.schemaVersion -ne 1) {
        throw 'Only test route registry schemaVersion 1 is supported.'
    }

    $routes = @($registry.routes)
    if ($routes.Count -eq 0) {
        throw 'Test route registry must contain at least one route.'
    }

    $byName = [Collections.Generic.Dictionary[string, object]]::new(
        [StringComparer]::OrdinalIgnoreCase)
    foreach ($route in $routes) {
        $name = [string]$route.name
        if ([string]::IsNullOrWhiteSpace($name)) {
            throw 'Every test route requires a name.'
        }
        if ($byName.ContainsKey($name)) {
            throw "Duplicate test route name '$name'."
        }
        $byName.Add($name, $route)
    }

    $maximumTimeoutSeconds = 1800
    foreach ($route in $routes) {
        $name = [string]$route.name
        $kind = [string]$route.kind
        if ($kind -notin @('leaf', 'composite')) {
            throw "Route '$name' has unknown kind '$kind'."
        }
        if ([string]::IsNullOrWhiteSpace([string]$route.description)) {
            throw "Route '$name' requires a description."
        }
        if ($route.hardwareEvidence -ne $false) {
            throw "Route '$name' must declare hardwareEvidence false."
        }
        if ($route.milestoneOnly -isnot [bool]) {
            throw "Route '$name' must declare milestoneOnly as a boolean."
        }

        $properties = @($route.PSObject.Properties.Name)
        if ($kind -eq 'leaf') {
            if ($properties -contains 'children') {
                throw "Leaf route '$name' cannot contain children."
            }
            if ([string]::IsNullOrWhiteSpace([string]$route.testProject)) {
                throw "Leaf route '$name' requires testProject."
            }
            $projectPath = [IO.Path]::GetFullPath((Join-Path $Root ([string]$route.testProject)))
            $relativeProjectPath = [IO.Path]::GetRelativePath($Root, $projectPath)
            if ($relativeProjectPath -eq '..' -or
                $relativeProjectPath.StartsWith("../", [StringComparison]::Ordinal) -or
                $relativeProjectPath.StartsWith("..\", [StringComparison]::Ordinal)) {
                throw "Leaf route '$name' testProject must remain inside the repository."
            }
            if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
                throw "Leaf route '$name' references missing project '$($route.testProject)'."
            }
            $hasFilter = $properties -contains 'filter' -and
                -not [string]::IsNullOrWhiteSpace([string]$route.filter)
            if (-not $hasFilter -and $name -ine 'Full') {
                throw "Leaf route '$name' requires a filter; only Full may omit it."
            }
            $timeout = 0
            if (-not [int]::TryParse([string]$route.defaultTimeoutSeconds, [ref]$timeout) -or
                $timeout -le 0 -or
                $timeout -gt $maximumTimeoutSeconds) {
                throw "Leaf route '$name' has an invalid timeout; use 1..$maximumTimeoutSeconds seconds."
            }
        }
        else {
            foreach ($forbidden in @('filter', 'testProject', 'defaultTimeoutSeconds')) {
                if ($properties -contains $forbidden) {
                    throw "Composite route '$name' cannot contain $forbidden."
                }
            }
            $children = @($route.children)
            if ($children.Count -eq 0) {
                throw "Composite route '$name' requires children."
            }
            foreach ($child in $children) {
                if (-not $byName.ContainsKey([string]$child)) {
                    throw "Composite route '$name' references missing child '$child'."
                }
            }
        }
    }

    $visiting = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $visited = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    function Visit-Route {
        param([string]$Name)

        if ($visiting.Contains($Name)) {
            throw "Test route graph contains a cycle at '$Name'."
        }
        if ($visited.Contains($Name)) {
            return
        }

        [void]$visiting.Add($Name)
        $route = $byName[$Name]
        if ([string]$route.kind -eq 'composite') {
            foreach ($child in @($route.children)) {
                Visit-Route -Name ([string]$child)
            }
        }
        [void]$visiting.Remove($Name)
        [void]$visited.Add($Name)
    }

    foreach ($route in $routes) {
        Visit-Route -Name ([string]$route.name)
    }

    foreach ($route in $routes | Where-Object { $_.kind -eq 'composite' -and -not $_.milestoneOnly }) {
        $plan = Resolve-TestRoutePlan -Registry $registry -RouteName ([string]$route.name) -SkipValidation
        $milestoneChild = $plan | Where-Object { $_.milestoneOnly } | Select-Object -First 1
        if ($null -ne $milestoneChild) {
            throw "Ordinary composite '$($route.name)' hides milestone route '$($milestoneChild.name)'."
        }
    }

    return $registry
}

function Resolve-TestRoutePlan {
    param(
        [Parameter(Mandatory)]$Registry,
        [Parameter(Mandatory)][string]$RouteName,
        [switch]$SkipValidation
    )

    $byName = [Collections.Generic.Dictionary[string, object]]::new(
        [StringComparer]::OrdinalIgnoreCase)
    foreach ($route in @($Registry.routes)) {
        $byName[[string]$route.name] = $route
    }
    if (-not $byName.ContainsKey($RouteName)) {
        $supported = (@($Registry.routes.name) | Sort-Object) -join ', '
        throw "Unknown test route '$RouteName'. Supported routes: $supported"
    }

    $leaves = [Collections.Generic.List[object]]::new()
    $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    function Expand-Route {
        param([string]$Name)

        $route = $byName[$Name]
        if ([string]$route.kind -eq 'leaf') {
            if (-not $seen.Add([string]$route.name)) {
                throw "Route '$RouteName' resolves leaf '$($route.name)' more than once."
            }
            $leaves.Add($route)
            return
        }
        foreach ($child in @($route.children)) {
            Expand-Route -Name ([string]$child)
        }
    }

    Expand-Route -Name $RouteName
    return @($leaves)
}

function Read-UiSmokeCatalog {
    param(
        [string]$Root,
        [string]$CatalogPath = 'eng/ui-smoke-cases.json'
    )

    $absolutePath = if ([IO.Path]::IsPathRooted($CatalogPath)) {
        $CatalogPath
    }
    else {
        Join-Path $Root $CatalogPath
    }
    if (-not (Test-Path -LiteralPath $absolutePath -PathType Leaf)) {
        throw "UI smoke catalog does not exist: $CatalogPath"
    }
    try {
        $catalog = Get-Content -LiteralPath $absolutePath -Raw | ConvertFrom-Json
    }
    catch {
        throw "UI smoke catalog is not valid JSON: $($_.Exception.Message)"
    }
    if ($catalog.schemaVersion -ne 1) {
        throw 'Only UI smoke catalog schemaVersion 1 is supported.'
    }

    $cases = @($catalog.cases)
    if ($cases.Count -eq 0) {
        throw 'UI smoke catalog must contain at least one case.'
    }
    $ids = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $screenshotsByEvidence = @{}
    foreach ($case in $cases) {
        foreach ($field in @('id', 'title', 'area', 'screenshotFileName')) {
            if ([string]::IsNullOrWhiteSpace([string]$case.$field)) {
                throw "Every UI smoke case requires non-empty '$field'."
            }
        }
        if (-not $ids.Add([string]$case.id)) {
            throw "Duplicate UI smoke case id '$($case.id)'."
        }
        foreach ($field in @('requiredFor', 'actions', 'expected', 'themes', 'languages', 'windowModes')) {
            if (@($case.$field).Count -eq 0) {
                throw "UI smoke case '$($case.id)' requires non-empty '$field'."
            }
        }
        foreach ($theme in @($case.themes)) {
            if ([string]$theme -notin @('dark', 'light')) {
                throw "UI smoke case '$($case.id)' has unknown theme '$theme'."
            }
        }
        foreach ($language in @($case.languages)) {
            if ([string]$language -notin @('ru', 'en')) {
                throw "UI smoke case '$($case.id)' has unknown language '$language'."
            }
        }
        if ($case.trackScreenshot -ne $false) {
            throw "UI smoke case '$($case.id)' must set trackScreenshot to false."
        }

        $fileName = [string]$case.screenshotFileName
        if ([IO.Path]::IsPathRooted($fileName) -or
            $fileName -ne [IO.Path]::GetFileName($fileName) -or
            $fileName -match '\(\d+\)') {
            throw "UI smoke case '$($case.id)' has an unsafe screenshot filename '$fileName'."
        }

        foreach ($evidenceSet in @($case.requiredFor)) {
            $key = [string]$evidenceSet
            if (-not $screenshotsByEvidence.ContainsKey($key)) {
                $screenshotsByEvidence[$key] = [Collections.Generic.HashSet[string]]::new(
                    [StringComparer]::OrdinalIgnoreCase)
            }
            if (-not $screenshotsByEvidence[$key].Add($fileName)) {
                throw "UI smoke screenshot '$fileName' is duplicated in evidence set '$key'."
            }
        }
    }

    return $catalog
}

Export-ModuleMember -Function @(
    'Get-RepositoryRoot',
    'Get-RelativeRepositoryPath',
    'Read-TestRouteRegistry',
    'Resolve-TestRoutePlan',
    'Read-UiSmokeCatalog'
)
