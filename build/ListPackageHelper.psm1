# Copyright (c) 2022 Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Helpers for processing 'dotnet list package' results.
#>

#Requires -Version 7.2
#Requires -Modules Microsoft.PowerShell.Utility

using namespace System
using namespace System.IO
using namespace System.Collections
using namespace System.Collections.Generic
using namespace System.Management.Automation
using namespace System.Net
using namespace System.Reflection
using namespace System.Text
using namespace System.Text.RegularExpressions
using namespace Newtonsoft.Json
using module Microsoft.PowerShell.Utility
using namespace Microsoft.PowerShell.Commands

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

if ($null -eq (Get-ChildItem function:Start-NativeExecution -ErrorAction SilentlyContinue)) {
    . "$PSScriptRoot/startNativeExecution.ps1"
}

# .SYNOPSIS
#   Report types corresonding to tool options like --outdated. As flags because mergeable
[Flags()]
enum ReportTypes {
    Default = 1
    Outdated = 2
    Deprecated = 4
    Vulnerable = 8
}

# .SYNOPSIS
#   Package reference type
enum PackageRefType {
    TopLevel; Transitive
}

# .SYNOPSIS
#   Project with target frameworks
class Project : IComparable {
    [ValidateNotNullOrEmpty()][string]$Project
    [ValidateNotNullOrEmpty()][string[]]$Frameworks

    Project([string]$project, [string[]]$frameworks) {
        $this.Project = $project
        $this.Frameworks = $frameworks.Clone()
        [array]::Sort($this.Frameworks)
    }

    [string]ToString() {
        return $this.Project + ' [' + ($this.Frameworks -join ', ') + ']'
    }

    [int]CompareTo([Object]$other) {
        if ($null -eq $other) { return 1 }
        if ($other -isnot [Project]) { throw [ArgumentException]::new($null, 'other') }
        [int]$retVal = [string]::Compare($this.Project, ([Project]$other).Project,
            [StringComparer]::OrdinalIgnoreCase)
        if ($retVal -ne 0) { return $retVal }
        return [string]::Compare(($this.Frameworks -join ', '), (([Project]$other).Frameworks -join ', '),
            [StringComparer]::OrdinalIgnoreCase)
    }

    [bool]Equals([Object]$other) {
        if ([Object]::ReferenceEquals($this, $other)) { return $true }
        if ($null -eq $other) { return $false }
        if ($other -isnot [Project]) { return ([Object]$this).Equals($other) }
        return $this.CompareTo($other) -eq 0
    }

    [int]GetHashCode() {
        [HashCode]$hash = [HashCode]::new()
        $hash.Add($this.ToString(), [StringComparer]::OrdinalIgnoreCase)
        return $hash.ToHashCode()
    }
}

# .SYNOPSIS
#   Single parsed package reference for one specific project/framework combo, but can contain
#   results from different report types via AddReport().
# .NOTES
#   - All reports must have been added here before merging package references that are identical
#     across multiple projects/frameworks. See also ListPackageResult and its AddReport() method.
#   - In proper C# the properties would be readonly. Also, Pwsh cannot handle having the
#     class name as parameter type on a virtual method, thus making the implementation
#     of IEquatable[ParsedPackageRef] impossible.
#   - Pwsh ConvertTo-Json doesn't honor the JsonIgnoreAttribute or the ISerializable interface
class ParsedPackageRef {
    #region Properties
    [ReportTypes]$ReportTypes
    [PackageRefType]$RefType
    [ValidateNotNullOrEmpty()][string]$Project
    [ValidateNotNullOrEmpty()][string]$Framework
    # Default data, contained in every report
    [ValidateNotNullOrEmpty()][string]$Name
    [ValidateNotNullOrEmpty()][string]$Resolved
    [bool]$AutoReference # Only for top-level
    [string]$Requested # Only for top-level
    # Outdated
    [string]$Latest
    # Deprecated (Reasons, Alternative; 'list package' omits the optional message field)
    # https://docs.microsoft.com/en-us/nuget/api/registration-base-url-resource#package-deprecation
    [ValueTuple[string, string]]$Deprecated
    # Vulnerable (Severity, Advisory Url)
    # https://docs.microsoft.com/en-us/nuget/api/registration-base-url-resource#vulnerabilities
    [ValueTuple[string, string][]]$Vulnerable
    # Calculated properties
    # - PERF This is about 5 to 6 times faster than using CodeProperty (calculated each time) or
    #   NoteProperty (calculated once). Oddly enough, CodeProperty and NoteProperty have about the
    #   same performance.
    # - JsonIgnoreAttribute. See note above. We have to handle this ourselves.
    [JsonIgnore()][string]$Key
    [JsonIgnore()][string]$DisplayName
    [JsonIgnore()][string]$ValueGroup
    #endregion

