#requires -Version 7.0
[CmdletBinding()]
param(
    [string]$Case,
    [switch]$Json,
    [switch]$NoBuild,
    [string]$Root,
    [string]$Routes,
    [string]$Evals
)

$ErrorActionPreference = 'Stop'
$repoRoot = if ($Root) { (Resolve-Path -LiteralPath $Root).Path } else { (& git rev-parse --show-toplevel).Trim() }
if (-not $repoRoot) { Write-Error 'Unable to resolve repository root.'; exit 2 }
$project = Join-Path $repoRoot 'NoxAeterna.Tools.Repository/NoxAeterna.Tools.Repository.csproj'
$dll = Join-Path $repoRoot 'NoxAeterna.Tools.Repository/bin/Debug/net10.0/NoxAeterna.Tools.Repository.dll'
if (-not $NoBuild) {
    & dotnet build $project -c Debug --no-restore --nologo --verbosity quiet 1>$null
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
if (-not (Test-Path -LiteralPath $dll -PathType Leaf)) { Write-Error 'Repository tool output is missing; build first or omit -NoBuild.'; exit 2 }
$arguments = @($dll, 'context-eval', '--root', $repoRoot)
if ($Case) { $arguments += @('--case', $Case) }
if ($Routes) { $arguments += @('--routes', $Routes) }
if ($Evals) { $arguments += @('--evals', $Evals) }
if ($Json) { $arguments += '--json' }
& dotnet @arguments
exit $LASTEXITCODE
