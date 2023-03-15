# Copyright (c) 2023 Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Generate markdown help variants via platyPS and export results for comparison.

.DESCRIPTION
    Imports the required version of platyPS, builds and imports WindowsSearchManager,
    generates markdown help with and without -UseFullTypeName, generates the respective
    MAML files.
    Runs Get-Help for each MAML and without MAML and exports the results as text files.
    Also exports a syntax-only summary for all commands in a single text file.

.NOTES
    By default, the script starts a new PowerShell process to avoid importing WindowsSearchManager
    into the current instance, which would block subsequent builds of the module because the assembly
    remains loaded until PowerShell exits.
    The results are stored in a 'helpcmp<timestamp>' directory, with subdirectories named
    shorttype, fulltype, nomaml.
    Each directory contains the maml-help.xml and the syntax-only.txt files as well as further subdirectories
    'md' and 'txt' for the generated markdown help the text produced by Get-Help -Full for each
    command.
#>

#Requires -Version 7

using namespace System
using namespace System.Collections
using namespace System.Text

[CmdletBinding()]
param(
    # Destination directory for creating the 'helpcmp<timestamp>' directory and its subdirectories.
    [Parameter(Mandatory = $false, Position = 0)]
    [ValidateNotNullOrEmpty()]
    [string]$Path = "$PSScriptRoot/../help/obj",

    # Controls whether the script is run in a new PowerShell process.
    # 'Conditional' starts a new process only if WindowsSearchManager has already been imported.
    [ValidateSet('Always', 'Conditional', 'Never')]
    [string]$NewProcess = 'Always'
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

if ($NewProcess -eq 'Always' -or ($NewProcess -eq 'Conditional' -and (Get-Module WindowsSearchManager))) {
    [string]$command = "& '$PSCommandPath' -NewProcess:Never"
    # Need to enumerate because common parameters may exist
    foreach ($param in $PSBoundParameters.GetEnumerator()) {
        if ($param.Key -eq 'NewProcess') { continue }
        if ($MyInvocation.MyCommand.Parameters[$param.Key].SwitchParameter) {
            $command += " -$($param.Key):`$$($param.Value)"
        }
        else {
            # This should be sufficient for non-switches in common parameters, i.e. no quotes needed.
            $command += " -$($param.Key) $($param.Value)"
        }
    }
    pwsh -Command $command
    if ($LASTEXITCODE -ne 0) {
        throw "New PowerShell process failed with exit code $LASTEXITCODE"
    }
    return
}

# .SYNOPSIS
#   Export full help text for each command and a syntax-only summary for all.
function Export-TextHelp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory, Position = 0)]
        [string]$Path
    )

    [string]$txtdir = Join-Path $Path 'txt'
    $null = New-Item $txtdir -ItemType Directory -Force
    [hashtable]$cmdsByNoun = Get-Command -Module WindowsSearchManager | Group-Object Noun -AsHashTable
    [System.Collections.ArrayList]$cmdsOrdered = @()
    foreach ($noun in 'SearchManager', 'SearchCatalog', 'SearchRoot', 'SearchRule')
    {
        $cmds = $cmdsByNoun[$noun] | Sort-Object Name
        if ($cmds) { 
            $cmdsOrdered.AddRange($cmds) 
            $cmdsByNoun.Remove($noun)
        }
    }
    if ($cmdsByNoun.Count -gt 0) {
        Write-Warning "Found commands with unexpected nouns."
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
    $syntax.ToString() | Set-Content -Path (Join-Path $Path 'syntax.txt')
}


################################################################################
#
#   Main
#
################################################################################

& "$PSScriptRoot/importPlatyPS.ps1"

# We need to exclude the current MAML help file from build/import.
# Otherwise platyPS would not see any changes to parameter sets and simply report the MAML help content.
& "$PSScriptRoot/importWindowsSearchManager.ps1" -NoMamlHelp
[string]$moduledir = Split-Path (Get-Module WindowsSearchManager).Path -Parent

[string]$helpcmpdir = Join-Path $Path 'helpcmp'
[int] $i = 1
while (Test-Path $helpcmpdir) {
    $i++
    $helpcmpdir = Join-Path $Path "helpcmp$i"
}
$null = New-Item $helpcmpdir -ItemType Directory

Export-TextHelp (Join-Path $helpcmpdir 'nomaml')

[string[]]$subdirs = @('shorttype', 'fulltype')
foreach ($subdir in $subdirs) {
    [string]$typedir = Join-Path $helpcmpdir $subdir
    [string]$mddir = Join-Path $typedir 'md'
    $null = New-MarkdownHelp -Module 'WindowsSearchManager' -OutputFolder $mddir -UseFullTypeName:($subdir -eq 'fulltype')
    $null = New-ExternalHelp -Path $mddir -OutputPath $typedir -Force
}

foreach ($subdir in $subdirs) {
    [string]$typedir = Join-Path $helpcmpdir $subdir
    Get-ChildItem (Join-Path $typedir '*-help.xml') | Copy-Item -Destination $moduledir
    Remove-Module WindowsSearchManager
    Import-Module $moduledir
    Export-TextHelp $typedir
    Get-ChildItem (Join-Path $moduledir '*-help.xml') | Remove-Item
}