    #region Methods

    # Construct from 'list package' output. Processed lines will be dequeued from $lines.
    ParsedPackageRef([string]$project, [string]$framework, [ParsedColumnInfo]$columnInfo, [Queue[string]]$lines) {
        if (-not $lines.Peek().StartsWith('   > ')) {
            throw "Invalid package row"
        }
        [string]$line = $lines.Dequeue()
        [string]$pkgname = [ParsedPackageRef]::TryGetColumnValue($columnInfo, 'Name', $line).Substring(2).Trim()
        if ($pkgname.EndsWith(' (A)')) {
            $pkgname = $pkgname.Substring(0, $pkgname.Length - 4).Trim()
            $this.AutoReference = $true
        }
        $this.Name = $pkgname
        $this.ReportTypes = $columnInfo.ReportType
        $this.RefType = $columnInfo.RefType
        $this.Project = $project
        $this.Framework = $framework
        $this.Resolved = [ParsedPackageRef]::TryGetColumnValue($columnInfo, 'Resolved', $line)
        $this.Requested = [ParsedPackageRef]::TryGetColumnValue($columnInfo, 'Requested', $line)
        $this.Latest = [ParsedPackageRef]::TryGetColumnValue($columnInfo, 'Latest', $line)
        $this.Deprecated = [ValueTuple]::Create(
            [ParsedPackageRef]::TryGetColumnValue($columnInfo, 'Reason(s)', $line),
            [ParsedPackageRef]::TryGetColumnValue($columnInfo, 'Alternative', $line))
        $this.Vulnerable = @()
        if ($this.ReportTypes -eq [ReportTypes]::Vulnerable) {
            $this.Vulnerable += [ValueTuple]::Create(
                [ParsedPackageRef]::TryGetColumnValue($columnInfo, 'Severity', $line),
                [ParsedPackageRef]::TryGetColumnValue($columnInfo, 'Advisory URL', $line))
            while ($lines.Count -ne 0 -and $lines.Peek().StartsWith('     ')) {
                $line = $lines.Dequeue()
                $this.Vulnerable += [ValueTuple]::Create(
                    [ParsedPackageRef]::TryGetColumnValue($columnInfo, 'Severity', $line),
                    [ParsedPackageRef]::TryGetColumnValue($columnInfo, 'Advisory URL', $line))
            }
        }
        $this.Key = $this.GetKey()
        $this.DisplayName = $this.GetDisplayName()
        $this.ValueGroup = $this.GetValueGroup()
    }

    # Construct from deserialized object
    ParsedPackageRef([PsCustomObject]$deserialized) {
        [PropertyInfo[]]$pi = [ParsedPackageRef]::GetSerializableProperties()
        $pi.ForEach({ $this.($_.Name) = $deserialized.($_.Name) })
        $this.Key = $this.GetKey()
        $this.DisplayName = $this.GetDisplayName()
        $this.ValueGroup = $this.GetValueGroup()
    }

    # Convert to serializable object
    [PSCustomObject]ToSerializable() {
        [PropertyInfo[]]$pi = [ParsedPackageRef]::GetSerializableProperties()
        [Specialized.OrderedDictionary]$retVal = [ordered]@{}
        $pi.ForEach({ $retVal.Add($_.Name, $this.($_.Name)) })
        return [PSCustomObject]$retVal
    }

