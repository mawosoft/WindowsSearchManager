# Copyright (c) 2023-2024 Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Updates the module manifest with PowerShell-compatible version info derived
    from given SemVer2 version.

.DESCRIPTION
    The module manifest template must contain the placeholders @ModuleVersion@ and @Prerelease@
    to be replaced with the corresponding parts from the SemVer2 version.

    The prerelease tag will be sanitized to be PS-compatible by removing dots and hyphens and
    padding any numeric-only part with leading zeros.

    The manifest content or the proper placement of the placeholders is *not* validated.
#>

#Requires -Version 7

using namespace System

[CmdletBinding()]
param(
    # Path of the manifest file to use as a template.
    [Parameter(Mandatory, Position = 0)]
    [ValidateNotNullOrEmpty()]
    [string]$Path,

    # Destination file or directory path.
    # If this is a directory, the name of the source file is used.
    [Parameter(Mandatory, Position = 1)]
    [ValidateNotNullOrEmpty()]
    [string]$Destination,

    # Version string (SemVer 2.0)
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$Version,

    # Padding to use for numeric parts of the prerelease label when making it PS-compatible.
    [ValidateRange(1, 10)]
    [int]$NumberPadding = 4,

    # Always write to output file, even if content is unchanged.
    [switch]$WriteAlways
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

[string]$moduleversionPattern = '@ModuleVersion@'
[string]$prereleasePattern = '@Prerelease@'

if (Test-Path $Destination -PathType Container) {
    $Destination = Join-Path $Destination (Split-Path $Path -Leaf)
}

[string]$manifest = Get-Content $Path -Raw

if (-not $manifest.Contains($moduleversionPattern, [StringComparison]::OrdinalIgnoreCase) -or
    -not $manifest.Contains($prereleasePattern, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Placeholders not found.'
    return
}

[string]$moduleversion = $Version
[string]$prerelease = ''
[int]$plusminus = $Version.IndexOfAny('+-', [StringComparison]::Ordinal)
if ($plusminus -ge 0) {
    $moduleversion = $Version.Substring(0, $plusminus)
    if ($Version[$plusminus] -ceq '-') {
        $prerelease = $Version.Substring($plusminus + 1)
        [int]$plus = $prerelease.IndexOf('+', [StringComparison]::Ordinal)
        if ($plus -ge 0) { $prerelease = $prerelease.Substring(0, $plus) }
        $parts = $prerelease.Split([char[]]'.-')
        # Sanitize and pad prerelease
        [int]$parsed = 0
        [string]$format = [string]::new('0', $NumberPadding)
        for ([int]$i = 0; $i -lt $parts.Count; $i++) {
            if ([int]::TryParse($parts[$i], [ref]$parsed)) {
                $parts[$i] = $parsed.ToString($format)
            }
        }
        $prerelease = $parts -join ''
    }
}
$manifest = $manifest -replace $moduleversionPattern, $moduleversion
$manifest = $manifest -replace $prereleasePattern, $prerelease
if ($WriteAlways -or -not (Test-Path $Destination -PathType Leaf) -or
    -not $manifest.Equals((Get-Content $Destination -Raw), [StringComparison]::Ordinal)) {
    $manifest | Set-Content -Path $Destination -NoNewline
}
