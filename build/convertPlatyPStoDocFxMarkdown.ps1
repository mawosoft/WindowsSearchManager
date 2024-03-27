# Copyright (c) 2023-2024 Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Converts markdown help generated by platyPS to markdown for use with DocFx.

.DESCRIPTION
    - Transforms the markdown to take advantage of DocFx formatting.
    - Optionally removes technical details from the help text.
    - Adds xref links to type names. If short type names are used, mapping must be provided by the caller.
    - Generates index-All.md and index-<noun>.md files containing cmdlet indices with link and synopsis.
    - Generates a toc.yml and optionally appends other toc files.

.NOTES
    Type names are replaced with <xref:typename>, which should work well for full type names.
    If short type names are used or to create a custom xref link, a JSON mapping file must be provided.
        {
            "Foo": "<xref:MyNamespace.Foo>",
            "MyNamespace.Bar": "xref:MyNamespace.Bar?displayProperty=fullName>"
        }
    Automatic mappings are provided for a few common short names like String and SwitchParameter.

    The <xref:uid> syntax is the recommended notation.
    Shortcuts with quotes like @'uid' or @"uid" work too, but DocFx won't warn if it didn't find the uid.
    Markdown link [](xref:uid) is problematic because a missing uid will display only the empty brackets.
#>

#Requires -Version 7

using namespace System
using namespace System.Collections
using namespace System.Collections.Generic
using namespace System.Net
using namespace System.Text

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

    # Additional toc.yml paths to append to the ToC for cmdlets.
    # The complete ToC will be saved as toc.yml in -Destination.
    [ValidateNotNullOrEmpty()]
    [string[]]$AdditionalTocPath,

    # JSON file that maps type names in markdown help to xref notation.
    [ValidateNotNullOrEmpty()]
    [string]$XrefMap,

    # Parameter metadata to exclude in Parameters section.
    # These must match the spelling in the markdown help.
    [ValidateNotNullOrEmpty()]
    [string[]]$ExcludeParameterMetadata,

    # Exclude the description of common parameters from the Parameters section.
    [switch]$ExludeCommonParameters,

    # Exclude the descriptions for -WhatIf and -Confirm from the Parameters section
    [switch]$ExcludeWhatifConfirm,

    # Exclude the parameterset heading before syntax code block.
    [switch]$ExludeSyntaxParameterSetHeading,

    # Minimum number of parameters to break syntax blocks and put each parameter on a new line.
    [int]$BreakSyntaxThreshold = 2,

    # Always write to output file, even if content is unchanged.
    [switch]$WriteAlways
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'


# Conversion is implemented as a class for better code structure and to avoid output leakage.
class HelpConverter {
    # Map type names to xref links (provided by caller)
    static [hashtable]$Xrefmap = [hashtable]::new() # Casesensitive
    # Common short type mappings
    static [hashtable]$Automap = [hashtable]::new(@{
            'Object'          = '<xref:System.Object>'
            'String'          = '<xref:System.String>'
            'Int32'           = '<xref:System.Int32>'
            'UInt32'          = '<xref:System.UInt32>'
            'SwitchParameter' = '<xref:System.Management.Automation.SwitchParameter>'
        })
    # Module name from YAML header
    [string]$ModuleName = ''
    # Converted markdown
    [StringBuilder]$DocfxMarkdown = [StringBuilder]::new()
    # Synopsis content range in DocfxMarkdown StringBuilder
    [Range]$SynopsisRange
    # Types resolved via caller-provided Xrefmap.
    [HashSet[string]]$UsedTypes = [HashSet[string]]::new()
    # Short type names not found via Xrefmap or Automap
    [HashSet[string]]$UnknownTypes = [HashSet[string]]::new()

