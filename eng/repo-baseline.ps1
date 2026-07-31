[CmdletBinding()]
param(
    [switch]$Json
)

$ErrorActionPreference = 'Stop'

function Invoke-Git {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments)

    $output = @(& git @Arguments 2>$null)
    [ordered]@{
        exitCode = $LASTEXITCODE
        output = $output
    }
}

function Get-RepositoryLocalDotNetProcesses {
    param([string]$RepositoryRoot)

    $matches = [Collections.Generic.List[object]]::new()
    try {
        $candidates = if ($IsWindows) {
            Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
                ForEach-Object {
                    [ordered]@{
                        pid = [int]$_.ProcessId
                        commandLine = [string]$_.CommandLine
                    }
                }
        }
        elseif ($IsLinux) {
            Get-Process -Name dotnet -ErrorAction SilentlyContinue |
                ForEach-Object {
                    $commandLinePath = "/proc/$($_.Id)/cmdline"
                    $commandLine = if (Test-Path -LiteralPath $commandLinePath) {
                        (Get-Content -LiteralPath $commandLinePath -Raw).Replace([char]0, ' ')
                    } else { '' }
                    [ordered]@{
                        pid = $_.Id
                        commandLine = $commandLine
                    }
                }
        }
        else {
            @(& ps -axo pid=,command= 2>$null) |
                Where-Object { $_ -match '^\s*(?<pid>\d+)\s+(?<command>.*\bdotnet\b.*)$' } |
                ForEach-Object {
                    [ordered]@{
                        pid = [int]$Matches['pid']
                        commandLine = $Matches['command']
                    }
                }
        }

        $rootVariants = @(
            $RepositoryRoot,
            $RepositoryRoot.Replace('\', '/')
        )
        foreach ($candidate in $candidates) {
            if ([string]::IsNullOrWhiteSpace($candidate.commandLine)) {
                continue
            }

            $isLocal = $false
            foreach ($rootVariant in $rootVariants) {
                if ($candidate.commandLine.Contains($rootVariant, [StringComparison]::OrdinalIgnoreCase)) {
                    $isLocal = $true
                    break
                }
            }

            if ($isLocal) {
                $matches.Add([ordered]@{
                    pid = $candidate.pid
                    commandLine = $candidate.commandLine
                })
            }
        }
    }
    catch {
        return @()
    }

    return @($matches | Sort-Object -Property pid)
}

$rootResult = Invoke-Git rev-parse --show-toplevel
if ($rootResult.exitCode -ne 0 -or $rootResult.output.Count -eq 0) {
    throw 'Unable to determine repository root.'
}

$repositoryRoot = (Resolve-Path -LiteralPath $rootResult.output[0].Trim()).Path
$headResult = Invoke-Git rev-parse HEAD
$messageResult = Invoke-Git log -1 --pretty=%s
$parentResult = Invoke-Git rev-parse 'HEAD^'
$branchResult = Invoke-Git symbolic-ref --short -q HEAD
$statusResult = Invoke-Git status --short
$gitDirectoryResult = Invoke-Git rev-parse --git-dir

$branch = if ($branchResult.exitCode -eq 0 -and $branchResult.output.Count -gt 0) {
    $branchResult.output[0].Trim()
} else {
    '(detached)'
}
$parentSha = if ($parentResult.exitCode -eq 0 -and $parentResult.output.Count -gt 0) {
    $parentResult.output[0].Trim()
} else {
    $null
}
$statusEntries = @($statusResult.output | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })

$operations = [Collections.Generic.List[string]]::new()
if ($gitDirectoryResult.exitCode -eq 0 -and $gitDirectoryResult.output.Count -gt 0) {
    $gitDirectory = $gitDirectoryResult.output[0].Trim()
    if (-not [IO.Path]::IsPathRooted($gitDirectory)) {
        $gitDirectory = Join-Path $repositoryRoot $gitDirectory
    }
    $markers = [ordered]@{
        merge = @('MERGE_HEAD')
        rebase = @('rebase-merge', 'rebase-apply')
        'cherry-pick' = @('CHERRY_PICK_HEAD')
        revert = @('REVERT_HEAD')
        bisect = @('BISECT_LOG')
    }
    foreach ($operation in $markers.Keys) {
        if ($markers[$operation] | Where-Object { Test-Path -LiteralPath (Join-Path $gitDirectory $_) }) {
            $operations.Add($operation)
        }
    }
}

$processes = Get-RepositoryLocalDotNetProcesses -RepositoryRoot $repositoryRoot
$report = [ordered]@{
    schemaVersion = 1
    repositoryRoot = $repositoryRoot
    branch = $branch
    headSha = $headResult.output[0].Trim()
    headMessage = $messageResult.output[0]
    parentSha = $parentSha
    worktreeClean = $statusEntries.Count -eq 0
    statusEntries = $statusEntries
    activeGitOperations = @($operations)
    repositoryLocalDotNetProcesses = @($processes)
}

if ($Json) {
    $report | ConvertTo-Json -Depth 6
    exit 0
}

Write-Host "Repository: $repositoryRoot"
Write-Host "Branch: $branch"
Write-Host "HEAD: $($report.headSha) $($report.headMessage)"
Write-Host "Parent: $(if ($null -eq $parentSha) { '(none)' } else { $parentSha })"
Write-Host "Worktree: $(if ($report.worktreeClean) { 'clean' } else { "dirty ($($statusEntries.Count) entries)" })"
Write-Host "Git operations: $(if ($operations.Count -eq 0) { 'none' } else { $operations -join ', ' })"
Write-Host "Repo-local .NET processes: $($processes.Count)"

exit 0