    # Add a report from another ParsedPackageRef
    [void]AddReport([ParsedPackageRef]$other) {
        if ($this.Key -ne $other.Key) {
            throw "Package keys don't match: $($this.Key) <-> $($other.Key)"
        }
        if ($this.ReportTypes -band $other.ReportTypes) {
            throw "Report of type '$($other.ReportTypes)' has already been added."
        }
        $this.ReportTypes = $this.ReportTypes -bor $other.ReportTypes
        if ($other.ReportTypes -band [ReportTypes]::Outdated) {
            $this.Latest = $other.Latest
        }
        if ($other.ReportTypes -band [ReportTypes]::Deprecated) {
            $this.Deprecated = $other.Deprecated
        }
        if ($other.ReportTypes -band [ReportTypes]::Vulnerable) {
            $this.Vulnerable = $other.Vulnerable.Clone()
        }
        $this.ValueGroup = $this.GetValueGroup()
    }

    # HACK instead of IEquatable[ParsedPackageRef].Equals() override. See notes above.
    [bool]Equals2([ParsedPackageRef]$other) {
        if ([Object]::ReferenceEquals($this, $other)) { return $true }
        if ($null -eq $other) { return $false }
        if ($this.ReportTypes -ne $other.ReportTypes) { return $false }
        if ($this.Key -ne $other.Key) { return $false }
        if ($this.ValueGroup -ne $other.ValueGroup) { return $false }
        return $true
    }

    [bool]Equals([Object]$other) {
        if ([Object]::ReferenceEquals($this, $other)) { return $true }
        if ($null -eq $other) { return $false }
        if ($other -is [ParsedPackageRef]) { return $this.Equals2([ParsedPackageRef]$other) }
        return ([Object]$this).Equals($other)
    }

    [int]GetHashCode() {
        [HashCode]$hash = [HashCode]::new()
        $hash.Add($this.ReportTypes)
        $hash.Add($this.Key, [StringComparer]::OrdinalIgnoreCase)
        $hash.Add($this.ValueGroup, [StringComparer]::OrdinalIgnoreCase)
        return $hash.ToHashCode()
    }

    hidden [string]GetKey() {
        return "$($this.Project)/$($this.Framework)/$($this.RefType)/$($this.Name)/$($this.Resolved)/$(
            $this.AutoReference)/$($this.Requested)"
    }

    hidden [string]GetDisplayName() {
        return "$($this.Name) $(if ($this.AutoReference) { '(A) ' })$(
            if ($this.Requested -and $this.Requested -ne $this.Resolved) { $this.Requested + ' >> ' }
        )$($this.Resolved)"
    }

    hidden [string]GetValueGroup() {
        return "$($this.Latest)/$($this.Deprecated)/$($this.Vulnerable -join '/')"
    }

    hidden static [PropertyInfo[]]GetSerializableProperties() {
        return [ParsedPackageRef].GetProperties().Where({ -not $_.IsDefined([JsonIgnoreAttribute], $false) })
    }

    # Gets a column value from the line adjusting for index-out-of-range and excess whitespace
    hidden static [string]TryGetColumnValue([ParsedColumnInfo]$columnInfo, [string]$columnName, [string]$line) {
        if (-not $columnInfo.Columns.ContainsKey($columnName)) { return [string]::Empty }
        [ValueTuple[int, int]]$slice = $columnInfo.Columns[$columnName]
        if ($slice.Item2 -le 0) { return [string]::Empty }
        [int]$start = $slice.Item1
        [int]$end = $start + $slice.Item2
        if ($start -lt 0) { $start = 0 }
        if ($end -gt $line.Length) { $end = $line.Length }
        if ($end -le $start) { return [string]::Empty }
        return $line.Substring($start, $end - $start).Trim()
    }
    #endregion
}

# .SYNOPSIS
#   Merged package reference used by multiple projects/frameworks
# .NOTES
#   We keep this separate from ParsedPackageRef due to some ambiguities regarding requested
#   version (e.g. 'list package' won't show versions requested via PackageVersion instead of
#   PackageReference).
class MergedPackageRef {
    [ValidateNotNullOrEmpty()][string]$DisplayName
    [string]$Latest
    [ValueTuple[string, string]]$Deprecated
    [ValueTuple[string, string][]]$Vulnerable
    [ValidateNotNullOrEmpty()][Project[]]$Projects
    [string]$ProjectGroup
    hidden [GroupInfo]$GroupInfo

