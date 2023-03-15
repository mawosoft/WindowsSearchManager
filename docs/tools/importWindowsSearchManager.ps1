# Copyright (c) 2023 Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Build and import WindowsSearchManager
#>

#Requires -Version 7

[CmdletBinding()]
param(
    # Build configuration to use. Defaults to 'Release'.
    [ValidateSet('Debug', 'Release')]
    [Alias('c')]
    [string]$Configuration = 'Release',

    # Exclude the MAML help file from build
    [switch]$NoMamlHelp
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

if (Get-Module WindowsSearchManager) {
    throw 'Module WindowsSearchManager has already been imported. Cannot remove binary module(s).'
    return
}

[string]$srcdir = "$PSScriptRoot/../../src/Mawosoft.PowerShell.WindowsSearchManager"
[string]$publishdir = "$srcdir/bin/$Configuration/netstandard2.0/WindowsSearchManager"
dotnet publish "$srcdir/Mawosoft.PowerShell.WindowsSearchManager.csproj" -c $Configuration -p:NoMamlHelp=$NoMamlHelp
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish exited with code=$LASTEXITCODE"
    return
}
if ($NoMamlHelp) {
    # There could be left-overs from a previous build. Normally, they are removed, but in case something went wrong...
    Get-ChildItem "$publishdir/*-help.xml" | Remove-Item
}
Import-Module $publishdir
