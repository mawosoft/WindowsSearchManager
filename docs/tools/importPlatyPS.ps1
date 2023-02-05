# Copyright (c) 2023 Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Import platyPS module as defined in help.msbuildproj.
#>

#Requires -Version 7

using namespace System
using namespace System.Xml

[CmdletBinding()]
param()

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

[string]$propsFile = "$PSScriptRoot/../help/obj/help.msbuildproj.nuget.g.props"

if (-not (Test-Path $propsFile -PathType Leaf)) {
    dotnet restore "$PSScriptRoot/../help/help.msbuildproj"
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore exited with code=$LASTEXITCODE"
        return
    }
}

[XmlNode]$docElem = (Select-Xml -Path $propsFile -XPath '/*').Node
[string]$pkgdir = (Select-Xml -xml $docElem -XPath '//ns:PkgplatyPS' -Namespace @{ ns = $docElem.NamespaceURI }).Node.InnerText
[string]$platyPSImportPath = Join-Path $pkgdir 'platyPS.psd1'
Import-Module $platyPSImportPath