    hidden MergedPackageRef([GroupInfo]$groupInfo) {
        $this.GroupInfo = $groupInfo
        [ParsedPackageRef]$pkg = $groupInfo.Group[0]
        $this.DisplayName = $pkg.DisplayName
        $this.Latest = $pkg.Latest
        $this.Deprecated = $pkg.Deprecated
        $this.Vulnerable = $pkg.Vulnerable.Clone()
        $this.Projects = $groupInfo.Group | Group-Object Project | ForEach-Object {
            [Project]::new($_.Values[0], (($_.Group)?.Framework | Sort-Object -Unique))
        } | Sort-Object -Property { $_.ToString() }
        $this.ProjectGroup = $this.Projects -join [Environment]::NewLine
    }

    # Merge collection of ParsedPackageRef into collection of MergedPackageRef
    static [MergedPackageRef[]]Create([ParsedPackageRef[]]$packages) {
        return $packages | Group-Object -Property DisplayName, ValueGroup | ForEach-Object {
            [MergedPackageRef]::new($_)
        }
    }

    # Gets the required column width for each property. This is for data only, padding etc must be added
    # by the caller. A value of 0 means the column contains no data.
    # For the value tuples Deprecated and Vulnerable, a tuple with the required width for each item,
    # plus the required width for the combined items is returned.
    static [hashtable]GetColumnWidth([MergedPackageRef[]]$packages) {
        return @{
            DisplayName = [int]($packages.DisplayName | Measure-Object Length -Maximum)?.Maximum
            Latest      = [int]($packages.Latest | Measure-Object Length -Maximum)?.Maximum
            Deprecated  = [ValueTuple]::Create(
                [int]($packages.Deprecated.Item1 | Measure-Object Length -Maximum)?.Maximum,
                [int]($packages.Deprecated.Item2 | Measure-Object Length -Maximum)?.Maximum,
                [int]($packages.Deprecated.ForEach({ if ($_.Item1 -or $_.Item2) { $_.ToString() } }) |
                    Measure-Object Length -Maximum)?.Maximum
            )
            Vulnerable  = [ValueTuple]::Create(
                [int]($packages.ForEach({ $_.Vulnerable.ForEach({ $_.Item1 }) }) |
                    Measure-Object Length -Maximum)?.Maximum,
                [int]($packages.ForEach({ $_.Vulnerable.ForEach({ $_.Item2 }) }) |
                    Measure-Object Length -Maximum)?.Maximum,
                [int]($packages.ForEach({ $_.Vulnerable.ForEach({ $_.ToString() }) }) |
                    Measure-Object Length -Maximum)?.Maximum
            )
            Projects    = [int]($packages.Projects.ForEach({ $_.ToString() }) |
                Measure-Object Length -Maximum)?.Maximum
        }
    }

    # Format as console table output.
    # Note: While Pwsh allows param defaults, they are *NOT* optional when calling a method.
    static [string]FormatTable([MergedPackageRef[]]$packages, [string]$packageCaption = 'Package',
        [bool]$excludeEmptyColumns = $true) {

        [string]$accent = ''
        [string]$reset = ''
        if ($global:PSStyle.OutputRendering -eq [OutputRendering]::Ansi) {
            $accent = $global:PSStyle.Formatting.FormatAccent
            $reset = $global:PSStyle.Reset
        }
        [Object[]]$columns = @(
            @{ l = $packageCaption; e = 'DisplayName' },
            'Latest',
            @{ l = 'Vulnerable'; e = { $_.Vulnerable.Item1 } },
            @{ l = 'Deprecated'; e = { $_.Deprecated.Item1 } }
        )
        [StringBuilder]$sb = [StringBuilder]::new()
        [GroupInfo[]] $groups = $packages | Group-Object ProjectGroup
        foreach ($group in $groups) {
            $sb.Append($accent).Append('Projects: ').Append($reset)
            $sb.AppendJoin([Environment]::NewLine + '          ', $group.Group[0].Projects)
            $sb.AppendLine()
            [Object[]]$columns2 = $columns
            if ($excludeEmptyColumns) {
                [hashtable]$widths = [MergedPackageRef]::GetColumnWidth($group.Group)
                $columns2 = @($columns[0])
                if ($widths.Latest -ne 0) { $columns2 += $columns[1] }
                if ($widths.Vulnerable.Item1 -ne 0) { $columns2 += $columns[2] }
                if ($widths.Deprecated.Item1 -ne 0) { $columns2 += $columns[3] }
            }
            $sb.Append(($group.Group | Format-Table $columns2 -AutoSize | Out-String))
        }
        return $sb.ToString()
    }

