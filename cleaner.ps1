# Copyright (c) 2022-2023 Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Delete bin/obj subfolders, optionally delete *.binlog files/subfolders, call all cleaner.ps1 in subfolders.
#>

[CmdletBinding(SupportsShouldProcess)]
Param (
    [Alias("bl")]
    # Also delete *.binlog files/subfolders
    [switch]$BinLog
)

Get-ChildItem -Path $PSScriptRoot -Directory -Recurse -Include bin, obj | Remove-Item -Recurse
Get-ChildItem -Path $PSScriptRoot -Filter cleaner.ps1 -Recurse -File | Where-Object DirectoryName -ne $PSScriptRoot | ForEach-Object { & $_.FullName }
if ($BinLog.IsPresent) {
    Get-ChildItem -Path $PSScriptRoot -Directory -Recurse -Include MSBuild_Logs | Remove-Item -Recurse
    Get-ChildItem -Path $PSScriptRoot -File -Recurse -Include *.binlog | Remove-Item
}
