# Copyright (c) 2023 Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Generate markdown help via platyPS.

.DESCRIPTION
    Imports the required version of platyPS, builds and imports WindowsSearchManager.
    Uses New-MarkdownHelp if no help files exists yet, otherwise Update-MarkdownHelpModule.
    In the latter case, the current version of the help files is backed up before updating.
    If no changes occured, the backup is deleted, otherwise WinMerge is started to show the diffs.

.NOTES
    By default, the script starts a new PowerShell process to avoid importing WindowsSearchManager
    into the current instance, which would block subsequent builds of the module because the assembly
    remains loaded until PowerShell exits.

    Current behavior of platyPS (as of v0.14.2) for full vs. short type names (String vs. System.String):
    - Syntax: **Always** uses short type name. (But **only** in markdown, MAML/Get-Help has full type name)
    - Parameters: Controlled by -UseFullTypeName
    - Inputs/Outputs: **Always uses full type name.
    As long as the Syntax section keeps the short name, we should use the -UseFullTypeName switch
    to make DocFx xrefs easier.

    Current behavior of platyPS (as of v0.14.2) for parameter -AlphabeticParamsOrder:
    - Syntax: true:  Original order is preserved in markdown and MAML/Get-Help.
              false: Original order is preserved in markdown, but **not** in MAML/Get-Help.
              In MAML/Get-Help, the original order is sometimes preserved, sometimes not,
              regardless of the setting.
    - Parameters: true:  Sorted alphabetically, except for selected params like -Confirm, WhatIf.
                  false: Sorted alphabetically, includig -Confirm, -WhatIf, etc.
    Doesn't seem to make much sense. We have to use -AlphabeticParamsOrder to avoid the syntax block
    getting messed up.
#>

#Requires -Version 7

[CmdletBinding()]
param(
    # Forwarded to New-MarkdownHelp and Update-MarkdownHelpModule.
    # Uses full type name instead of short name for parameters.
    [switch]$UseFullTypeName,

    # Forwarded in case Update-MarkdownHelpModule is invoked, otherwise ignored.
    # Refreshes the Input and Output sections to reflect the current state of the cmdlet.
    # WARNING: this parameter will remove any manual additions to these sections.
    [switch]$UpdateInputOutput,

    # Forwarded in case Update-MarkdownHelpModule is invoked, otherwise ignored.
    # Removes help files that no longer exists within sessions (for example if function was deleted).
    [switch]$Force,

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

& "$PSScriptRoot/importPlatyPS.ps1"

# We need to exclude the current MAML help file from build/import.
# Otherwise platyPS would not see any changes to parameter sets and simply report the MAML help content.
& "$PSScriptRoot/importWindowsSearchManager.ps1" -NoMamlHelp

[string]$helpdir = "$PSScriptRoot/../help"
if (-not (Test-Path "$helpdir/*.md")) {
    New-MarkdownHelp -Module 'WindowsSearchManager' -OutputFolder $helpdir -AlphabeticParamsOrder `
        -UseFullTypeName:$UseFullTypeName
}
else {
    [string]$bakdir = Join-Path $helpdir "bak$(Get-Date -Format FileDateTime)"
    $null = New-Item $bakdir -ItemType Directory
    Get-ChildItem "$helpdir/*.md" -File | Copy-Item -Destination $bakdir

    Update-MarkdownHelpModule -Path $helpdir -AlphabeticParamsOrder `
        -UseFullTypeName:$UseFullTypeName -UpdateInputOutput:$UpdateInputOutput -Force:$Force

    if (Compare-Object -ReferenceObject (Get-ChildItem "$helpdir/*.md" -File | Get-FileHash) `
            -DifferenceObject (Get-ChildItem "$bakdir/*" -File | Get-FileHash) `
            -Property Hash, { Split-Path $_.Path -Leaf }) {
        [string]$winmerge = Join-Path $env:ProgramFiles 'WinMerge\WinMergeU.exe'
        if (Test-Path $winmerge -PathType Leaf) {
            & $winmerge $helpdir $bakdir
        }
    }
    else {
        Remove-Item $bakdir -Recurse -ErrorAction Ignore
    }
}