    # Format as GitHub Markdown Html table
    # Note: While Pwsh allows param defaults, they are *NOT* optional when calling a method.
    static [string]FormatMarkdownHtmlTable([MergedPackageRef[]]$packages, [string]$packageCaption = 'Package',
        [int]$projectHeadingThreshold = 1, [bool]$excludeEmptyColumns = $true) {

        [StringBuilder]$sb = [StringBuilder]::new()
        [GroupInfo[]] $groups = $packages | Group-Object ProjectGroup
        if ($groups.Count -le $projectHeadingThreshold) {
            foreach ($group in $groups) {
                [MergedPackageRef]$pkg = $group.Group[0]
                $sb.Append('<ul><li><samp>')
                [int]$n = $pkg.Projects.Count - 1
                for ([int]$i = 0; $i -le $n; $i++) {
                    $sb.Append([WebUtility]::HtmlEncode($pkg.Projects[$i].Project)).Append(' <code>[')
                    $sb.AppendJoin(', ', $pkg.Projects[$i].Frameworks).Append(']</code>')
                    if ($i -ne $n) { $sb.AppendLine('<br>') }
                }
                $sb.AppendLine('</li></ul></samp>')
                [MergedPackageRef]::HtmlTableHelper($sb, $group.Group, $packageCaption, $false,
                    $excludeEmptyColumns)
            }

        }
        else {
            [MergedPackageRef]::HtmlTableHelper($sb, $packages, $packageCaption, $true,
                $excludeEmptyColumns)
        }
        return $sb.ToString()
    }

    hidden static [void]HtmlTableHelper([StringBuilder]$sb, [MergedPackageRef[]]$packages,
        [string]$packageCaption, [bool]$projectsAsDetails, [bool]$excludeEmptyColumns) {

        [hashtable]$widths = [MergedPackageRef]::GetColumnWidth($packages)
        $sb.AppendLine('<table><tr>')
        $sb.Append('<th>').Append([WebUtility]::HtmlEncode($packageCaption)).AppendLine('</th>')
        if (-not $excludeEmptyColumns -or $widths.Latest -ne 0) { $sb.AppendLine('<th>Latest</th>') }
        if (-not $excludeEmptyColumns -or $widths.Vulnerable.Item3 -ne 0) { $sb.AppendLine('<th>Vulnerable</th>') }
        if (-not $excludeEmptyColumns -or $widths.Deprecated.Item3 -ne 0) { $sb.AppendLine('<th>Deprecated</th>') }
        $sb.AppendLine('</tr>')
        foreach ($pkg in $packages) {
            $sb.AppendLine('<tr valign="top">')
            if ($projectsAsDetails) {
                $sb.Append('<td><details><summary>').Append([WebUtility]::HtmlEncode($pkg.DisplayName))
                $sb.AppendLine('</summary>').Append('<samp>')
                [int]$n = $pkg.Projects.Count - 1
                for ([int]$i = 0; $i -le $n; $i++) {
                    $sb.Append([WebUtility]::HtmlEncode($pkg.Projects[$i].Project)).Append(' <code>[')
                    $sb.AppendJoin(', ', $pkg.Projects[$i].Frameworks).Append(']</code>')
                    if ($i -ne $n) { $sb.AppendLine('<br>') }
                }
                $sb.AppendLine('</samp></details></td>')
            }
            else {
                $sb.Append('<td>').Append([WebUtility]::HtmlEncode($pkg.DisplayName)).AppendLine('</td>')
            }
            if (-not $excludeEmptyColumns -or $widths.Latest -ne 0) {
                $sb.Append('<td>').Append([WebUtility]::HtmlEncode($pkg.Latest)).AppendLine('</td>')
            }
            if (-not $excludeEmptyColumns -or $widths.Vulnerable.Item3 -ne 0) {
                $sb.Append('<td>')
                [int]$n = $pkg.Vulnerable.Count - 1
                for ([int]$i = 0; $i -le $n; $i++) {
                    $sb.Append('<a href="').Append($pkg.Vulnerable[$i].Item2).Append('">')
                    $sb.Append([WebUtility]::HtmlEncode($pkg.Vulnerable[$i].Item1)).Append('</a>')
                    if ($i -ne $n) { $sb.AppendLine(', ') }
                }
                $sb.AppendLine('</td>')
            }
            if (-not $excludeEmptyColumns -or $widths.Deprecated.Item3 -ne 0) {
                $sb.Append('<td>')
                [string[]]$d = @()
                if ($pkg.Deprecated.Item1) { $d += [WebUtility]::HtmlEncode($pkg.Deprecated.Item1) }
                if ($pkg.Deprecated.Item2) { $d += [WebUtility]::HtmlEncode($pkg.Deprecated.Item2) }
                $sb.AppendJoin('<br' + [Environment]::NewLine, $d)
                $sb.AppendLine('</td>')
            }
            $sb.AppendLine('</tr>')
        }
        $sb.AppendLine('</table>')
    }
}