    HelpConverter([string]$markdownHelp) {
        [Queue[string]]$lines = $markdownHelp.Split([string[]]("`r`n", "`n"), [StringSplitOptions]::None)
        # Get module name from YAML header
        if ($lines.Count -gt 0 -and $lines.Peek() -ceq '---') {
            $lines.Dequeue()
            while ($lines.Count -gt 0 -and -not $this.ModuleName) {
                [string]$line = $lines.Dequeue()
                if ($line.TrimEnd() -ceq '---') { break }
                [int]$colon = $line.IndexOf([char]':')
                if ($colon -lt 0) { continue }
                if ($line.Substring(0, $colon).Trim() -ne 'Module Name') { continue }
                $this.ModuleName = $line.Substring($colon + 1).Trim()
            }
        }
        # Fast forward to level1 header
        while ($lines.Count -gt 0 -and -not $lines.Peek().StartsWith('# ', [StringComparison]::Ordinal)) {
            $lines.Dequeue()
        }
        if ($lines.Count -eq 0) { throw 'Level 1 header not found' }
        $this.DocfxMarkdown.AppendLine($lines.Dequeue()).AppendLine()
        # Convert help sections. Each section starts with a level 2 header
        for (; ; ) {
            while ($lines.Count -gt 0 -and -not $lines.Peek().StartsWith('## ', [StringComparison]::Ordinal)) {
                $lines.Dequeue()
            }
            if ($lines.Count -eq 0) { break }
            [string]$level2 = $this.ToTitleCase($lines.Dequeue().Substring(3).Trim())
            switch -exact -casesensitive ($level2) {
                'Synopsis' { $this.SynopsisRange = $this.ConvertSimpleSection('', $lines); break }
                'Syntax' { $this.ConvertSyntax($level2, $lines); break }
                'Parameters' { $this.ConvertParameters($level2, $lines); break }
                'Inputs' { $this.ConvertInputsOutputs($level2, $lines); break }
                'Outputs' { $this.ConvertInputsOutputs($level2, $lines); break }
                'Related Links' { $this.ConvertRelatedLinks($level2, $lines); break }
                # Description, Examples, Notes
                Default { $this.ConvertSimpleSection($level2, $lines); break }
            }
        }
    }

    # Simple conversion to title case
    hidden [string]ToTitleCase([string]$value) {
        [string[]]$parts = $value.Split([char]' ')
        for ([int]$i = $parts.Length - 1; $i -ge 0; $i--) {
            [string]$part = $parts[$i]
            if ($part.Length -gt 1) {
                $parts[$i] = $part.Substring(0, 1).ToUpper() + $part.Substring(1).ToLower()
            }
        }
        return [string]::Join([char]' ', $parts)
    }

    # Convert type name to xref link and record used/unknown
    hidden [string]ToXrefLink([string]$value) {
        if (-not $Value -or $Value -eq 'None') {
            return $Value
        }
        [string]$suffix = ''
        if ($value.EndsWith('[]', [StringComparison]::Ordinal)) {
            $suffix = '[]'
            $value = $value.Substring(0, $value.Length - 2)
        }
        [object]$link = [HelpConverter]::Xrefmap[$value]
        if ($null -ne $link) {
            $this.UsedTypes.Add($value)
        }
        else {
            $link = [HelpConverter]::Automap[$value]
            if ($null -eq $link -and $value.IndexOf([char]'.') -lt 0) {
                $this.UnknownTypes.Add($value)
            }
        }
        if (-not $link) {
            # DocFx doesn't use '+' for nested types
            $link = '<xref:' + $value.Replace([char]'+', [char]'.') + '>'
        }
        return $link + $suffix
    }

    # For simple sections, just copy w/o changes if section has content
    hidden [Range]ConvertSimpleSection([string]$heading, [Queue[string]]$lines) {
        [int]$startHeading = $this.DocfxMarkdown.Length
        if ($heading) {
            $this.DocfxMarkdown.Append('## ').AppendLine($heading)
        }
        [int]$startContent = $this.DocfxMarkdown.Length
        [bool]$hasContent = $false
        while ($lines.Count -gt 0 -and -not $lines.Peek().StartsWith('## ', [StringComparison]::Ordinal)) {
            [string]$line = $lines.Dequeue()
            $this.DocfxMarkdown.AppendLine($line)
            if (-not $hasContent -and -not [string]::IsNullOrWhiteSpace($line)) { $hasContent = $true }
        }
        if (-not $hasContent) {
            $this.DocfxMarkdown.Length = $startHeading
            $startContent = $startHeading
        }
        return [Range]::new($startContent, $this.DocfxMarkdown.Length)
    }

