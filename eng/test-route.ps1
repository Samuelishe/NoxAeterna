[CmdletBinding()]
param(
    [Parameter(Position = 0, Mandatory)]
    [ValidateSet('list', 'resolve', 'run')]
    [string]$Command,
    [Parameter(Position = 1)]
    [string]$Name,
    [switch]$Json,
    [switch]$NoBuild,
    [switch]$DryRun,
    [switch]$AllowMilestone,
    [string]$Configuration = 'Debug',
    [string]$Root,
    [string]$Registry = 'eng/test-routes.json'
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'RepoVerification.psm1') -Force

function New-LeafArguments {
    param(
        $Leaf,
        [string]$ResultsDirectory,
        [string]$ConfigurationName,
        [bool]$UseNoBuild
    )

    $arguments = [Collections.Generic.List[string]]::new()
    foreach ($argument in @('test', [string]$Leaf.testProject, '-c', $ConfigurationName)) {
        $arguments.Add($argument)
    }
    if ($UseNoBuild) {
        $arguments.Add('--no-build')
    }
    if ($Leaf.PSObject.Properties.Name -contains 'filter') {
        $arguments.Add('--filter')
        $arguments.Add([string]$Leaf.filter)
    }
    $arguments.Add('--logger')
    $arguments.Add("trx;LogFileName=$($Leaf.name).trx")
    $arguments.Add('--results-directory')
    $arguments.Add($ResultsDirectory)
    return @($arguments)
}

function Convert-ToRelativePath {
    param([string]$RepositoryRoot, [string]$Path)
    Get-RelativeRepositoryPath -Root $RepositoryRoot -Path $Path
}

try {
    $repositoryRoot = Get-RepositoryRoot -RequestedRoot $Root
    $registryData = Read-TestRouteRegistry -Root $repositoryRoot -RegistryPath $Registry
}
catch {
    if ($Json) {
        [ordered]@{
            schemaVersion = 1
            succeeded = $false
            errors = @($_.Exception.Message)
        } | ConvertTo-Json -Depth 8
    }
    else {
        Write-Error $_.Exception.Message
    }
    exit 2
}

if ($Command -eq 'list') {
    $listed = @($registryData.routes | Sort-Object -Property name | ForEach-Object {
        [ordered]@{
            name = [string]$_.name
            kind = [string]$_.kind
            category = [string]$_.category
            milestoneOnly = [bool]$_.milestoneOnly
            description = [string]$_.description
        }
    })
    if ($Json) {
        [ordered]@{ schemaVersion = 1; routes = $listed } | ConvertTo-Json -Depth 6
    }
    else {
        foreach ($route in $listed) {
            Write-Host ("{0} [{1}]{2} - {3}" -f
                $route.name,
                $route.kind,
                $(if ($route.milestoneOnly) { ' [milestone]' } else { '' }),
                $route.description)
        }
    }
    exit 0
}

if ([string]::IsNullOrWhiteSpace($Name)) {
    Write-Error "$Command requires a route name."
    exit 2
}

try {
    $plan = @(Resolve-TestRoutePlan -Registry $registryData -RouteName $Name)
    $requested = @($registryData.routes |
        Where-Object { $_.name -ieq $Name } |
        Select-Object -First 1)[0]
}
catch {
    if ($Json) {
        [ordered]@{
            schemaVersion = 1
            requestedRoute = $Name
            succeeded = $false
            errors = @($_.Exception.Message)
        } | ConvertTo-Json -Depth 8
    }
    else {
        Write-Error $_.Exception.Message
    }
    exit 2
}