# .SYNOPSIS
#   'list package' column info as dictionary of (index, length) tuples from the table header.
#   Also deducts reference type (top-level/transitive) and report type from header.
class ParsedColumnInfo {
    [ReportTypes]$ReportType
    [PackageRefType]$RefType
    [Dictionary[string, ValueTuple[int, int]]]$Columns = [Dictionary[string, ValueTuple[int, int]]]::new()

    ParsedColumnInfo([string]$tableHeader) {
        [MatchCollection]$m = [regex]::Matches($tableHeader, '   [^ ].*?(?=   [^ ]|$)')
        try {
            $this.RefType = switch ($m[0].Value.Trim()) {
                'Top-level Package' { [PackageRefType]::TopLevel; break }
                'Transitive Package' { [PackageRefType]::Transitive; break }
                Default { throw }
            }
            $this.Columns.Add('Name', [ValueTuple]::Create($m[0].Index, $m[0].Length))
            $this.ReportType = [ReportTypes]::Default
            for ([int]$i = 1; $i -lt $m.Count; $i++) {
                [string]$column = $m[$i].Value.Trim()
                $this.Columns.Add($column, [ValueTuple]::Create($m[$i].Index, $m[$i].Length))
                switch ($column) {
                    'Latest' { $this.ReportType = [ReportTypes]::Outdated; break }
                    'Reason(s)' { $this.ReportType = [ReportTypes]::Deprecated; break }
                    'Severity' { $this.ReportType = [ReportTypes]::Vulnerable; break }
                }
            }
        }
        catch {
            throw "Invalid table header: $tableHeader"
        }
    }
}

# .SYNOPSIS
#   Parsed result of 'list package' output, can contain results from different report types
#   for the same projects via AddReport()
class ListPackageResult {
    # NuGet sources for Outdated, etc.
    [HashSet[string]]$Sources
    # Project or framework related messages
    [List[string]]$Messages
    # Unparsed lines, most likely error messages followed by usage info
    [List[string]]$Unparsed
    # Parsed packages with an extended identity as key
    [Dictionary[string, ParsedPackageRef]]$Packages

    ListPackageResult() {
        $this.Init()
    }

    # Construct from one 'list package' output
    # Do not concat lines from multiple outputs. Use the ctor for each and merge with AddReport().
    ListPackageResult([string[]]$dotnetLines) {
        $this.Init()
        [Queue[string]]$lines = [Queue[string]]::new($dotnetLines)
        while ($lines.Count -ne 0) {
            if ($lines.Peek().Length -eq 0) {
                $lines.Dequeue()
            }
            elseif ($lines.Peek() -ceq 'The following sources were used:') {
                $lines.Dequeue()
                while ($lines.Count -ne 0 -and $lines.Peek().Length -ne 0) {
                    $this.Sources.Add($lines.Dequeue().Trim())
                }
            }
            else {
                break
            }
        }
        while ($lines.Count -ne 0) {
            if ($lines.Peek() -match 'project (?<quote>[''`])(?<project>.+)\k<quote> (?:has |given )') {
                $this.ParseProject($lines)
            }
            else {
                [string]$line = $lines.Dequeue()
                if ($line.Length -ne 0 -and $line -cne '(A) : Auto-referenced package.') {
                    $this.Unparsed.Add($line)
                }
            }

        }
    }

