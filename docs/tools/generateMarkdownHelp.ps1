# Copyright (c) 2023 Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Generate markdown help via platyPS.

.NOTES
    - Imports platyPS module as defined in help.msbuildproj.
    - Builds and imports WindowsSearchManager
    - Invokes New-MarkdownHelp or Update-MarkdownHelp
#>

#Requires -Version 7

[CmdletBinding()]
param(
    # Forwarded in case Update-MarkdownHelpModule is invoked, otherwise ignored.
    [switch]$UpdateInputOutput,
    # Forwarded in case Update-MarkdownHelpModule is invoked, otherwise ignored.
    [switch]$Force
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

& "$PSScriptRoot/importPlatyPS.ps1"
[string]$srcdir = "$PSScriptRoot/../../src/Mawosoft.PowerShell.WindowsSearchManager"
dotnet publish "$srcdir/Mawosoft.PowerShell.WindowsSearchManager.csproj"
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish exited with code=$LASTEXITCODE"
    return
}
Import-Module "$srcdir/bin/Debug/netstandard2.0/WindowsSearchManager" -Force
[string]$helpdir = "$PSScriptRoot/../help"
if (Test-Path "$helpdir/*.md") {
    Update-MarkdownHelpModule -Path $helpdir -UpdateInputOutput:$UpdateInputOutput -Force:$Force
}
else {
    New-MarkdownHelp -Module 'WindowsSearchManager' -OutputFolder $helpdir
}
