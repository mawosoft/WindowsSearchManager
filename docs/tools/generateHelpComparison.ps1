# Copyright (c) Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Generate markdown help variants via platyPS and export results for comparison.

.DESCRIPTION
    Imports the required version of platyPS, builds and imports WindowsSearchManager,
    generates markdown help with and without -UseFullTypeName, with and without AlphabeticParamsOrder,
    generates the respective MAML files.
    Runs Get-Help for each MAML and without MAML and exports the results as text files.
    Also exports a syntax-only summary for all commands in a single text file.

.NOTES
    By default, the script starts a new PowerShell process to avoid importing WindowsSearchManager
    into the current instance, which would block subsequent builds of the module because the assembly
    remains loaded until PowerShell exits.
    The results are stored in a 'helpcmp<number>' directory, with subdirectories named
    shorttype, fulltype, nomaml.
    Each directory contains the maml-help.xml and the syntax-only.txt files as well as further subdirectories
    'md' and 'txt' for the generated markdown help the text produced by Get-Help -Full for each
    command.
#>

#Requires -Version 7

[CmdletBinding()]
param(
    # Destination directory for creating the 'helpcmp<number>' directory and its subdirectories.
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

& "$PSScriptRoot/exportTextHelp.ps1" (Join-Path $helpcmpdir 'nomaml')

[string[]]$subdirs = @('shorttype', 'fulltype', 'shortalpha', 'fullalpha')
foreach ($subdir in $subdirs) {
    [string]$typedir = Join-Path $helpcmpdir $subdir
    [string]$mddir = Join-Path $typedir 'md'
    $null = New-MarkdownHelp -Module 'WindowsSearchManager' -OutputFolder $mddir `
        -UseFullTypeName:($subdir.StartsWith('full')) -AlphabeticParamsOrder:($subdir.EndsWith('alpha'))
    $null = New-ExternalHelp -Path $mddir -OutputPath $typedir -Force
}

foreach ($subdir in $subdirs) {
    [string]$typedir = Join-Path $helpcmpdir $subdir
    Get-ChildItem (Join-Path $typedir '*-help.xml') | Copy-Item -Destination $moduledir
    Remove-Module WindowsSearchManager
    Import-Module $moduledir
    & "$PSScriptRoot/exportTextHelp.ps1" $typedir
    Get-ChildItem (Join-Path $moduledir '*-help.xml') | Remove-Item
}
