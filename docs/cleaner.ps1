# Copyright (c) 2023 Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Delete bin/obj/_site subfolders and generated files in reference subfolder
#>

[CmdletBinding(SupportsShouldProcess)]
Param ()

Get-ChildItem -Path $PSScriptRoot -Directory -Recurse -Include bin, obj, _site | Remove-Item -Recurse
Get-ChildItem -Path $PSScriptRoot -File -Filter log.txt | Remove-Item
Get-ChildItem -Path "$PSScriptRoot/reference/*" -File -Exclude 'index.md' | Remove-Item
Get-ChildItem -Path "$PSScriptRoot/reference/includes/index-*.md" -File | Remove-Item
Get-ChildItem -Path "$PSScriptRoot/api/*" -File -Exclude 'index.md' | Remove-Item
