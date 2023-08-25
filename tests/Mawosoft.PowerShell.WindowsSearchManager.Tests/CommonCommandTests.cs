// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class CommonCommandTests : CommandTestBase
{

    private static string GetCommandAndFirstParameter(string command, out string? firstParameter)
    {
        firstParameter = null;
        string[] split = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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

    private static (string Command, string? Parameter) GetCommandAndFirstParameter(string command)
    {
        string cmd = GetCommandAndFirstParameter(command, out string? firstParameter);
        return (cmd, firstParameter);
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
                a => a.GetData(m1).Select(
                    d => (GetCommandAndFirstParameter((string)d.First(), out _), "Path"))))
            .Concat(m3.GetCustomAttributes<DataAttribute>().SelectMany(
                a => a.GetData(m1).Select(
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
        @"Add-SearchRoot -InputObject @{ Path = 'x:\' }, @{ Path = 'y:\' } ", // 2
        @"Remove-SearchRoot x:\ ",
        @"Remove-SearchRoot x:\, y:\ ", // 2
        @"Add-SearchRule x:\foo -RuleType Exclude ",
        @"Add-SearchRule x:\bar, x:\buzz -RuleType Include ", // 2
        @"Add-SearchRule -InputObject @{ Path = 'x:\foo'; RuleType = 'Exclude' } ",
        @"Remove-SearchRule x:\foo ",
        @"Remove-SearchRule x:\foo, x:\bar Default ", // 2
        @"Reset-SearchRule "
    }.AsRows()
     .ToArray();

    [Theory]
    [MemberData(nameof(ShouldProcess_TestData))]
    public void WhatIf_Succeeds(string command)
    {
        Collection<PSObject> results = InvokeScript(command + " -WhatIf ");
        Assert.False(PowerShell.HadErrors);
        Assert.False(InterfaceChain.HasWriteRecordings());
        Assert.Empty(results);
    }

    [Theory]
    [MemberData(nameof(ShouldProcess_TestData))]
    public void ShouldProcess_CallCount_Matches(string command)
    {
        _ = InvokeScript(command + " -Verbose ");
        Assert.False(PowerShell.HadErrors);
        Assert.True(InterfaceChain.HasWriteRecordings());
        // A bit hacky. For commands that may call ShouldProcess() multiple times,
        // the test commands contain an array with two elements, separated by comma.
        // This is the only use of comma in the command.
        // Note: This will fail if there are other verbose outputs than from ShouldProcess().
        int verboseCount = command.IndexOf(',') < 0 ? 1 : 2;
        Assert.Equal(verboseCount, PowerShell.Streams.Verbose.Count);
    }

    private static readonly string[] s_CatalogCommands =
    {
        @"Reset-SearchCatalog ",
        @"Set-SearchCatalog -ConnectTimeout 100 ",
        @"Update-SearchCatalog -All",
        @"Update-SearchCatalog -RootPath x:\ ",
        @"Update-SearchCatalog -Path x:\foo ",
        @"Add-SearchRoot x:\ ",
        @"Get-SearchRoot ",
        @"Remove-SearchRoot x:\ ",
        @"Add-SearchRule x:\foo -RuleType Exclude ",
        @"Get-SearchRule ",
        @"Remove-SearchRule x:\foo ",
        @"Reset-SearchRule ",
        @"Test-SearchRule x:\foo ",
    };

    private static readonly string[] s_CatalogValidationOnlyCommands =
    {
        @"Get-SearchCatalog ",
        @"New-SearchCatalog ",
        @"Remove-SearchCatalog ",
    };

    public static readonly object?[][] CatalogSelection_TestData = s_CatalogCommands
        .CrossJoin(new string?[] { null, "SecondCatalog" })
        .ToArray();

    public static readonly object?[][] CatalogValidation_TestData = s_CatalogCommands
        .Concat(s_CatalogValidationOnlyCommands)
        .CrossJoin(new string[] { "", "''", "$null" })
        .ToArray();

    [Theory]
    [MemberData(nameof(CatalogSelection_TestData))]
    public void CatalogParameter_CatalogSelection_Succeeds(string command, string? catalog)
    {
        if (catalog is null)
        {
            catalog = SearchApiCommandBase.DefaultCatalogName;
        }
        else
        {
            command += $" -Catalog {catalog} ";
        }
        _ = InvokeScript(command);
        Assert.False(PowerShell.HadErrors);
        Assert.Equal($"GetCatalog({catalog})", Assert.Single(InterfaceChain.SearchManager.RecordedCalls));
    }

    [Theory]
    [MemberData(nameof(CatalogValidation_TestData))]
    public void CatalogParameter_ParameterValidation_Succeeds(string command, string catalog)
    {
        command += $" -Catalog {catalog} ";
        AssertParameterValidation(command, "Catalog");
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
    public void PathParameter_ParameterValidation_Succeeds(string command, string path)
    {
        command += $" -Path {path} ";
        AssertParameterValidation(command, "Path");
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
    [InlineData(@"Add-SearchRoot -InputObject @{ foo = 'bar' } ")]
    [InlineData(@"Add-SearchRule -RuleSet 2 -Path x:\foo -RuleType Include ")]
    [InlineData(@"Add-SearchRule -RuleType 2 -Path x:\foo ")]
    [InlineData(@"Add-SearchRule -InputObject @{ foo = 'bar' } ")]
    [InlineData(@"Remove-SearchRule -RuleSet system -Path x:\foo ")]
    public void OtherParameter_ParameterValidation_Succeeds(string command)
    {
        _ = GetCommandAndFirstParameter(command, out string? firstParameter);
        AssertParameterValidation(command, firstParameter);
    }
}