$containsMilestone = [bool]($requested.milestoneOnly -or @($plan | Where-Object milestoneOnly).Count -gt 0)
$runId = if ($Command -eq 'resolve' -or $DryRun) {
    '_run-id_'
}
else {
    "{0}_{1}_{2}" -f
        (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssfffZ'),
        ([Diagnostics.Process]::GetCurrentProcess().Id),
        ([Guid]::NewGuid().ToString('N').Substring(0, 8))
}
$relativeResultsDirectory = "TestResults/RepoRoutes/$runId"
$resultsDirectory = Join-Path $repositoryRoot $relativeResultsDirectory
$resolved = @($plan | ForEach-Object {
    $arguments = New-LeafArguments `
        -Leaf $_ `
        -ResultsDirectory $relativeResultsDirectory `
        -ConfigurationName $Configuration `
        -UseNoBuild $NoBuild.IsPresent
    [ordered]@{
        name = [string]$_.name
        timeoutSeconds = [int]$_.defaultTimeoutSeconds
        command = 'dotnet'
        arguments = $arguments
    }
})

if ($Command -eq 'resolve' -or $DryRun) {
    $resolution = [ordered]@{
        schemaVersion = 1
        requestedRoute = [string]$requested.name
        resolvedLeaves = @($plan.name)
        milestoneAuthorized = [bool]($AllowMilestone -and $containsMilestone)
        noBuild = [bool]$NoBuild
        configuration = $Configuration
        dryRun = [bool]$DryRun
        plan = $resolved
    }
    if ($Json) {
        $resolution | ConvertTo-Json -Depth 10
    }
    else {
        Write-Host ("Route: {0}" -f $resolution.requestedRoute)
        foreach ($leaf in $resolved) {
            Write-Host ("  {0}: dotnet {1}" -f $leaf.name, ($leaf.arguments -join ' '))
        }
    }
    exit 0
}

if ($containsMilestone -and -not $AllowMilestone) {
    $message = "Route '$($requested.name)' is milestone-only. Re-run with -AllowMilestone."
    if ($Json) {
        [ordered]@{
            schemaVersion = 1
            requestedRoute = [string]$requested.name
            resolvedLeaves = @($plan.name)
            succeeded = $false
            timedOut = $false
            duration = 0
            children = @()
            logPaths = @()
            exitCode = 3
            milestoneAuthorized = $false
            errors = @($message)
        } | ConvertTo-Json -Depth 8
    }
    else {
        Write-Error $message
    }
    exit 3
}

New-Item -ItemType Directory -Path $resultsDirectory -Force | Out-Null
$stopwatch = [Diagnostics.Stopwatch]::StartNew()
$children = [Collections.Generic.List[object]]::new()
$logPaths = [Collections.Generic.List[string]]::new()
$overallExitCode = 0
$anyTimeout = $false

foreach ($entry in $resolved) {
    $leafStopwatch = [Diagnostics.Stopwatch]::StartNew()
    $logPath = Join-Path $resultsDirectory "$($entry.name).log"
    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = 'dotnet'
    $startInfo.WorkingDirectory = $repositoryRoot
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    foreach ($argument in $entry.arguments) {
        $startInfo.ArgumentList.Add([string]$argument)
    }

    if (-not $Json) {
        Write-Host ("Running {0} (timeout {1}s)..." -f $entry.name, $entry.timeoutSeconds)
    }
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    [void]$process.Start()
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    $completed = $process.WaitForExit([int]$entry.timeoutSeconds * 1000)
    $timedOut = -not $completed
    if ($timedOut) {
        $process.Kill($true)
        $process.WaitForExit()
    }
    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()
    $logContent = $stdout
    if (-not [string]::IsNullOrWhiteSpace($stderr)) {
        $logContent += [Environment]::NewLine + $stderr
    }
    [IO.File]::WriteAllText($logPath, $logContent)
    $leafExitCode = if ($timedOut) { 124 } else { $process.ExitCode }
    $leafStopwatch.Stop()
    $relativeLogPath = Convert-ToRelativePath -RepositoryRoot $repositoryRoot -Path $logPath
    $logPaths.Add($relativeLogPath)
    $children.Add([ordered]@{
        route = $entry.name
        succeeded = [bool]($leafExitCode -eq 0)
        timedOut = $timedOut
        duration = [Math]::Round($leafStopwatch.Elapsed.TotalSeconds, 3)
        exitCode = $leafExitCode
        logPath = $relativeLogPath
    })
    if (-not $Json) {
        Write-Host ("{0}: exit {1}, {2:N2}s, {3}" -f
            $entry.name,
            $leafExitCode,
            $leafStopwatch.Elapsed.TotalSeconds,
            $relativeLogPath)
    }
    $process.Dispose()
    if ($leafExitCode -ne 0) {
        $overallExitCode = $leafExitCode
        $anyTimeout = $timedOut
        break
    }
}

$stopwatch.Stop()
$report = [ordered]@{
    schemaVersion = 1
    requestedRoute = [string]$requested.name
    resolvedLeaves = @($plan.name)
    succeeded = [bool]($overallExitCode -eq 0)
    timedOut = $anyTimeout
    duration = [Math]::Round($stopwatch.Elapsed.TotalSeconds, 3)
    children = @($children)
    logPaths = @($logPaths)
    exitCode = $overallExitCode
    milestoneAuthorized = [bool]($AllowMilestone -and $containsMilestone)
}

if ($Json) {
    $report | ConvertTo-Json -Depth 8
}
else {
    Write-Host ("Route {0}: {1} in {2:N2}s." -f
        $report.requestedRoute,
        $(if ($report.succeeded) { 'SUCCEEDED' } else { 'FAILED' }),
        $stopwatch.Elapsed.TotalSeconds)
}
exit $overallExitCode
