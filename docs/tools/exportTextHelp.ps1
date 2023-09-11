# Copyright (c) 2023 Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Exports text of Get-Help -Full for each WindowsSearchManager command and a syntax-only summary
    for all commands in a single text file.

.NOTES
    Caller must have already imported the WindowsSearchManager module.
#>

#Requires -Version 7

using namespace System
using namespace System.Collections
using namespace System.Text

[CmdletBinding()]
param(
    # Output directory
    [Parameter(Mandatory, Position = 0)]
    [string]$Path
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

[string]$txtdir = Join-Path $Path 'txt'
$null = New-Item $txtdir -ItemType Directory -Force
[hashtable]$cmdsByNoun = Get-Command -Module WindowsSearchManager | Group-Object Noun -AsHashTable
[ArrayList]$cmdsOrdered = @()
foreach ($noun in 'SearchManager', 'SearchCatalog', 'SearchRoot', 'SearchRule') {
    $cmds = $cmdsByNoun[$noun] | Sort-Object Name
    if ($cmds) {
        $cmdsOrdered.AddRange($cmds)
        $cmdsByNoun.Remove($noun)
    }
}
if ($cmdsByNoun.Count -gt 0) {
    Write-Warning 'Found commands with unexpected nouns.'
    $cmds = $cmdsByNoun.GetEnumerator() | Sort-Object Key | Select-Object -ExpandProperty Value | Sort-Object Name
    $cmdsOrdered.AddRange($cmds)
}
[StringBuilder]$syntax = [StringBuilder]::new()
foreach ($cmd in $cmdsOrdered) {
    $lines = (Get-Help $cmd.Name -Full | Out-String) -join [Environment]::NewLine -split [Environment]::NewLine
    $lines | Set-Content -Path (Join-Path $txtdir "$($cmd.Name).txt")
    $null = $syntax.Append('## ').AppendLine($cmd.Name).AppendLine()
    [bool]$isSyntax = $false
    foreach ($line in $lines) {
        if ($isSyntax) {
            if (-not $line.StartsWith('  ', [StringComparison]::Ordinal)) { break }
            $null = $syntax.AppendLine($line)
        }
        elseif ($line.StartsWith('SYNTAX', [StringComparison]::Ordinal)) {
            $isSyntax = $true
        }
    }
}

[string]$s = $syntax.ToString()
$s | Set-Content -Path (Join-Path $Path 'syntax.txt')

[string]$break = [Environment]::NewLine + [string]::new([char]' ', 8)
$s = [regex]::Replace($s, ' (?=\[?\[?-|\[<CommonParameters>\])', $break)
$s = [regex]::Replace($s, '<(?!CommonParameters>)[^>]+>', '<value>')
$s | Set-Content -Path (Join-Path $Path 'syntax-order.txt')
