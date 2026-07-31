[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [switch]$NoBuild,
    [string]$Root,
    [string]$OutputDirectory,
    [switch]$Json,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'RepoVerification.psm1') -Force

try {
    $repositoryRoot = Get-RepositoryRoot -RequestedRoot $Root
    if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
        $runId = if ($DryRun) {
            '_run-id_'
        }
        else {
            "{0}_{1}_{2}" -f
                (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssfffZ'),
                ([Diagnostics.Process]::GetCurrentProcess().Id),
                ([Guid]::NewGuid().ToString('N').Substring(0, 8))
        }
        $outputPath = Join-Path $repositoryRoot "TestResults/Coverage/$runId"
    }
    elseif ([IO.Path]::IsPathRooted($OutputDirectory)) {
        $outputPath = [IO.Path]::GetFullPath($OutputDirectory)
    }
    else {
        $outputPath = [IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputDirectory))
    }

    $relativeOutput = Get-RelativeRepositoryPath -Root $repositoryRoot -Path $outputPath
    if ($relativeOutput -eq '..' -or $relativeOutput.StartsWith('../', [StringComparison]::Ordinal)) {
        throw 'Coverage output must remain inside the repository.'
    }
}
catch {
    Write-Error $_.Exception.Message
    exit 2
}

$arguments = [Collections.Generic.List[string]]::new()
foreach ($argument in @('test', 'NoxAeterna.sln', '-c', $Configuration)) {
    $arguments.Add($argument)
}
if ($NoBuild) {
    $arguments.Add('--no-build')
}
foreach ($argument in @(
        '--collect', 'XPlat Code Coverage',
        '--logger', 'trx;LogFileName=coverage.trx',
        '--results-directory', $relativeOutput,
        '--',
        'DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura')) {
    $arguments.Add($argument)
}

if ($DryRun) {
    $dryReport = [ordered]@{
        schemaVersion = 1
        dryRun = $true
        outputDirectory = $relativeOutput
        command = 'dotnet'
        arguments = @($arguments)
    }
    if ($Json) {
        $dryReport | ConvertTo-Json -Depth 6
    }
    else {
        Write-Host ("dotnet {0}" -f ($arguments -join ' '))
    }
    exit 0
}

New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
$logPath = Join-Path $outputPath 'coverage.log'
$startInfo = [Diagnostics.ProcessStartInfo]::new()
$startInfo.FileName = 'dotnet'
$startInfo.WorkingDirectory = $repositoryRoot
$startInfo.UseShellExecute = $false
$startInfo.CreateNoWindow = $true
$startInfo.RedirectStandardOutput = $true
$startInfo.RedirectStandardError = $true
foreach ($argument in $arguments) {
    $startInfo.ArgumentList.Add($argument)
}

$stopwatch = [Diagnostics.Stopwatch]::StartNew()
$process = [Diagnostics.Process]::new()
$process.StartInfo = $startInfo
[void]$process.Start()
$stdoutTask = $process.StandardOutput.ReadToEndAsync()
$stderrTask = $process.StandardError.ReadToEndAsync()
$completed = $process.WaitForExit(1200000)
$timedOut = -not $completed
if ($timedOut) {
    $process.Kill($true)
    $process.WaitForExit()
}
$stdout = $stdoutTask.GetAwaiter().GetResult()
$stderr = $stderrTask.GetAwaiter().GetResult()
$content = $stdout
if (-not [string]::IsNullOrWhiteSpace($stderr)) {
    $content += [Environment]::NewLine + $stderr
}
[IO.File]::WriteAllText($logPath, $content)
$exitCode = if ($timedOut) { 124 } else { $process.ExitCode }
$process.Dispose()
$stopwatch.Stop()

$trxFiles = @(Get-ChildItem -LiteralPath $outputPath -Filter '*.trx' -File -Recurse |
    Sort-Object -Property FullName |
    ForEach-Object { Get-RelativeRepositoryPath -Root $repositoryRoot -Path $_.FullName })
$coverageFiles = @(Get-ChildItem -LiteralPath $outputPath -Filter 'coverage.cobertura.xml' -File -Recurse |
    Sort-Object -Property FullName |
    ForEach-Object { Get-RelativeRepositoryPath -Root $repositoryRoot -Path $_.FullName })
$succeeded = $exitCode -eq 0 -and $trxFiles.Count -gt 0 -and $coverageFiles.Count -gt 0

$report = [ordered]@{
    schemaVersion = 1
    succeeded = $succeeded
    timedOut = $timedOut
    duration = [Math]::Round($stopwatch.Elapsed.TotalSeconds, 3)
    testResultPaths = $trxFiles
    coveragePaths = $coverageFiles
    outputDirectory = $relativeOutput
    logPath = Get-RelativeRepositoryPath -Root $repositoryRoot -Path $logPath
    exitCode = $exitCode
}
if ($Json) {
    $report | ConvertTo-Json -Depth 6
}
else {
    Write-Host ("Coverage: {0}; {1:N2}s; output {2}" -f
        $(if ($succeeded) { 'SUCCEEDED' } else { 'FAILED' }),
        $stopwatch.Elapsed.TotalSeconds,
        $relativeOutput)
    foreach ($path in $trxFiles + $coverageFiles) {
        Write-Host ("  {0}" -f $path)
    }
}
exit $exitCode
