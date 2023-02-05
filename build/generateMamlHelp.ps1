# Copyright (c) 2023 Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Generates a MAML help file from markdown help files via platyPS\New-ExternalHelp.

.NOTES
    This is intended for use in MSBuild projects where platyPS is restored via <PackageReference>
    to support central version management via Directory.Packages.props.
#>

#Requires -Version 7

[CmdletBinding()]
param(
    # Source directory containing the markdown help file(s) created by platyPS
    [Parameter(Mandatory = $false, Position = 0)]
    [ValidateNotNullOrEmpty()]
    [string]$Path = "$PSScriptRoot/../docs/help",

    # Destination directory for converted markdown files.
    [Parameter(Mandatory = $true, Position = 1)]
    [ValidateNotNullOrEmpty()]
    [string]$Destination,

    # Path from which to import the platyPS module.
    # If not provided, platyPS is assumed to be already installed or imported.
    [Parameter(Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [string]$PlatyPSImportPath
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

if ($PlatyPSImportPath) {
    Import-Module -Name $PlatyPSImportPath
}
# Leave path testing to platyPS
New-ExternalHelp -Path $Path -OutputPath $Destination -Force