    # Deserialize and create from JSON
    static [ListPackageResult]CreateFromJson([string]$jsonText) {
        [PsCustomObject]$deserialized = ConvertFrom-Json -InputObject $jsonText
        [ListPackageResult]$result = [ListPackageResult]::new()
        $result.Sources.UnionWith([string[]]$deserialized.Sources)
        $result.Messages.AddRange([string[]]$deserialized.Messages)
        $result.Unparsed.AddRange([string[]]$deserialized.Unparsed)
        foreach ($pkgobj in $deserialized.Packages) {
            [ParsedPackageRef]$pkg = [ParsedPackageRef]::new($pkgobj)
            $result.Packages.Add($pkg.Key, $pkg)
        }
        return $result
    }

    # Convert to JSON
    [string]ToJson([bool]$compress = $true) {
        return [PSCustomObject]@{
            Sources  = $this.Sources
            Messages = $this.Messages
            Unparsed = $this.Unparsed
            Packages = $this.Packages.Values.ForEach({ $_.ToSerializable() })
        } | ConvertTo-Json -Depth 4 -Compress:$compress
    }

    # Add parsed results from another report type
    [void]AddReport([ListPackageResult]$other) {
        # TODO sources should be either empty on one side or identical
        $this.Sources.UnionWith($other.Sources)
        $this.Messages.AddRange($other.Messages)
        $this.Unparsed.AddRange($other.Unparsed)
        foreach ($kvp in $other.Packages.GetEnumerator()) {
            [ParsedPackageRef]$package = $null
            if ($this.Packages.TryGetValue($kvp.Key, [ref]$package)) {
                $package.AddReport($kvp.Value)
            }
            else {
                $this.Packages.Add($kvp.Key, $kvp.Value)
            }
        }
    }

    # No ctor chaining in Pwsh
    hidden [void]Init() {
        $this.Sources = [HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        $this.Messages = [List[string]]::new()
        $this.Unparsed = [List[string]]::new()
        $this.Packages = [Dictionary[string, ParsedPackageRef]]::new([StringComparer]::OrdinalIgnoreCase)
    }

    # Parse a project section
    hidden [void]ParseProject([Queue[string]]$lines) {
        if ($lines.Peek() -cmatch '^The given project (?<quote>[''`])(?<project>.+)\k<quote> has no |^No packages were found for the project (?<quote>[''`])(?<project>.+)\k<quote> given ') {
            $this.Messages.Add($lines.Dequeue())
            return
        }
        if ($lines.Peek() -cnotmatch '^Project (?<quote>[''`])(?<project>.+)\k<quote> has the following ') {
            throw "First line is not a project line: $($lines.Peek())"
        }
        [string]$project = $Matches['project']
        $lines.Dequeue()
        while ($lines.Count -ne 0) {
            if ($lines.Peek() -cnotmatch '^   \[(?<framework>[^\]]+)\]: (?<message>.*)') { break }
            $this.ParseFramework($project, $lines)
        }
    }

    # Parse a framework section
    hidden [void]ParseFramework([string]$project, [Queue[string]]$lines) {
        if ($lines.Peek() -cnotmatch '^   \[(?<framework>[^\]]+)\]: (?<message>.*)') {
            throw "First line is not a framework line: $($lines.Peek())"
        }
        [string]$framework = $Matches['framework']
        if ($Matches['message']) {
            $this.Messages.Add($lines.Dequeue().Trim())
            return
        }
        $lines.Dequeue()
        while ($lines.Count -ne 0 -and $lines.Peek() -cmatch '^   Top-level|^   Transitive') {
            [ParsedColumnInfo]$columnInfo = [ParsedColumnInfo]::new($lines.Dequeue())
            while ($lines.Count -ne 0 -and $lines.Peek().StartsWith('   > ')) {
                [ParsedPackageRef]$package = [ParsedPackageRef]::new($project, $framework, $columnInfo, $lines)
                $this.Packages.Add($package.Key, $package)
            }
            while ($lines.Count -ne 0 -and $lines.Peek().Length -eq 0) { $lines.Dequeue() }
        }
    }
}

# .SYNOPSIS
#   Comparison of two ListPackageResult.
# .NOTES
#   Only meaningful if the results have been created for the same projects and from the
#   same set of merged report types.
class ListPackageComparison {
    [HashSet[string]]$LeftOnly
    [HashSet[string]]$RightOnly
    [HashSet[string]]$Changed
    [HashSet[string]]$Unchanged