    # For Syntax, optionally omit parameter set headings and reformat codeblock
    # TODO Syntax generated by platyPS is wrong with regards to Required (maybe for switches only?)
    #      For multiple paramsets, see Test-SearchRule syntax.
    hidden [void]ConvertSyntax([string]$heading, [Queue[string]]$lines) {
        if ($heading) {
            $this.DocfxMarkdown.Append('## ').AppendLine($heading)
        }
        while ($lines.Count -gt 0 -and -not $lines.Peek().StartsWith('## ', [StringComparison]::Ordinal)) {
            [string]$line = $lines.Dequeue()
            if ($line.StartsWith('```', [StringComparison]::Ordinal)) {
                [StringBuilder]$codeblock = [StringBuilder]::new()
                while ($lines.Count -gt 0) {
                    $line = $lines.Dequeue()
                    if ($line.StartsWith('```', [StringComparison]::Ordinal)) { break }
                    $codeblock.AppendLine($line)
                }
                [string]$code = $codeblock.ToString()
                $code = [regex]::Replace($code, '\r\n|\n', ' ')
                $code = [regex]::Replace($code, '\s\s+', ' ').Trim()
                [string]$pattern = ' (?=\[?\[?-|\[<CommonParameters>\])'
                if ([regex]::Matches($code, $pattern).Count -ge $script:BreakSyntaxThreshold) {
                    [int]$indent = $code.IndexOf([char]'-')
                    if ($indent -lt 0) { $indent = 4 }
                    [string]$break = [Environment]::NewLine + [string]::new([char]' ', $indent)
                    $code = [regex]::Replace($code, $pattern, $break)
                    $codeblock.Clear().AppendLine($code)

                }
                # Using self-defined 'psmeta', because 'powershell' hljs doesn't work well for syntax notation.
                $this.DocfxMarkdown.AppendLine('```psmeta').Append($codeblock).AppendLine('```')
            }
            elseif (-not $script:ExludeSyntaxParameterSetHeading -or
                -not $line.StartsWith('### ', [StringComparison]::Ordinal)) {
                $this.DocfxMarkdown.AppendLine($line)
            }
        }
    }

    # For Parameters, convert yaml code block to table. Also apply other tweaks.
    hidden [void]ConvertParameters([string]$heading, [Queue[string]]$lines) {
        [int]$startHeading = $this.DocfxMarkdown.Length
        if ($heading) {
            $this.DocfxMarkdown.Append('## ').AppendLine($heading)
        }
        [bool]$hasContent = $false
        [bool]$skipParameter = $false
        while ($lines.Count -gt 0 -and -not $lines.Peek().StartsWith('## ', [StringComparison]::Ordinal)) {
            [string]$line = $lines.Dequeue()
            if ($line.StartsWith('### ', [StringComparison]::Ordinal)) {
                [string]$paramName = $line.Substring(4).Trim()
                if ($script:ExludeCommonParameters -and $paramName -eq 'CommonParameters') {
                    $skipParameter = $true
                }
                elseif ($script:ExcludeWhatifConfirm -and ($paramName -in '-WhatIf', '-Confirm')) {
                    $skipParameter = $true
                }
                else {
                    $this.DocfxMarkdown.AppendLine($line)
                    $skipParameter = $false
                    $hasContent = $true
                }
            }
            elseif (-not $skipParameter) {
                if ($line.StartsWith('```', [StringComparison]::Ordinal)) {
                    $hasContent = $true
                    [StringBuilder]$sb = $this.DocfxMarkdown.AppendLine('<table>')
                    while ($lines.Count -gt 0) {
                        $line = $lines.Dequeue()
                        if ($line.StartsWith('```', [StringComparison]::Ordinal)) { break }
                        [int]$colon = $line.IndexOf([char]':')
                        if ($colon -lt 0) { continue }
                        [string]$label = $line.Substring(0, $colon).Trim()
                        if ($label -in $script:ExcludeParameterMetadata) { continue }
                        [string]$value = $line.Substring($colon + 1).Trim()
                        if (-not $value) { continue }
                        if ($label -eq 'Type') {
                            $value = $this.ToXrefLink($value)
                        }
                        # extra lines to toggle between inline html and markdown
                        $sb.AppendLine('<tr><td>').AppendLine().AppendLine($label).AppendLine()
                        $sb.AppendLine('</td><td>').AppendLine().AppendLine($value).AppendLine()
                        $sb.AppendLine('</td></tr>')
                    }
                    $sb.AppendLine('</table>')
                }
                else {
                    $this.DocfxMarkdown.AppendLine($line)
                    if (-not $hasContent -and -not [string]::IsNullOrWhiteSpace($line)) { $hasContent = $true }
                }
            }
        }
        if (-not $hasContent) {
            $this.DocfxMarkdown.Length = $startHeading
        }
    }

