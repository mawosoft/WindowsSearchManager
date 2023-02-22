# Copyright (c) 2023 Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Build and import WindowsSearchManager
#>

#Requires -Version 7

[CmdletBinding()]
param(
    # Build configuration to use. Defaults to 'Debug'.
    [ValidateSet('Debug', 'Release')]
    [Alias('c')]
    [string]$Configuration = 'Debug'
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

if (Get-Module WindowsSearchManager) {
    throw 'Module WindowsSearchManager has already been imported. Cannot remove binary module(s).'
    return
}

[string]$srcdir = "$PSScriptRoot/../../src/Mawosoft.PowerShell.WindowsSearchManager"
dotnet publish "$srcdir/Mawosoft.PowerShell.WindowsSearchManager.csproj" -c $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish exited with code=$LASTEXITCODE"
    return
}
Import-Module "$srcdir/bin/$Configuration/netstandard2.0/WindowsSearchManager"
