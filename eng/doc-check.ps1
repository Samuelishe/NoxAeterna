[CmdletBinding()]
param(
    [switch]$Json,
    [string]$Root
)

$ErrorActionPreference = 'Stop'

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

function New-Diagnostic {
    param(
        [string]$Code,
        [string]$Path,
        [string]$Message
    )

    [ordered]@{
        code = $Code
        path = $Path
        message = $Message
    }
}

function Get-RelativePath {
    param(
        [string]$BasePath,
        [string]$TargetPath
    )

    [IO.Path]::GetRelativePath($BasePath, $TargetPath).Replace('\', '/')
}

function Test-Metadata {
    param(
        [string]$Content,
        [string]$Label
    )

    $escaped = [Regex]::Escape($Label)
    return [Regex]::IsMatch(
        $Content,
        "(?im)(?:^|\|)\s*(?:\*\*)?$escaped(?:\*\*)?\s*(?:\||:)")
}

function Get-MarkdownDestinations {
    param([string]$Content)

    $withoutFences = [Regex]::Replace(
        $Content,
        '(?ms)^[ \t]*```.*?^[ \t]*```[ \t]*$',
        '')
    $matches = [Regex]::Matches(
        $withoutFences,
        '!?\[[^\]]*\]\((?<destination>[^)\r\n]+)\)')

    foreach ($match in $matches) {
        $raw = $match.Groups['destination'].Value.Trim()
        if ($raw.StartsWith('<', [StringComparison]::Ordinal) -and $raw.Contains('>')) {
            $raw = $raw.Substring(1, $raw.IndexOf('>') - 1)
        }
        else {
            $raw = ($raw -split '\s+')[0]
        }

        if (-not [string]::IsNullOrWhiteSpace($raw)) {
            $raw
        }
    }
}

function Get-SessionLogHeadings {
    param([string]$Content)

    foreach ($match in [Regex]::Matches(
            $Content,
            '(?im)^##\s+(?<date>\d{4}-\d{2}-\d{2})\s*:\s*(?<title>[^\r\n]+?)\s*$')) {
        $title = [Regex]::Replace($match.Groups['title'].Value.Trim(), '\s+', ' ')
        [ordered]@{
            date = $match.Groups['date'].Value
            canonical = "## $($match.Groups['date'].Value): $title"
        }
    }
}

$warnings = [Collections.Generic.List[object]]::new()
$errors = [Collections.Generic.List[object]]::new()
$measurements = [Collections.Generic.List[object]]::new()
$brokenLinks = [Collections.Generic.List[object]]::new()
$archiveDiagnostics = [Collections.Generic.List[object]]::new()
$routeDiagnostics = [Collections.Generic.List[object]]::new()
$uiSmokeDiagnostics = [Collections.Generic.List[object]]::new()
$repositoryRoot = $null
$manifest = $null

try {
    $repositoryRoot = Get-RepositoryRoot -RequestedRoot $Root
}
catch {
    $errors.Add((New-Diagnostic 'root.invalid' '' $_.Exception.Message))
}

if ($null -ne $repositoryRoot) {
    $manifestPath = Join-Path $repositoryRoot 'eng/document-budgets.json'
    if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
        $errors.Add((New-Diagnostic 'manifest.missing' 'eng/document-budgets.json' 'Budget manifest does not exist.'))
    }
    else {
        try {
            $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
        }
        catch {
            $errors.Add((New-Diagnostic 'manifest.invalid-json' 'eng/document-budgets.json' $_.Exception.Message))
        }
    }
}

