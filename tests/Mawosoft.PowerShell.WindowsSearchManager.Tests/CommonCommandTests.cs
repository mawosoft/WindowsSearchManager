// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class CommonCommandTests : CommandTestBase
{

    private static void Assert_TestDataCoverage(
        List<(Type Type, string Name)> expectedCommands,
        Delegate method)
    {
        MethodInfo m = method.GetMethodInfo();
        Assert.NotNull(m);
        List<object[]> data = new(
            m.GetCustomAttributes<DataAttribute>().SelectMany(a => a.GetData(m)));
        Type type = data.First().First().GetType();
        if (type == typeof(string))
        {
            HashSet<string> expected = new(expectedCommands.ConvertAll(vt => vt.Name));
            HashSet<string> actual = new(
                data.ConvertAll(d => ((string)d.First()).Split(' ').First()));
            Assert.Empty(expected.Except(actual));
            Assert.Empty(actual.Except(expected));
        }
        else if (type == typeof(string))
        {
            HashSet<Type> expected = new(expectedCommands.ConvertAll(vt => vt.Type));
            HashSet<Type> actual = new(data.Select(d => (Type)d.First()));
            Assert.Empty(expected.Except(actual));
            Assert.Empty(actual.Except(expected));
        }
        else
        {
            Assert.Fail("Expected value of type 'string' or 'Type' as first parameter");
        }
    }

    [Fact]
    public void CommonTests_TestData_CoverExpectedCommands()
    {
        var commandsWithCatalogValidation = AllCommands.FindAll(
            vt => vt.Type != typeof(GetSearchManagerCommand)
                  && vt.Type != typeof(SetSearchManagerCommand));
        var commandsWithCatalogSelection = commandsWithCatalogValidation.FindAll(
            vt => vt.Type != typeof(GetSearchCatalogCommand)
                  && vt.Type != typeof(NewSearchCatalogCommand)
                  && vt.Type != typeof(RemoveSearchCatalogCommand));

        Assert_TestDataCoverage(AllCommands, ConfirmImpact_Matches_SupportsShouldProcess);
        Assert_TestDataCoverage(CommandsSupportingShouldProcess, WhatIf_Succeeds);
        Assert_TestDataCoverage(commandsWithCatalogSelection, CatalogParameter_CatalogSelection_Succeeds);
        Assert_TestDataCoverage(commandsWithCatalogValidation, CatalogParameter_ParameterValidation_Succeeds);
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

    private static readonly string[] s_WhatifCommands =
    {
        @"Set-SearchManager -UserAgent foo-agent ",
        @"Set-SearchManager -ProxyAccess PROXY_ACCESS_DIRECT ",
        @"Set-SearchManager -UserAgent foo-agent -ProxyAccess PROXY_ACCESS_DIRECT ",
        @"Set-SearchManager -ProxyAccess PROXY_ACCESS_PROXY -ProxyName bar.com -ProxyPortNumber 0x8080 -ProxyBypassLocal -ProxyBypassList buzz.com,baz.org ",
        @"New-SearchCatalog newcat ",
        @"Remove-SearchCatalog oldcat ",
        @"Reset-SearchCatalog ",
        @"Set-SearchCatalog -ConnectTimeout 100 ",
        @"Set-SearchCatalog -DataTimeout 100 ",
        @"Set-SearchCatalog -DiacriticSensitivity ",
        @"Set-SearchCatalog -ConnectTimeout 100 -DataTimeout 100 -DiacriticSensitivity ",
        @"Update-SearchCatalog ",
        @"Update-SearchCatalog -RootPath x:\ ",
        @"Update-SearchCatalog -Path x:\foo ",
        @"Add-SearchRoot x:\ ",
        @"Add-SearchRoot -InputObject @{ Path = 'x:\' } ",
        @"Remove-SearchRoot x:\ ",
        @"Add-SearchRule x:\foo -RuleType Exclude ",
        @"Add-SearchRule -InputObject @{ Path = 'x:\foo'; RuleType = 'Exclude' } ",
        @"Remove-SearchRule x:\foo ",
        @"Remove-SearchRule x:\foo Default ",
        @"Reset-SearchRule ",
    };

    public static IEnumerable<object[]> WhatIf_TestData()
        => CombineTestDataParameters(s_WhatifCommands, new[] { true, false });

    [Theory]
    [MemberData(nameof(WhatIf_TestData))]
    public void WhatIf_Succeeds(string command, bool whatIf)
    {
        if (whatIf) command += " -WhatIf ";
        Collection<PSObject> results = InvokeScript(command);
        Assert.Equal(!whatIf, InterfaceChain.HasWriteRecordings());
        if (whatIf) Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
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

    public static IEnumerable<object[]> CatalogSelection_TestData()
        => CombineTestDataParameters(s_CatalogCommands, new string?[] { null, "SecondCatalog" });

    public static IEnumerable<object[]> CatalogValidation_TestData()
        => CombineTestDataParameters(
            s_CatalogCommands.Concat(s_CatalogValidationOnlyCommands),
            new string[] { "", "''", "$null" });

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
}
