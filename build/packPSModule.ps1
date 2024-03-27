# Copyright (c) 2023-2024 Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Creates a PowerShell-compatible NuGet package which can be pushed via dotnet/nuget to PSGallery and
    other repositories.

.DESCRIPTION
    This uses Publish-Module with a temporary local repository to create the NuGet package once.

.NOTES
    TODO Publish-Module creates a nuspec and a temp project to use dotnet pack and dotnet nuget push.
         We could probably do this ourselves with less overhead?
#>

#Requires -Version 7

using namespace System
using namespace System.IO

[CmdletBinding()]
param(
    # Path of the module folder.
    [Parameter(Mandatory, Position = 0)]
    [ValidateNotNullOrEmpty()]
    [string]$Path,

    # Destination directory to store the nupkg
    [Parameter(Mandatory, Position = 1)]
    [ValidateNotNullOrEmpty()]
    [string]$Destination,

    # Skip generation of auto-tags. This is passed to Publish-Module.
    [switch]$SkipAutomaticTags
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Path -PathType Container)) {
    throw "Directory does not exist: $Path"
}
if (-not (Test-Path $Destination -PathType Container)) {
    # Destination may not exist if overwritten via cmdline option
    $null = New-Item $Destination -ItemType 'Directory'
}

[string]$tempRepoName = [Path]::GetRandomFileName()
[string]$tempRepoPath = Join-Path ([Path]::GetTempPath()) $tempRepoName
try {
    $null = New-Item $tempRepoPath -ItemType 'Directory' -Force
    Register-PSRepository -Name $tempRepoName -SourceLocation $tempRepoPath -InstallationPolicy Trusted
    Publish-Module -Repository $tempRepoName -Path $Path -SkipAutomaticTags:$SkipAutomaticTags
    Get-ChildItem -Path $tempRepoPath -File | Copy-Item -Destination $Destination
}
finally {
    Unregister-PSRepository -Name $tempRepoName -ErrorAction Ignore
    Remove-Item $tempRepoPath -Recurse -Force -ErrorAction Ignore
}