    # For Inputs and Outputs sections, replace level 3 type name with bold xref as paragraph
    hidden [void]ConvertInputsOutputs([string]$heading, [Queue[string]]$lines) {
        [int]$startHeading = $this.DocfxMarkdown.Length
        if ($heading) {
            $this.DocfxMarkdown.Append('## ').AppendLine($heading)
        }
        [bool]$hasContent = $false
        while ($lines.Count -gt 0 -and -not $lines.Peek().StartsWith('## ', [StringComparison]::Ordinal)) {
            [string]$line = $lines.Dequeue()
            if ($line.StartsWith('### ', [StringComparison]::Ordinal)) {
                $this.DocfxMarkdown.Append('**').Append($this.ToXrefLink($line.Substring(4).Trim()))
                $this.DocfxMarkdown.AppendLine('**').AppendLine()
                $hasContent = $true
            }
            else {
                $this.DocfxMarkdown.AppendLine($line)
                if (-not $hasContent -and -not [string]::IsNullOrWhiteSpace($line)) { $hasContent = $true }
            }
        }
        if (-not $hasContent) {
            $this.DocfxMarkdown.Length = $startHeading
        }
    }

    # For Related Links, add bullet points
    hidden [void]ConvertRelatedLinks([string]$heading, [Queue[string]]$lines) {
        [int]$startHeading = $this.DocfxMarkdown.Length
        if ($heading) {
            $this.DocfxMarkdown.Append('## ').AppendLine($heading)
        }
        [bool]$hasContent = $false
        while ($lines.Count -gt 0 -and -not $lines.Peek().StartsWith('## ', [StringComparison]::Ordinal)) {
            [string]$line = $lines.Dequeue()
            if (-not [string]::IsNullOrWhiteSpace($line)) {
                $this.DocfxMarkdown.Append('- ')
                $hasContent = $true
            }
            $this.DocfxMarkdown.AppendLine($line)
        }
        if (-not $hasContent) {
            $this.DocfxMarkdown.Length = $startHeading
        }
    }
}

# Write content to file if changed or -WriteAlways is specified
function Update-Content {
    [CmdletBinding()]
    param(
        # Path of the file
        [Parameter(Mandatory, Position = 0)]
        [ValidateNotNullOrEmpty()]
        [string]$Path,
        # Content to write
        [Parameter(Mandatory, Position = 1, ValueFromPipeline)]
        [Object[]]$Value,
        # Suppress newline
        [switch]$NoNewline
    )

    begin {
        [StringBuilder]$sb = [StringBuilder]::new()

        # Append to $content while flattening input
        function Append {
            param([object]$obj)

            if ($obj -is [IList]) {
                foreach ($o in $obj) { Append $o }
            }
            else {
                $null = $sb.Append($obj)
                if (-not $NoNewline) { $null = $sb.AppendLine() }
            }
        }
    }
    process {
        Append $Value
    }
    end {
        [string]$content = $sb.ToString()
        if ($WriteAlways -or -not (Test-Path $Path -PathType Leaf) -or
            -not $content.Equals((Get-Content $Path -Raw), [StringComparison]::Ordinal)) {
            $content | Set-Content -Path $Path -NoNewline
        }
    }
}

################################################################################
#
#   Main
#
################################################################################

# Supress VT100 esc sequences when invoked via dotnet/msbuild
# Exec task process hierarchy is dotnet|msbuild -> cmd|sh -> pwsh

