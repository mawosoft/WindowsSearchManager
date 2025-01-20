# Copyright (c) Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Generates Get-Help -Full preview for WindowsSearch with and without MAML and exports results for
    comparison.

.DESCRIPTION
    Builds and imports WindowsSearchManager, runs Get-Help for each command with MAML and without
    MAML and exports the results as text files.
    Also exports a syntax-only summary for all commands in a single text file.

.NOTES
    By default, the script starts a new PowerShell process to avoid importing WindowsSearchManager
    into the current instance, which would block subsequent builds of the module because the assembly
    remains loaded until PowerShell exits.
    The results are stored in a 'helpcmp<number>' directory, with subdirectories named
    nomaml and preview.
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

& "$PSScriptRoot/importWindowsSearchManager.ps1"
[string]$moduledir = Split-Path (Get-Module WindowsSearchManager).Path -Parent

[string]$helpcmpdir = Join-Path $Path 'helpcmp'
[int] $i = 1
while (Test-Path $helpcmpdir) {
    $i++
    $helpcmpdir = Join-Path $Path "helpcmp$i"
}
$null = New-Item $helpcmpdir -ItemType Directory

[string]$previewdir = Join-Path $helpcmpdir 'preview'
& "$PSScriptRoot/exportTextHelp.ps1" $previewdir

Get-ChildItem (Join-Path $moduledir '*-help.xml') | Move-Item -Destination $previewdir
Remove-Module WindowsSearchManager
Import-Module $moduledir

& "$PSScriptRoot/exportTextHelp.ps1" (Join-Path $helpcmpdir 'nomaml')
