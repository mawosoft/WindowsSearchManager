// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class CommonCommandTests : CommandTestBase
{
    [Fact]
    public void AllParameterMetadata_MatchExpected()
    {
        var expected = new[]
        {
            // Command, Parameter, Type, Position, IsMandatory, ValueFromPipeline, ValueFromPipelineByPropertyName, ParameterSet
            ("Get-SearchCatalog", "Catalog", "String", 0, false, false, false, "__AllParameterSets"),
            ("Set-SearchCatalog", "ConnectTimeout", "UInt32", -2147483648, false, false, false, "__AllParameterSets"),
            ("Set-SearchCatalog", "DataTimeout", "UInt32", -2147483648, false, false, false, "__AllParameterSets"),
            ("Set-SearchCatalog", "DiacriticSensitivity", "SwitchParameter", -2147483648, false, false, false, "__AllParameterSets"),
            ("Set-SearchCatalog", "Catalog", "String", -2147483648, false, false, false, "__AllParameterSets"),
            ("Reset-SearchCatalog", "Catalog", "String", 0, false, false, false, "__AllParameterSets"),
            ("Update-SearchCatalog", "All", "SwitchParameter", -2147483648, false, false, false, "AllParameterSet"),
            ("Update-SearchCatalog", "RootPath", "String[]", -2147483648, true, false, true, "RootParameterSet"),
            ("Update-SearchCatalog", "Path", "String[]", 0, true, true, true, "PathParameterSet"),
            ("Update-SearchCatalog", "Catalog", "String", -2147483648, false, false, false, "__AllParameterSets"),
            ("New-SearchCatalog", "Catalog", "String", 0, true, false, false, "__AllParameterSets"),
            ("Remove-SearchCatalog", "Catalog", "String", 0, true, false, false, "__AllParameterSets"),
            ("Get-SearchManager", "", "", -2147483648, false, false, false, ""),
            ("Set-SearchManager", "UserAgent", "String", -2147483648, false, false, false, "__AllParameterSets"),
            ("Set-SearchManager", "ProxyAccess", "_PROXY_ACCESS", -2147483648, false, false, false, "__AllParameterSets"),
            ("Set-SearchManager", "ProxyName", "String", -2147483648, false, false, false, "__AllParameterSets"),
            ("Set-SearchManager", "ProxyPortNumber", "UInt32", -2147483648, false, false, false, "__AllParameterSets"),
            ("Set-SearchManager", "ProxyBypassLocal", "SwitchParameter", -2147483648, false, false, false, "__AllParameterSets"),
            ("Set-SearchManager", "ProxyBypassList", "String[]", -2147483648, false, false, false, "__AllParameterSets"),
            ("Get-SearchRoot", "Catalog", "String", 0, false, false, false, "__AllParameterSets"),
            ("Get-SearchRoot", "PathOnly", "SwitchParameter", -2147483648, false, false, false, "__AllParameterSets"),
            ("Add-SearchRoot", "Path", "String[]", 0, true, true, false, "PathParameterSet"),
            ("Add-SearchRoot", "InputObject", "SearchRootInfo[]", -2147483648, true, true, false, "InputParameterSet"),
            ("Add-SearchRoot", "Catalog", "String", -2147483648, false, false, false, "__AllParameterSets"),
            ("Remove-SearchRoot", "Path", "String[]", 0, true, true, true, "__AllParameterSets"),
            ("Remove-SearchRoot", "Catalog", "String", -2147483648, false, false, false, "__AllParameterSets"),
            ("Get-SearchRule", "Catalog", "String", 0, false, false, false, "__AllParameterSets"),
            ("Add-SearchRule", "Path", "String[]", 0, true, true, false, "PathParameterSet"),
            ("Add-SearchRule", "RuleType", "SearchRuleType", 1, true, false, false, "PathParameterSet"),
            ("Add-SearchRule", "RuleSet", "SearchRuleSet", 2, false, false, false, "PathParameterSet"),
            ("Add-SearchRule", "InputObject", "SearchRuleInfo[]", -2147483648, true, true, false, "InputParameterSet"),
            ("Add-SearchRule", "OverrideChildren", "SwitchParameter", -2147483648, false, false, false, "__AllParameterSets"),
            ("Add-SearchRule", "Catalog", "String", -2147483648, false, false, false, "__AllParameterSets"),
            ("Remove-SearchRule", "Path", "String[]", 0, true, true, true, "__AllParameterSets"),
            ("Remove-SearchRule", "RuleSet", "SearchRuleSet", 1, false, false, true, "__AllParameterSets"),
            ("Remove-SearchRule", "Catalog", "String", -2147483648, false, false, false, "__AllParameterSets"),
            ("Reset-SearchRule", "Catalog", "String", 0, false, false, false, "__AllParameterSets"),
            ("Test-SearchRule", "Path", "String[]", 0, true, true, true, "__AllParameterSets"),
            ("Test-SearchRule", "IsIncluded", "SwitchParameter", -2147483648, false, false, false, "IncludedParameterSet"),
            ("Test-SearchRule", "HasChildScope", "SwitchParameter", -2147483648, true, false, false, "ChildScopeParameterSet"),
            ("Test-SearchRule", "HasParentScope", "SwitchParameter", -2147483648, true, false, false, "ParentScopeParameterSet"),
            ("Test-SearchRule", "Detailed", "SwitchParameter", -2147483648, true, false, false, "DetailedParameterSet"),
            ("Test-SearchRule", "Catalog", "String", -2147483648, false, false, false, "__AllParameterSets"),
        }
        .ToHashSet();

        var actual = AllCommands.SelectMany(vt =>
        {
            CommandMetadata meta = new(vt.Type);
            return meta.Parameters.DefaultIfEmpty().SelectMany(param =>
            {
                var sets = param.Value is null
                    ? new Dictionary<string, ParameterSetMetadata>()
                    : param.Value.ParameterSets;
                return sets.DefaultIfEmpty().Select(set => (
                    Command: meta.Name,
                    Parameter: param.Key ?? "",
                    Type: param.Value?.ParameterType.Name ?? "",
                    Position: set.Value?.Position ?? int.MinValue,
                    IsMandatory: set.Value?.IsMandatory ?? false,
                    ValueFromPipeline: set.Value?.ValueFromPipeline ?? false,
                    ValueFromPipelineByPropertyName: set.Value?.ValueFromPipelineByPropertyName ?? false,
                    ParameterSet: set.Key ?? ""));
            });
        })
        .ToHashSet();
#if false
        // Using an anonymous type instead of ValueTuple, because ValueTuple is limited to
        // 7 fields + TRest, which again is a ValueTuple.
        // Also, the VStudio debugger's CSV export is unwieldy. Hence we are creating a string
        // that can be copied to the clipboard.
        var csvrows = actual.Select((vt, i) => new
        {
            Index = i,
            vt.Command,
            vt.Parameter,
            vt.Type,
            Position = vt.Position == int.MinValue ? null : $"{vt.Position}",
            Mandatory = vt.IsMandatory ? "m" : null,
            Pipeline = vt.ValueFromPipeline
                ? vt.ValueFromPipelineByPropertyName ? "pipe,prop" : "pipe"
                : vt.ValueFromPipelineByPropertyName ? "prop" : null,
            vt.ParameterSet
        });
        PropertyInfo[] props = csvrows.First().GetType().GetProperties();
        string sep = "\t";
        string csv = string.Join(
            Environment.NewLine,
            csvrows.Select(r => string.Join(sep, props.Select(p => $"\"{p.GetValue(r)}\"")))
                   .Prepend(string.Join(sep, props.Select(p => $"\"{p.Name}\""))));
#endif
        Assert.Equal(expected, actual);
    }

    private static string GetCommandAndFirstParameter(string script, out string? firstParameter)
    {
        firstParameter = null;
        string[] split = script.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (split.Length > 1)
        {
            string first = split[1].Trim();
            if (first.StartsWith("-", StringComparison.Ordinal))
            {
                firstParameter = first.Substring(1);
            }
        }
        return split[0].Trim();
    }

    private static (string Command, string? Parameter) GetCommandAndFirstParameter(string script)
    {
        string command = GetCommandAndFirstParameter(script, out string? firstParameter);
        return (command, firstParameter);
    }

    private static void Assert_TestDataCoverage(
        List<(Type Type, string Name)> expectedCommands,
        Delegate method)
    {
        MethodInfo m = method.GetMethodInfo();
        Assert.NotNull(m);
        var data = m.GetCustomAttributes<DataAttribute>().SelectMany(a => a.GetData(m)).ToList();
        var expected = expectedCommands.ConvertAll(vt => vt.Name).ToHashSet();
        var actual = data.ConvertAll(d => GetCommandAndFirstParameter((string)d.First(), out _)).ToHashSet();
        Assert.Empty(expected.Except(actual));
        Assert.Empty(actual.Except(expected));
    }

    [Fact]
    public void CommonTests_TestData_CoverExpectedCommands()
    {
        var commandsWithCatalogSelection = AllCommands.FindAll(
            vt => vt.Type != typeof(GetSearchManagerCommand)
                  && vt.Type != typeof(SetSearchManagerCommand)
                  && vt.Type != typeof(GetSearchCatalogCommand)
                  && vt.Type != typeof(NewSearchCatalogCommand)
                  && vt.Type != typeof(RemoveSearchCatalogCommand));

        Assert_TestDataCoverage(AllCommands, ConfirmImpact_Matches_SupportsShouldProcess);
        Assert_TestDataCoverage(CommandsSupportingShouldProcess, WhatIf_Succeeds);
        Assert_TestDataCoverage(CommandsSupportingShouldProcess, ShouldProcess_CallCount_Matches);
        Assert_TestDataCoverage(commandsWithCatalogSelection, CatalogParameter_CatalogSelection_Succeeds);
    }

    [Fact]
    public void CommonTests_TestData_CoverExpectedParameters()
    {
        var expected = AllCommands.SelectMany(
            vt => vt.Type.GetProperties().Where(pi => pi.PropertyType != typeof(SwitchParameter))
            .Cast<MemberInfo>()
            .Concat(vt.Type.GetFields().Where(fi => fi.FieldType != typeof(SwitchParameter)))
            .Where(mi => mi.GetCustomAttribute<ParameterAttribute>() is not null)
            .Select(mi => (Command: vt.Name, Parameter: mi.Name)))
            .ToHashSet();
        MethodInfo m1 = ((Delegate)CatalogParameter_ParameterValidation_Succeeds).GetMethodInfo();
        MethodInfo m2 = ((Delegate)PathParameter_ParameterValidation_Succeeds).GetMethodInfo();
        MethodInfo m3 = ((Delegate)OtherParameter_ParameterValidation_Succeeds).GetMethodInfo();
        var actual =
            m1.GetCustomAttributes<DataAttribute>().SelectMany(
                a => a.GetData(m1).Select(
                    d => (GetCommandAndFirstParameter((string)d.First(), out _), "Catalog")))
            .Concat(m2.GetCustomAttributes<DataAttribute>().SelectMany(
                a => a.GetData(m2).Select(
                    d => (GetCommandAndFirstParameter((string)d.First(), out _), "Path"))))
            .Concat(m3.GetCustomAttributes<DataAttribute>().SelectMany(
                a => a.GetData(m3).Select(
                    d => (ValueTuple<string, string>)GetCommandAndFirstParameter((string)d.First())!)))
            .ToHashSet();
        Assert.Empty(expected.Except(actual));
        Assert.Empty(actual.Except(expected));
    }

    [Theory]
    [InlineData("Get-SearchManager", ConfirmImpact.None)]
    [InlineData("Set-SearchManager", ConfirmImpact.Medium)]
    [InlineData("Get-SearchCatalog", ConfirmImpact.None)]
    [InlineData("New-SearchCatalog", ConfirmImpact.Low)]
    [InlineData("Remove-SearchCatalog", ConfirmImpact.High)]
    [InlineData("Reset-SearchCatalog", ConfirmImpact.Medium)]
    [InlineData("Set-SearchCatalog", ConfirmImpact.Medium)]
    [InlineData("Update-SearchCatalog", ConfirmImpact.Medium)]
    [InlineData("Add-SearchRoot", ConfirmImpact.Medium)]
    [InlineData("Get-SearchRoot", ConfirmImpact.None)]
    [InlineData("Remove-SearchRoot", ConfirmImpact.High)]
    [InlineData("Add-SearchRule", ConfirmImpact.Medium)]
    [InlineData("Get-SearchRule", ConfirmImpact.None)]
    [InlineData("Remove-SearchRule", ConfirmImpact.Medium)]
    [InlineData("Reset-SearchRule", ConfirmImpact.High)]
    [InlineData("Test-SearchRule", ConfirmImpact.None)]
    public void ConfirmImpact_Matches_SupportsShouldProcess(string commandName, ConfirmImpact confirmImpact)
    {
        Type commandType = AllCommands.Find(vt => vt.Name == commandName).Type;
        Assert.NotNull(commandType);
        CmdletAttribute a = commandType.GetCustomAttribute<CmdletAttribute>()!;
        Assert.NotNull(a);
        Assert.Equal(confirmImpact, a.ConfirmImpact);
        Assert.Equal(confirmImpact != ConfirmImpact.None, a.SupportsShouldProcess);
    }

    public static readonly object?[][] ShouldProcess_TestData = new string[]
    {
        @"Set-SearchManager -UserAgent foo-agent ",
        @"Set-SearchManager -ProxyAccess PROXY_ACCESS_DIRECT ",
        @"Set-SearchManager -UserAgent foo-agent -ProxyAccess PROXY_ACCESS_DIRECT ",
        @"New-SearchCatalog newcat ",
        @"Remove-SearchCatalog oldcat ",
        @"Reset-SearchCatalog ",
        @"Set-SearchCatalog -ConnectTimeout 100 ",
        @"Update-SearchCatalog ",
        @"Update-SearchCatalog -RootPath x:\, y:\ ", // 2
        @"Update-SearchCatalog -Path x:\foo, x:\bar ", // 2
        @"Add-SearchRoot x:\ ",
        @"Add-SearchRoot -InputObject @([pscustomobject]@{ Path = 'x:\' }, [pscustomobject]@{ Path = 'y:\' }) ", // 2
        @"Remove-SearchRoot x:\ ",
        @"Remove-SearchRoot x:\, y:\ ", // 2
        @"Add-SearchRule x:\foo -RuleType Exclude ",
        @"Add-SearchRule x:\bar, x:\buzz -RuleType Include ", // 2
        @"Add-SearchRule -InputObject ([pscustomobject]@{ Path = 'x:\foo'; RuleType = 'Exclude' }) ",
        @"Remove-SearchRule x:\foo ",
        @"Remove-SearchRule x:\foo, x:\bar Default ", // 2
        @"Reset-SearchRule "
    }.AsRows()
     .ToArray();

    [Theory]
    [MemberData(nameof(ShouldProcess_TestData))]
    public void WhatIf_Succeeds(string script)
    {
        Collection<PSObject> results = InvokeScript(script + " -WhatIf ");
        Assert.False(PowerShell.HadErrors);
        Assert.False(InterfaceChain.HasWriteRecordings());
        Assert.Empty(results);
    }

    [Trait("WSearch", "IsEnabled")]
    [SkippableTheory(SkipCondition.WSearchDisabled)]
    [MemberData(nameof(ShouldProcess_TestData))]
    public void ShouldProcess_CallCount_Matches(string script)
    {
        _ = InvokeScript(script + " -Verbose ");
        Assert.False(PowerShell.HadErrors);
        Assert.True(InterfaceChain.HasWriteRecordings());
        // A bit hacky. For commands that may call ShouldProcess() multiple times,
        // the test commands contain an array with two elements, separated by comma.
        // This is the only use of comma in the command.
        // Note: This will fail if there are other verbose outputs than from ShouldProcess().
        int verboseCount = script.IndexOf(',') < 0 ? 1 : 2;
        Assert.Equal(verboseCount, PowerShell.Streams.Verbose.Count);
    }

    private static readonly string[] s_CatalogCommands =
    {
        @"Reset-SearchCatalog {catalog} ",
        @"Set-SearchCatalog -ConnectTimeout 100 ",
        @"Update-SearchCatalog -All",
        @"Update-SearchCatalog -RootPath x:\ ",
        @"Update-SearchCatalog -Path x:\foo ",
        @"Add-SearchRoot x:\ ",
        @"Get-SearchRoot {catalog} ",
        @"Remove-SearchRoot x:\ ",
        @"Add-SearchRule x:\foo -RuleType Exclude -RuleSet User ",
        @"Get-SearchRule {catalog} ",
        @"Remove-SearchRule x:\foo User ",
        @"Reset-SearchRule {catalog} ",
        @"Test-SearchRule x:\foo ",
    };

    private static readonly string[] s_CatalogValidationOnlyCommands =
    {
        @"Get-SearchCatalog {catalog} ",
        @"New-SearchCatalog {catalog} ",
        @"Remove-SearchCatalog {catalog} ",
    };

    public static readonly object?[][] CatalogSelection_TestData = s_CatalogCommands
        .CrossJoin(new object?[][]
        {
            new object?[] { null, false },
            new object?[] { "SecondCatalog", false },
            new object?[] { "SecondCatalog", true }
        })
        .ToArray();

    public static readonly object?[][] CatalogValidation_TestData = s_CatalogCommands
        .Concat(s_CatalogValidationOnlyCommands)
        .CrossJoin(new string[] { "", "''", "$null" })
        .ToArray();

    [Trait("WSearch", "IsEnabled")]
    [SkippableTheory(SkipCondition.WSearchDisabled)]
    [MemberData(nameof(CatalogSelection_TestData))]
    public void CatalogParameter_CatalogSelection_Succeeds(string script, string? catalog, bool positional)
    {
        if (catalog is null)
        {
            catalog = SearchApiCommandBase.DefaultCatalogName;
            script = script.Replace("{catalog}", "");
        }
        else
        {
            if (positional && script.Contains("{catalog}"))
            {
                script = script.Replace("{catalog}", catalog);
            }
            else
            {
                script = script.Replace("{catalog}", "");
                script += $" -Catalog {catalog} ";
            }
        }
        _ = InvokeScript(script);
        Assert.False(PowerShell.HadErrors);
        Assert.Equal($"GetCatalog({catalog})", Assert.Single(InterfaceChain.SearchManager.RecordedCalls));
    }

    [Theory]
    [MemberData(nameof(CatalogValidation_TestData))]
    public void CatalogParameter_ParameterValidation_Succeeds(string script, string catalog)
    {
        script = script.Replace("{catalog}", "");
        script += $" -Catalog {catalog} ";
        AssertParameterValidation(script, "Catalog");
    }

    public static readonly object?[][] PathValidation_TestData = new string[]
    {
        @"Update-SearchCatalog ",
        @"Add-SearchRoot ",
        @"Remove-SearchRoot ",
        @"Add-SearchRule -RuleType Exclude ",
        @"Remove-SearchRule ",
        @"Test-SearchRule "
    }.CrossJoin(new string[] { "", "''", "$null", "@()", @"@('x:\foo', '')", @"@('x:\foo', $null)" })
     .ToArray();

    [Theory]
    [MemberData(nameof(PathValidation_TestData))]
    public void PathParameter_ParameterValidation_Succeeds(string script, string path)
    {
        script += $" -Path {path} ";
        AssertParameterValidation(script, "Path");
    }

    // The parameters -Catalog and -Path have separate tests.
    // Switches are excluded.
    [Theory]
    [InlineData(@"Set-SearchManager -UserAgent ")]
    [InlineData(@"Set-SearchManager -UserAgent '' ")]
    [InlineData(@"Set-SearchManager -ProxyAccess 3 ")]
    [InlineData(@"Set-SearchManager -ProxyName ")]
    [InlineData(@"Set-SearchManager -ProxyPortNumber -1 ")]
    [InlineData(@"Set-SearchManager -ProxyPortNumber 65536 ")]
    [InlineData(@"Set-SearchManager -ProxyBypassList ")]
    [InlineData(@"Set-SearchManager -ProxyBypassList $null ")]
    [InlineData(@"Set-SearchManager -ProxyBypassList @('foo', $null) ")]
    [InlineData(@"Set-SearchCatalog -ConnectTimeout -1 ")]
    [InlineData(@"Set-SearchCatalog -DataTimeout -1 ")]
    [InlineData(@"Update-SearchCatalog -RootPath ")]
    [InlineData(@"Update-SearchCatalog -RootPath '' ")]
    [InlineData(@"Update-SearchCatalog -RootPath @() ")]
    [InlineData(@"Update-SearchCatalog -RootPath @('x:\foo', '') ")]
    [InlineData(@"Update-SearchCatalog -RootPath @('x:\foo', $null) ")]
    [InlineData(@"Add-SearchRoot -InputObject ([pscustomobject]@{ foo = 'bar' }) ")]
    [InlineData(@"Add-SearchRule -RuleSet 2 -Path x:\foo -RuleType Include ")]
    [InlineData(@"Add-SearchRule -RuleType 2 -Path x:\foo ")]
    [InlineData(@"Add-SearchRule -InputObject ([pscustomobject]@{ foo = 'bar' }) ")]
    [InlineData(@"Remove-SearchRule -RuleSet system -Path x:\foo ")]
    public void OtherParameter_ParameterValidation_Succeeds(string script)
    {
        _ = GetCommandAndFirstParameter(script, out string? firstParameter);
        AssertParameterValidation(script, firstParameter);
    }
}
