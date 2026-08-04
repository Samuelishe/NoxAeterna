#requires -Version 7.0
[CmdletBinding()]
param(
    [switch]$Install,
    [switch]$Check,
    [string]$DestinationRoot
)

$ErrorActionPreference = 'Stop'

function Get-SkillName {
    param([Parameter(Mandatory)][string]$SkillDirectory)

    $skillFile = Join-Path $SkillDirectory 'SKILL.md'
    if (-not (Test-Path -LiteralPath $skillFile -PathType Leaf)) {
        throw "SKILL.md is missing: $skillFile"
    }

    $content = [IO.File]::ReadAllText($skillFile)
    $frontmatter = [regex]::Match(
        $content,
        '\A---\r?\n(?<body>.*?)\r?\n---(?:\r?\n|\z)',
        [Text.RegularExpressions.RegexOptions]::Singleline)
    if (-not $frontmatter.Success) {
        throw "Invalid YAML frontmatter boundary: $skillFile"
    }

    $nameMatch = [regex]::Match($frontmatter.Groups['body'].Value, '(?m)^\s*name\s*:\s*(?<value>.+?)\s*$')
    if (-not $nameMatch.Success) {
        throw "YAML frontmatter name is missing: $skillFile"
    }

    $name = $nameMatch.Groups['value'].Value.Trim()
    if (($name.StartsWith('"') -and $name.EndsWith('"')) -or
        ($name.StartsWith("'") -and $name.EndsWith("'"))) {
        $name = $name.Substring(1, $name.Length - 2)
    }

    if ($name -notmatch '^[a-z0-9]+(?:-[a-z0-9]+)*$') {
        throw "Invalid skill name '$name' in $skillFile"
    }

    $directoryName = Split-Path -Leaf $SkillDirectory
    if ($directoryName -cne $name) {
        throw "Skill directory '$directoryName' does not match YAML name '$name'."
    }

    return $name
}