if ($null -ne $manifest) {
    if ($manifest.schemaVersion -ne 1) {
        $errors.Add((New-Diagnostic 'manifest.schema-version' 'eng/document-budgets.json' 'Only schemaVersion 1 is supported.'))
    }

    $warningRatio = 0d
    if (-not [double]::TryParse(
            [string]$manifest.warningRatio,
            [Globalization.NumberStyles]::Float,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$warningRatio) -or
        $warningRatio -le 0d -or
        $warningRatio -ge 1d) {
        $errors.Add((New-Diagnostic 'manifest.warning-ratio' 'eng/document-budgets.json' 'warningRatio must be greater than 0 and less than 1.'))
    }

    $documents = @($manifest.documents)
    if ($documents.Count -eq 0) {
        $errors.Add((New-Diagnostic 'manifest.documents-empty' 'eng/document-budgets.json' 'documents must contain at least one entry.'))
    }

    $seenPaths = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach ($document in ($documents | Sort-Object -Property path)) {
        $path = [string]$document.path
        if ([string]::IsNullOrWhiteSpace($path)) {
            $errors.Add((New-Diagnostic 'manifest.path-empty' 'eng/document-budgets.json' 'Every document entry requires a path.'))
            continue
        }

        if (-not $seenPaths.Add($path)) {
            $errors.Add((New-Diagnostic 'manifest.path-duplicate' $path 'Document paths must be unique.'))
        }

        $hardLimit = 0
        if (-not [int]::TryParse([string]$document.hardLimit, [ref]$hardLimit) -or $hardLimit -le 0) {
            $errors.Add((New-Diagnostic 'manifest.hard-limit' $path 'hardLimit must be a positive integer.'))
        }

        $strategy = [string]$document.overflowStrategy
        if ($strategy -notin @('manual-reconcile', 'rollover-archive')) {
            $errors.Add((New-Diagnostic 'manifest.strategy' $path "Unknown overflow strategy '$strategy'."))
        }

        $hasArchiveDestination = $document.PSObject.Properties.Name -contains 'archiveDestination' -and
            -not [string]::IsNullOrWhiteSpace([string]$document.archiveDestination)
        if ($strategy -eq 'rollover-archive' -and -not $hasArchiveDestination) {
            $errors.Add((New-Diagnostic 'manifest.archive-required' $path 'rollover-archive requires archiveDestination.'))
        }
        elseif ($strategy -ne 'rollover-archive' -and $hasArchiveDestination) {
            $errors.Add((New-Diagnostic 'manifest.archive-unexpected' $path 'archiveDestination is valid only for rollover-archive.'))
        }

        $absoluteDocumentPath = Join-Path $repositoryRoot $path
        if (-not (Test-Path -LiteralPath $absoluteDocumentPath -PathType Leaf)) {
            $errors.Add((New-Diagnostic 'manifest.document-missing' $path 'Referenced active document does not exist.'))
            continue
        }

        if ($strategy -eq 'rollover-archive' -and $hasArchiveDestination) {
            $archivePath = Join-Path $repositoryRoot ([string]$document.archiveDestination)
            if (-not (Test-Path -LiteralPath $archivePath -PathType Container)) {
                $errors.Add((New-Diagnostic 'manifest.archive-missing' ([string]$document.archiveDestination) 'Archive destination does not exist.'))
            }
        }

        if ($hardLimit -gt 0 -and $warningRatio -gt 0d -and $warningRatio -lt 1d) {
            $characterCount = (Get-Content -LiteralPath $absoluteDocumentPath -Raw).Length
            $softLimit = [int][Math]::Floor($hardLimit * $warningRatio)
            $status = 'ok'
            if ($characterCount -gt $hardLimit) {
                $status = 'error'
                $errors.Add((New-Diagnostic 'budget.hard-overflow' $path "$characterCount characters exceed hard limit $hardLimit."))
            }
            elseif ($characterCount -ge $softLimit) {
                $status = 'warning'
                $warnings.Add((New-Diagnostic 'budget.soft-overflow' $path "$characterCount characters reached soft threshold $softLimit of hard limit $hardLimit."))
            }

            $measurements.Add([ordered]@{
                path = $path.Replace('\', '/')
                characterCount = $characterCount
                softLimit = $softLimit
                hardLimit = $hardLimit
                status = $status
            })
        }
    }
}

if ($null -ne $repositoryRoot) {
    $requiredDocuments = @(
        'AGENTS.md',
        'docs/PROJECT-STATE.md',
        'docs/PROJECT-STATS.md',
        'docs/DOCUMENTATION-GOVERNANCE.md',
        'docs/INDEX.md',
        'docs/SESSION-LOG.md',
        'docs/archive/README.md',
        'docs/TEST-EXECUTION.md',
        'docs/UI-SMOKE.md'
    )
    foreach ($path in $requiredDocuments) {
        if (-not (Test-Path -LiteralPath (Join-Path $repositoryRoot $path) -PathType Leaf)) {
            $errors.Add((New-Diagnostic 'documentation.required' $path 'Required documentation file does not exist.'))
        }
    }

    $metadataDocuments = @(
        'AGENTS.md',
        'docs/AGENTS.md',
        'docs/PROJECT-STATE.md',
        'docs/PROJECT-STATS.md',
        'docs/DOCUMENTATION-GOVERNANCE.md',
        'docs/VISUAL-DESIGN-SYSTEM.md',
        'docs/THEMES.md',
        'docs/TEST-EXECUTION.md',
        'docs/UI-SMOKE.md',
        'docs/archive/README.md',
        'eng/README.md'
    )
    foreach ($path in $metadataDocuments) {
        $absolutePath = Join-Path $repositoryRoot $path
        if (-not (Test-Path -LiteralPath $absolutePath -PathType Leaf)) {
            continue
        }

        $content = Get-Content -LiteralPath $absolutePath -Raw
        foreach ($label in @('Role', 'Read when', 'Authoritative for', 'Not authoritative for')) {
            if (-not (Test-Metadata -Content $content -Label $label)) {
                $errors.Add((New-Diagnostic 'metadata.missing' $path "Required metadata '$label' is missing."))
            }
        }
    }

    $projectStatePath = Join-Path $repositoryRoot 'docs/PROJECT-STATE.md'
    if (Test-Path -LiteralPath $projectStatePath -PathType Leaf) {
        $projectState = Get-Content -LiteralPath $projectStatePath -Raw
        foreach ($heading in @('Current Checkpoint', 'Current Focus', 'Preserved Contracts', 'Active Blockers')) {
            if (-not [Regex]::IsMatch($projectState, "(?im)^##\s+$([Regex]::Escape($heading))\s*$")) {
                $errors.Add((New-Diagnostic 'project-state.heading' 'docs/PROJECT-STATE.md' "Required heading '$heading' is missing."))
            }
        }
    }

    $verificationModulePath = Join-Path $repositoryRoot 'eng/RepoVerification.psm1'
    if (-not (Test-Path -LiteralPath $verificationModulePath -PathType Leaf)) {
        $errors.Add((New-Diagnostic 'verification.module-missing' 'eng/RepoVerification.psm1' 'Shared repository verification module does not exist.'))
    }
    else {
        try {
            Import-Module $verificationModulePath -Force
            $routeRegistry = Read-TestRouteRegistry -Root $repositoryRoot -RegistryPath 'eng/test-routes.json'
            foreach ($route in @($routeRegistry.routes | Sort-Object -Property name)) {
                $routeDiagnostics.Add([pscustomobject][ordered]@{
                    name = [string]$route.name
                    kind = [string]$route.kind
                    milestoneOnly = [bool]$route.milestoneOnly
                })
            }
        }
        catch {
            $errors.Add((New-Diagnostic 'test-routes.invalid' 'eng/test-routes.json' $_.Exception.Message))
        }

        try {
            $uiSmokeCatalog = Read-UiSmokeCatalog -Root $repositoryRoot
            foreach ($case in @($uiSmokeCatalog.cases | Sort-Object -Property id)) {
                $uiSmokeDiagnostics.Add([pscustomobject][ordered]@{
                    id = [string]$case.id
                    screenshotFileName = [string]$case.screenshotFileName
                })
            }
        }
        catch {
            $errors.Add((New-Diagnostic 'ui-smoke.invalid' 'eng/ui-smoke-cases.json' $_.Exception.Message))
        }
    }

    $ciWorkflowPath = Join-Path $repositoryRoot '.github/workflows/ci.yml'
    if (-not (Test-Path -LiteralPath $ciWorkflowPath -PathType Leaf)) {
        $errors.Add((New-Diagnostic 'ci.workflow-missing' '.github/workflows/ci.yml' 'CI workflow does not exist.'))
    }

    $testProjectPath = Join-Path $repositoryRoot 'NoxAeterna.Tests/NoxAeterna.Tests.csproj'
    if (-not (Test-Path -LiteralPath $testProjectPath -PathType Leaf) -or
        (Get-Content -LiteralPath $testProjectPath -Raw) -notmatch 'PackageReference\s+Include="coverlet\.collector"') {
        $errors.Add((New-Diagnostic 'coverage.collector-missing' 'NoxAeterna.Tests/NoxAeterna.Tests.csproj' 'coverlet.collector is not declared.'))
    }

    $excludedDirectoryNames = @('.git', 'bin', 'obj', 'TestResults', '.codex-cache')
    $markdownFiles = Get-ChildItem -LiteralPath $repositoryRoot -Filter '*.md' -File -Recurse |
        Where-Object {
            $relative = Get-RelativePath -BasePath $repositoryRoot -TargetPath $_.FullName
            -not ($excludedDirectoryNames | Where-Object { $relative -match "(^|/)$([Regex]::Escape($_))(/|$)" })
        } |
        Sort-Object -Property FullName

    foreach ($file in $markdownFiles) {
        $relativeSource = Get-RelativePath -BasePath $repositoryRoot -TargetPath $file.FullName
        $content = Get-Content -LiteralPath $file.FullName -Raw
        foreach ($destination in (Get-MarkdownDestinations -Content $content)) {
            if ($destination -match '^(?i:https?|mailto):' -or
                $destination.StartsWith('#', [StringComparison]::Ordinal) -or
                $destination -match '^[a-zA-Z][a-zA-Z0-9+.-]*://') {
                continue
            }

            $localTarget = ($destination -split '[?#]', 2)[0]
            if ([string]::IsNullOrWhiteSpace($localTarget)) {
                continue
            }

            try {
                $localTarget = [Uri]::UnescapeDataString($localTarget)
                $absoluteTarget = if ($localTarget.StartsWith('/', [StringComparison]::Ordinal)) {
                    Join-Path $repositoryRoot $localTarget.TrimStart('/')
                }
                else {
                    Join-Path $file.DirectoryName $localTarget
                }
                $absoluteTarget = [IO.Path]::GetFullPath($absoluteTarget)
                if (-not (Test-Path -LiteralPath $absoluteTarget)) {
                    $diagnostic = New-Diagnostic 'markdown.broken-link' $relativeSource "Missing local target '$destination'."
                    $errors.Add($diagnostic)
                    $brokenLinks.Add($diagnostic)
                }
            }
            catch {
                $diagnostic = New-Diagnostic 'markdown.invalid-link' $relativeSource "Invalid local target '$destination'."
                $errors.Add($diagnostic)
                $brokenLinks.Add($diagnostic)
            }
        }
    }

    $archiveIndexPath = Join-Path $repositoryRoot 'docs/archive/README.md'
    $sessionArchivePath = Join-Path $repositoryRoot 'docs/archive/session-log'
    $ranges = [Collections.Generic.List[object]]::new()
    $partialChunks = [Collections.Generic.List[object]]::new()
    $seenPartialKeys = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $archiveHeadingOwners = [Collections.Generic.Dictionary[string, string]]::new([StringComparer]::Ordinal)
    if (Test-Path -LiteralPath $sessionArchivePath -PathType Container) {
        $archiveIndex = if (Test-Path -LiteralPath $archiveIndexPath -PathType Leaf) {
            Get-Content -LiteralPath $archiveIndexPath -Raw
        } else { '' }

        foreach ($chunk in (Get-ChildItem -LiteralPath $sessionArchivePath -Filter 'SESSION-LOG_*.md' -File | Sort-Object -Property Name)) {
            $relativeChunkPath = Get-RelativePath $repositoryRoot $chunk.FullName
            $fullRangeMatch = [Regex]::Match(
                $chunk.Name,
                '^SESSION-LOG_(?<start>\d{4}-\d{2}-\d{2})_to_(?<end>\d{4}-\d{2}-\d{2})\.md$')
            $partialMatch = [Regex]::Match(
                $chunk.Name,
                '^SESSION-LOG_(?<date>\d{4}-\d{2}-\d{2})_part-(?<part>\d{2})\.md$')
            if (-not $fullRangeMatch.Success -and -not $partialMatch.Success) {
                $errors.Add((New-Diagnostic 'archive.filename' $relativeChunkPath 'Session archive filename is neither a parseable full range nor a partial-day chunk.'))
                continue
            }

            $indexReferenceCount = [Regex]::Matches(
                $archiveIndex,
                [Regex]::Escape($chunk.Name),
                [Text.RegularExpressions.RegexOptions]::IgnoreCase).Count
            if ($indexReferenceCount -eq 0) {
                $errors.Add((New-Diagnostic 'archive.not-indexed' $relativeChunkPath 'Archive chunk is not indexed by docs/archive/README.md.'))
            }

            $chunkContent = Get-Content -LiteralPath $chunk.FullName -Raw
            $chunkHeadings = @(Get-SessionLogHeadings -Content $chunkContent)
            foreach ($heading in $chunkHeadings) {
                if ($archiveHeadingOwners.ContainsKey($heading.canonical)) {
                    $errors.Add((New-Diagnostic 'archive.heading-duplicate' $relativeChunkPath "Session heading '$($heading.canonical)' is already owned by '$($archiveHeadingOwners[$heading.canonical])'."))
                }
                else {
                    $archiveHeadingOwners.Add($heading.canonical, $relativeChunkPath)
                }
            }

            if ($fullRangeMatch.Success) {
                $start = [DateTime]::MinValue
                $end = [DateTime]::MinValue
                $startValid = [DateTime]::TryParseExact(
                    $fullRangeMatch.Groups['start'].Value,
                    'yyyy-MM-dd',
                    [Globalization.CultureInfo]::InvariantCulture,
                    [Globalization.DateTimeStyles]::None,
                    [ref]$start)
                $endValid = [DateTime]::TryParseExact(
                    $fullRangeMatch.Groups['end'].Value,
                    'yyyy-MM-dd',
                    [Globalization.CultureInfo]::InvariantCulture,
                    [Globalization.DateTimeStyles]::None,
                    [ref]$end)
                if (-not $startValid -or -not $endValid) {
                    $errors.Add((New-Diagnostic 'archive.filename' $relativeChunkPath 'Full-range archive filename contains an invalid calendar date.'))
                    continue
                }
                if ($end -lt $start) {
                    $errors.Add((New-Diagnostic 'archive.range-order' $relativeChunkPath 'Archive end date precedes its start date.'))
                    continue
                }

                $ranges.Add([ordered]@{
                    kind = 'full-range'
                    path = $relativeChunkPath
                    start = $start
                    end = $end
                })
                continue
            }

            $partialDate = [DateTime]::MinValue
            if (-not [DateTime]::TryParseExact(
                    $partialMatch.Groups['date'].Value,
                    'yyyy-MM-dd',
                    [Globalization.CultureInfo]::InvariantCulture,
                    [Globalization.DateTimeStyles]::None,
                    [ref]$partialDate)) {
                $errors.Add((New-Diagnostic 'archive.filename' $relativeChunkPath 'Partial-day archive filename contains an invalid calendar date.'))
                continue
            }

            $part = [int]$partialMatch.Groups['part'].Value
            if ($part -lt 1) {
                $errors.Add((New-Diagnostic 'archive.partial-part' $relativeChunkPath 'Partial-day part number must be a positive two-digit number.'))
            }

            $partialKey = "$($partialDate.ToString('yyyy-MM-dd'))/$part"
            if (-not $seenPartialKeys.Add($partialKey) -or $indexReferenceCount -gt 1) {
                $errors.Add((New-Diagnostic 'archive.partial-duplicate' $relativeChunkPath "Partial-day date/part '$partialKey' must be unique and indexed exactly once."))
            }

            foreach ($heading in $chunkHeadings) {
                if ($heading.date -ne $partialDate.ToString('yyyy-MM-dd')) {
                    $errors.Add((New-Diagnostic 'archive.partial-heading-date' $relativeChunkPath "Session heading '$($heading.canonical)' does not match partial-day date $($partialDate.ToString('yyyy-MM-dd'))."))
                }
            }

            $partialChunks.Add([ordered]@{
                kind = 'partial-day'
                path = $relativeChunkPath
                date = $partialDate
                part = $part
            })
        }
    }

    $orderedRanges = @($ranges | Sort-Object -Property @{ Expression = { $_.start } }, @{ Expression = { $_.end } })
    for ($index = 1; $index -lt $orderedRanges.Count; $index++) {
        if ($orderedRanges[$index].start -le $orderedRanges[$index - 1].end) {
            $errors.Add((New-Diagnostic 'archive.overlap' $orderedRanges[$index].path "Archive range overlaps '$($orderedRanges[$index - 1].path)'."))
        }
    }

    $orderedPartials = @($partialChunks | Sort-Object -Property @{ Expression = { $_.date } }, @{ Expression = { $_.part } })
    foreach ($group in ($orderedPartials | Group-Object -Property { $_.date.ToString('yyyy-MM-dd') })) {
        $expectedPart = 1
        foreach ($partial in @($group.Group | Sort-Object -Property part)) {
            if ($partial.part -ne $expectedPart) {
                $errors.Add((New-Diagnostic 'archive.partial-sequence' $partial.path "Partial-day parts for $($group.Name) must be contiguous from 01; expected part $($expectedPart.ToString('00'))."))
                $expectedPart = $partial.part + 1
            }
            else {
                $expectedPart++
            }
        }
    }

    foreach ($partial in $orderedPartials) {
        foreach ($range in $orderedRanges) {
            if ($partial.date -ge $range.start -and $partial.date -le $range.end) {
                $errors.Add((New-Diagnostic 'archive.partial-full-overlap' $partial.path "Partial-day chunk falls inside full archive range '$($range.path)'."))
            }
        }
    }

    $activeSessionPath = Join-Path $repositoryRoot 'docs/SESSION-LOG.md'
    if (Test-Path -LiteralPath $activeSessionPath -PathType Leaf) {
        $activeContent = Get-Content -LiteralPath $activeSessionPath -Raw
        $activeHeadings = @(Get-SessionLogHeadings -Content $activeContent)
        $seenActiveHeadings = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($heading in $activeHeadings) {
            $date = [DateTime]::ParseExact($heading.date, 'yyyy-MM-dd', [Globalization.CultureInfo]::InvariantCulture)
            foreach ($range in $orderedRanges) {
                if ($date -ge $range.start -and $date -le $range.end) {
                    $errors.Add((New-Diagnostic 'archive.active-overlap' 'docs/SESSION-LOG.md' "Active heading $($date.ToString('yyyy-MM-dd')) overlaps '$($range.path)'."))
                }
            }

            if (-not $seenActiveHeadings.Add($heading.canonical)) {
                $errors.Add((New-Diagnostic 'archive.heading-duplicate' 'docs/SESSION-LOG.md' "Session heading '$($heading.canonical)' appears more than once in the active log."))
            }
            elseif ($archiveHeadingOwners.ContainsKey($heading.canonical)) {
                $errors.Add((New-Diagnostic 'archive.heading-duplicate' 'docs/SESSION-LOG.md' "Session heading '$($heading.canonical)' is already owned by '$($archiveHeadingOwners[$heading.canonical])'."))
            }
        }
    }

    foreach ($range in $orderedRanges) {
        $archiveDiagnostics.Add([ordered]@{
            kind = $range.kind
            path = $range.path
            start = $range.start.ToString('yyyy-MM-dd')
            end = $range.end.ToString('yyyy-MM-dd')
        })
    }
    foreach ($partial in $orderedPartials) {
        $archiveDiagnostics.Add([ordered]@{
            kind = $partial.kind
            path = $partial.path
            date = $partial.date.ToString('yyyy-MM-dd')
            part = $partial.part
        })
    }
}

$orderedWarnings = @($warnings | Sort-Object -Property path, code, message)
$orderedErrors = @($errors | Sort-Object -Property path, code, message)
$orderedMeasurements = @($measurements | Sort-Object -Property path)
$orderedBrokenLinks = @($brokenLinks | Sort-Object -Property path, message)
$result = if ($orderedErrors.Count -gt 0) { 'error' } elseif ($orderedWarnings.Count -gt 0) { 'warning' } else { 'ok' }

$report = [ordered]@{
    schemaVersion = 1
    result = $result
    warnings = $orderedWarnings
    errors = $orderedErrors
    measuredDocuments = $orderedMeasurements
    brokenLinks = $orderedBrokenLinks
    archiveDiagnostics = @($archiveDiagnostics)
    routeDiagnostics = @($routeDiagnostics | Sort-Object -Property name)
    uiSmokeDiagnostics = @($uiSmokeDiagnostics | Sort-Object -Property id)
}

if ($Json) {
    $report | ConvertTo-Json -Depth 8
}
else {
    foreach ($measurement in $orderedMeasurements) {
        Write-Host ("{0}: {1} {2}/{3}" -f $measurement.status.ToUpperInvariant(), $measurement.path, $measurement.characterCount, $measurement.hardLimit)
    }
    foreach ($warning in $orderedWarnings) {
        Write-Host ("WARNING: {0}: {1}" -f $warning.path, $warning.message)
    }
    foreach ($diagnosticItem in $orderedErrors) {
        Write-Host ("ERROR: {0}: {1}" -f $diagnosticItem.path, $diagnosticItem.message)
    }
    Write-Host ("Summary: {0} warning(s), {1} error(s)." -f $orderedWarnings.Count, $orderedErrors.Count)
}

if ($orderedErrors.Count -gt 0) {
    exit 1
}

exit 0