    ListPackageComparison([ListPackageResult]$left, [ListPackageResult]$right) {
        [HashSet[string]]$keys1 = [HashSet[string]]::new($left.Packages.Keys, [StringComparer]::OrdinalIgnoreCase)
        [HashSet[string]]$keys2 = [HashSet[string]]::new($right.Packages.Keys, [StringComparer]::OrdinalIgnoreCase)
        $this.LeftOnly = [HashSet[string]]::new($keys1, [StringComparer]::OrdinalIgnoreCase)
        $this.LeftOnly.ExceptWith($keys2)
        $this.RightOnly = [HashSet[string]]::new($keys2, [StringComparer]::OrdinalIgnoreCase)
        $this.RightOnly.ExceptWith($keys1)
        $keys1.IntersectWith($keys2)
        $this.Changed = [HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        $this.Unchanged = [HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        foreach ($key in $keys1) {
            if ($left.Packages[$key].Equals($right.Packages[$key])) {
                $this.Unchanged.Add($key)
            }
            else {
                $this.Changed.Add($key)
            }
        }
    }
}

# .SYNOPSIS
#   Invoke 'dotnet list package' for given projects and options matrix
# .OUTPUTS
#   [ListPackageResult]
# .NOTES
#   Writes the raw tool output to the Information stream.
#   Use -InformationAction or -InformationVariable to log or access the raw output.
function Invoke-ListPackage {
    [CmdletBinding()]
    [OutputType([ListPackageResult])]
    param (
        # Project or solution files to process. Defaults to the solution or project
        # in the current directory
        [Parameter(Position = 0)]
        [Alias('p')]
        [string[]]$Projects,

        # 'list package' options to run for each project/solution
        [ValidateNotNullOrEmpty()]
        [Alias('o')]
        [string[][]]$OptionsMatrix = (
            ('--outdated', '--include-transitive'),
            ('--vulnerable', '--include-transitive'),
            ('--deprecated', '--include-transitive')
        ),

        # Remove transitive packages that are only marked as outdated, but not
        # vulnerable or deprecated.
        [Alias('r')]
        [switch]$RemoveTransitiveIfOutdatedOnly
    )

    [ListPackageResult]$allResults = [ListPackageResult]::new()

    if (-not $Projects) {
        $Projects = @('')
    }

    foreach ($project in $Projects) {
        foreach ($options in $OptionsMatrix) {
            $retval = $null
            $output = $null
            [string[]] $params = @('list', $project, "package") + $options
            try {
                $retval = Start-NativeExecution { dotnet $params 2>&1 } -OutVariable output
                [ListPackageResult]$result = [ListPackageResult]::new($retval)
                if ($result.Unparsed) {
                    throw 'ListPackageResult returned unparsed lines.'
                }
                $allResults.AddReport($result)
            }
            finally {
                # Caller can control console logging via -InformationAction
                Write-Information ('dotnet ' + $params)
                if ($retval) {
                    $retval | Write-Information
                }
                else {
                    $output | Write-Information
                }
            }
        }
    }
    if ($RemoveTransitiveIfOutdatedOnly) {
        [string[]]$keys = ($allResults.Packages.GetEnumerator() | Where-Object {
                $_.Value.RefType -eq [PackageRefType]::Transitive -and $_.Value.ReportTypes -eq [ReportTypes]::Outdated
            })?.Key
        foreach ($key in $keys) {
            $null = $allResults.Packages.Remove($key)
        }
    }
    return $allResults
}

Export-ModuleMember -Function @(
    'Invoke-ListPackage'
)