try {
    [string]$parent = (Get-Process -Id $PID).Parent.Parent.Name
    if ($parent -eq 'dotnet' -or $parent -eq 'MSBuild') {
        $PSStyle.OutputRendering = 'PlainText'
    }
}
catch {}

# Validate paths

if (-not (Test-Path $Path -PathType Container)) {
    throw "Directory not found: $Path"
}
if (-not (Test-Path $Destination -PathType Container)) {
    throw "Directory not found: $Destination"
}

foreach ($toc in $AdditionalTocPath) {
    if (-not (Test-Path $toc -PathType Leaf)) {
        throw "File not found: $toc"
    }
}

if ($XrefMap -and -not (Test-Path $XrefMap -PathType Leaf)) {
    throw "File not found: $XrefMap"
}

[string]$includesPath = Join-Path $Destination 'includes'
if (-not (Test-Path $includesPath -PathType Container)) {
    $null = New-Item $includesPath -ItemType Directory
}

# Setup globals

[ArrayList]$cmdlets = @()
[HashSet[string]]$usedTypes = @()
[HashSet[string]]$unknownTypes = @()

if ($XrefMap) {
    [HelpConverter]::Xrefmap = Get-Content $XrefMap -Raw | ConvertFrom-Json -AsHashtable
}

# Convert the help files

Join-Path $Path '*-*.md' | Get-ChildItem | ForEach-Object {
    [string]$content = Get-Content -Path $_ -Raw
    [HelpConverter]$converter = [HelpConverter]::new($content)
    $usedTypes.UnionWith($converter.UsedTypes)
    $unknownTypes.UnionWith($converter.UnknownTypes)
    [string]$destinationFile = Join-Path $Destination $_.Name
    $content = $converter.DocfxMarkdown.ToString()
    $content | Update-Content -Path $destinationFile -NoNewline
    $null = $cmdlets.Add([PSCustomObject]@{
            ModuleName = $converter.ModuleName
            Name       = $_.BaseName
            Noun       = $_.BaseName.Split([char]'-', 2)[1]
            TocItem    = @(
                "  - name: $($_.BaseName)",
                "    href: $($_.BaseName).md"
            )
            # Links in an include file are relative to that include file.
            # Surrounding table cell content with empty lines re-enables markdown.
            TableRow   = @(
                '<tr><td>', '',
                "[$($_.BaseName)](../$($_.BaseName).md)",
                '', '</td><td>', '',
                $content.Substring($converter.SynopsisRange.Start.Value,
                    $converter.SynopsisRange.End.Value - $converter.SynopsisRange.Start.Value),
                '', '</td></tr>')
        })
}

# Create the ToC
# Assume we are dealing with a single module.
# Otherwise everything needs to be grouped by module name first.

[string]$moduleName = 'Table of Contents'
if ($cmdlets -and $cmdlets[0].ModuleName) {
    $moduleName = $cmdlets[0].ModuleName
}
[ArrayList]$lines = @(
    '### YamlMime:TableOfContent'
    'items:'
    "- name: $moduleName"
    '  href: index.md'
    '  items:'
    ($cmdlets | Sort-Object Name).TocItem
)
foreach ($toc in $AdditionalTocPath) {
    $lines.AddRange((Get-Content $toc).Where({ $_[0] -ceq '-' -or $_[0] -ceq ' ' }))
}
$lines | Update-Content -Path (Join-Path $Destination 'toc.yml')

# Create the includes

@(
    '<table>', ($cmdlets | Sort-Object Name).TableRow, '</table>'
) | Update-Content -Path (Join-Path $includesPath 'index-All.md')

$cmdlets | Group-Object Noun | ForEach-Object {
    @(
        '<table>', ($_.Group | Sort-Object Name).TableRow, '</table>'
    ) | Update-Content -Path (Join-Path $includesPath "index-$($_.Name).md")
}

# Verify type mapping

foreach ($t in $unknownTypes) {
    Write-Warning "No xrefmap for type: $t"
}
[HashSet[string]]$unusedTypes = $usedTypes.ExceptWith([string[]][HelpConverter]::Xrefmap.Keys)
foreach ($t in $unusedTypes) {
    Write-Information "Unused xrefmap for type: $t"
}