function Get-FileHashMap {
    param([Parameter(Mandatory)][string]$Root)

    $map = [Collections.Generic.Dictionary[string, string]]::new([StringComparer]::Ordinal)
    foreach ($file in Get-ChildItem -LiteralPath $Root -Recurse -File) {
        $relativePath = [IO.Path]::GetRelativePath($Root, $file.FullName).Replace('\', '/')
        $map.Add($relativePath, (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash)
    }

    return $map
}

function Install-Skill {
    param(
        [Parameter(Mandatory)][string]$SourceDirectory,
        [Parameter(Mandatory)][string]$TargetDirectory
    )

    [void](Get-SkillName -SkillDirectory $SourceDirectory)
    New-Item -ItemType Directory -Path $TargetDirectory -Force | Out-Null

    $sourcePaths = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($sourceFile in Get-ChildItem -LiteralPath $SourceDirectory -Recurse -File) {
        $relativePath = [IO.Path]::GetRelativePath($SourceDirectory, $sourceFile.FullName)
        [void]$sourcePaths.Add($relativePath.Replace('\', '/'))
    }

    foreach ($targetFile in Get-ChildItem -LiteralPath $TargetDirectory -Recurse -File) {
        $relativePath = [IO.Path]::GetRelativePath($TargetDirectory, $targetFile.FullName).Replace('\', '/')
        if (-not $sourcePaths.Contains($relativePath)) {
            Remove-Item -LiteralPath $targetFile.FullName -Force
        }
    }

    foreach ($sourceDirectoryItem in Get-ChildItem -LiteralPath $SourceDirectory -Recurse -Directory) {
        $relativePath = [IO.Path]::GetRelativePath($SourceDirectory, $sourceDirectoryItem.FullName)
        New-Item -ItemType Directory -Path (Join-Path $TargetDirectory $relativePath) -Force | Out-Null
    }

    foreach ($sourceFile in Get-ChildItem -LiteralPath $SourceDirectory -Recurse -File) {
        $relativePath = [IO.Path]::GetRelativePath($SourceDirectory, $sourceFile.FullName)
        $targetFile = Join-Path $TargetDirectory $relativePath
        New-Item -ItemType Directory -Path (Split-Path -Parent $targetFile) -Force | Out-Null
        Copy-Item -LiteralPath $sourceFile.FullName -Destination $targetFile -Force
    }

    foreach ($targetSubdirectory in Get-ChildItem -LiteralPath $TargetDirectory -Recurse -Directory |
        Sort-Object { $_.FullName.Length } -Descending) {
        if (-not (Get-ChildItem -LiteralPath $targetSubdirectory.FullName -Force)) {
            Remove-Item -LiteralPath $targetSubdirectory.FullName -Force
        }
    }

    [void](Get-SkillName -SkillDirectory $TargetDirectory)
}

function Compare-Skill {
    param(
        [Parameter(Mandatory)][string]$SourceDirectory,
        [Parameter(Mandatory)][string]$TargetDirectory
    )

    $skillName = Get-SkillName -SkillDirectory $SourceDirectory
    if (-not (Test-Path -LiteralPath $TargetDirectory -PathType Container)) {
        throw "Installed skill is missing: $TargetDirectory"
    }

    $installedName = Get-SkillName -SkillDirectory $TargetDirectory
    if ($skillName -cne $installedName) {
        throw "Installed skill name '$installedName' does not match source '$skillName'."
    }

    $sourceMap = Get-FileHashMap -Root $SourceDirectory
    $targetMap = Get-FileHashMap -Root $TargetDirectory
    $missing = @($sourceMap.Keys | Where-Object { -not $targetMap.ContainsKey($_) } | Sort-Object)
    $extra = @($targetMap.Keys | Where-Object { -not $sourceMap.ContainsKey($_) } | Sort-Object)
    $different = @($sourceMap.Keys | Where-Object {
        $targetMap.ContainsKey($_) -and $sourceMap[$_] -cne $targetMap[$_]
    } | Sort-Object)

    if ($missing.Count -or $extra.Count -or $different.Count) {
        $details = @()
        if ($missing.Count) { $details += "missing: $($missing -join ', ')" }
        if ($extra.Count) { $details += "extra: $($extra -join ', ')" }
        if ($different.Count) { $details += "different: $($different -join ', ')" }
        throw "Skill '$skillName' is not synchronized ($($details -join '; '))."
    }

    Write-Output "Checked ${skillName}: $($sourceMap.Count) files match by relative path and SHA-256."
}

try {
    if ($Install -eq $Check) {
        throw 'Specify exactly one mode: -Install or -Check.'
    }

    $sourceRoot = Join-Path $PSScriptRoot 'codex-skills'
    if (-not (Test-Path -LiteralPath $sourceRoot -PathType Container)) {
        throw "Repository skill root is missing: $sourceRoot"
    }

    if ([string]::IsNullOrWhiteSpace($DestinationRoot)) {
        if (-not [string]::IsNullOrWhiteSpace($env:CODEX_HOME)) {
            $DestinationRoot = Join-Path $env:CODEX_HOME 'skills'
        }
        elseif (-not [string]::IsNullOrWhiteSpace($env:USERPROFILE)) {
            $DestinationRoot = Join-Path (Join-Path $env:USERPROFILE '.codex') 'skills'
        }
        else {
            throw 'Neither CODEX_HOME nor USERPROFILE is available to resolve the destination.'
        }
    }

    $DestinationRoot = [IO.Path]::GetFullPath($DestinationRoot)
    $sourceSkills = @(Get-ChildItem -LiteralPath $sourceRoot -Directory | Where-Object {
        Test-Path -LiteralPath (Join-Path $_.FullName 'SKILL.md') -PathType Leaf
    })
    if (-not $sourceSkills.Count) {
        throw "No repository-owned skills were found under $sourceRoot"
    }

    if ($Install) {
        New-Item -ItemType Directory -Path $DestinationRoot -Force | Out-Null
    }

    foreach ($sourceSkill in $sourceSkills) {
        $skillName = Get-SkillName -SkillDirectory $sourceSkill.FullName
        $targetDirectory = Join-Path $DestinationRoot $skillName
        if ($Install) {
            Install-Skill -SourceDirectory $sourceSkill.FullName -TargetDirectory $targetDirectory
            Write-Output "Installed $skillName to $targetDirectory"
        }
        else {
            Compare-Skill -SourceDirectory $sourceSkill.FullName -TargetDirectory $targetDirectory
        }
    }
}
catch {
    Write-Error $_
    exit 1
}
