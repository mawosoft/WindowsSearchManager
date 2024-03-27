# Copyright (c) 2023-2024 Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Generates a MAML help file from markdown help files via platyPS\New-ExternalHelp.

.NOTES
    This is intended for use in MSBuild projects where platyPS is restored via <PackageReference>
    to support central version management via Directory.Packages.props. The generated MAML help
    file is postprocessed to apply some tweaks.
    MAML postprocessing:
    - Ensure proper parameter order in syntax blocks.
    - Use short type names in syntax blocks.
    - Remove empty NOTES sections.
    - Reformat docfx alerts (like > [!NOTE]).
#>

#Requires -Version 7

using namespace System
using namespace System.Collections.Generic
using namespace System.IO
using namespace System.Text.RegularExpressions
using namespace System.Xml.Linq

[CmdletBinding()]
param(
    # Source directory containing the markdown help file(s) created by platyPS
    [Parameter(Mandatory, Position = 0)]
    [ValidateNotNullOrEmpty()]
    [string]$Path,

    # Destination directory for converted markdown files.
    [Parameter(Mandatory, Position = 1)]
    [ValidateNotNullOrEmpty()]
    [string]$Destination,

    # Path from which to import the platyPS module.
    # If not provided, platyPS is assumed to be already installed or imported.
    [ValidateNotNullOrEmpty()]
    [string]$PlatyPSImportPath
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

# Supress VT100 esc sequences when invoked via dotnet/msbuild
# Exec task process hierarchy is dotnet|msbuild -> cmd|sh -> pwsh

try {
    [string]$parent = (Get-Process -Id $PID).Parent.Parent.Name
    if ($parent -eq 'dotnet' -or $parent -eq 'MSBuild') {
        $PSStyle.OutputRendering = 'PlainText'
    }
}
catch {}

if ($PlatyPSImportPath) {
    Import-Module -Name $PlatyPSImportPath
}

# Leave path testing to platyPS
$mamlHelp = New-ExternalHelp -Path $Path -OutputPath $Destination -Force | Where-Object Extension -EQ '.xml'

$syntaxes = Get-ChildItem $Path -Filter '*.md' -File | Get-Content -Raw | ForEach-Object {
    [regex]::Match($_, '(?ns)\n## SYNTAX(.*?\n```.*?\n(?<syntax>.*?)\n```)+?\s*\n## ').Groups['syntax'].Captures.Value | ForEach-Object {
        [PSCustomObject]@{
            Cmdlet = [regex]::Match($_, '(?ns)^[A-Za-z\-]+').Value
            Params = [string[]][regex]::Matches($_, '(?ns)[\[\s]-(?<param>[A-Za-z]+)').ForEach({ $_.Groups['param'].Value })
        }
    }
}

$mamlHelp | ForEach-Object {
    # Postprocess MAML files
    [XDocument]$maml = [XDocument]::Load($_.FullName)
    [XNamespace]$nsmaml = $maml.Root.FirstNode.GetNamespaceOfPrefix('maml')
    [XNamespace]$nscmd = $maml.Root.FirstNode.GetNamespaceOfPrefix('command')
    [bool]$script:dirty = $false

    # Ensure proper order of parameters per syntax item (helpItems/command/syntax/syntaxItem).
    $maml.Root.Descendants($nscmd + 'syntaxItem').ForEach({
            [string]$cmdlet = $_.Element($nsmaml + 'name')?.Value
            [XElement[]]$params = $_.Elements($nscmd + 'parameter')
            if ($params) {
                [Dictionary[string, XElement]]$paramsMap = [KeyValuePair[string, XElement][]]$params.ForEach({
                        [KeyValuePair[string, XElement]]::new($_.Element($nsmaml + 'name')?.Value, $_)
                    })
                [HashSet[string]]$paramsSet = $paramsMap.Keys
                $foundSyntax = $syntaxes.Where({ $cmdlet -eq $_.Cmdlet -and $paramsSet.SetEquals($_.Params) })
                if (-not $foundSyntax) {
                    Write-Warning "No matching markdown paramset found for $cmdlet $($paramsMap.Keys)"
                }
                elseif ($foundSyntax.Count -ne 1) {
                    Write-Warning "Multiple matching markdown paramsets found for $cmdlet $($paramsMap.Keys)"
                }
                else {
                    $syntax = $foundSyntax[0]
                    if (-not [System.Linq.Enumerable]::SequenceEqual($paramsSet, $syntax.Params)) {
                        Write-Verbose "Reordering syntax $cmdlet $($paramsMap.Keys) -> $($syntax.Params)"
                        $params.ForEach({ $_.Remove() })
                        $_.Add([XElement[]]$syntax[0].Params.ForEach({ $paramsMap[$_] }))
                        $script:dirty = $true
                    }
                }
            }
        })

    # Use short type names in syntax blocks (helpItems/command/syntax/syntaxItem/parameter/parameterValue).
    [hashtable]$shortenedTypes = @{}
    $maml.Root.Descendants($nscmd + 'syntaxItem').ForEach({
            $_.Descendants($nscmd + 'parameterValue').ForEach({
                    [string]$s = $_.Value
                    [Match]$m = [regex]::Match($s, '^(\w+\.)+')
                    if ($m.Success) {
                        $shortenedTypes[$s] = $shortenedTypes[$s] + 1
                        $_.Value = $s.Remove($m.Index, $m.Length)
                        $script:dirty = $true
                    }
                })
        })
    if ($shortenedTypes.Count) {
        'Shortened type names in syntax blocks:', ($shortenedTypes | Format-Table -AutoSize | Out-String) | Write-Verbose
    }

    # EXAMPLES and NOTES are the only help sections that can be omitted from display if they have no content.
    # All other help sections will *always* show their heading. For EXAMPLES, omission happens automatically,
    # for NOTES, an empty alertSet node must be removed (helpItems/command/alertSet).
    $maml.Root.Descendants($nsmaml + 'alertSet').Where({ $_.Value.Length -eq 0 }).ForEach({
            Write-Verbose "Removing empty notes for $($_.Parent?.Element($nscmd + 'details')?.Element($nscmd + 'name')?.Value)"
            $_.Remove()
            $script:dirty = $true
        })

    # [!NOTE] and similar docfx alerts appear as '> [!NOTE] > text' in MAML.
    [int]$alertCount = 0
    $maml.Root.Descendants($nsmaml + 'para').Where({ $_.Value.StartsWith('>') }).ForEach({
            [string]$s1 = $_.Value
            [string]$s2 = [regex]::Replace($s1, '(?n)^> ?\[!(?<alert>[A-Za-z]+)\][ \t]*\r?\n?>[ \t]*', "`${alert}:`n")
            if (-not [object]::ReferenceEquals($s1, $s2)) {
                # Handle multiline alerts as well
                $_.Value = [regex]::Replace($s2, '\n>[ \t]*', "`n")
                $script:dirty = $true
                $alertCount++
            }
        })
    if ($alertCount) {
        Write-Verbose "Reformatted $alertCount [!NOTE]-style alerts."
    }

    if ($script:dirty) {
        $maml.Save($_.FullName)
    }
}
