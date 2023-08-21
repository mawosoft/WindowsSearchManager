// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchCatalogCommandsTests : CommandTestBase
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("SecondCatalog", false)]
    [InlineData("ThirdCatalog", true)]
    public void GetSearchCatalog_Succeeds(string? catalogName, bool positional)
    {
        MockSearchManager searchManager = new();
        InterfaceChain.WithSearchManager(searchManager);
        List<MockCatalogManager> expected = searchManager.CatalogManagers;
        InterfaceChain.Factory.SearchRegistryProvider.CatalogNames = searchManager.CatalogManagers.ConvertAll(c => c.NameInternal);
        string script = "Get-SearchCatalog ";
        if (catalogName is not null)
        {
            if (!positional) script += "-Catalog ";
            script += $"'{catalogName}' ";
            expected = expected.FindAll(c => c.NameInternal == catalogName);
        }
        Collection<PSObject> results = InvokeScript(script);
        Assert.False(InterfaceChain.HasWriteRecordings());
        Assert.All(results, (item, i) => Assert.Equal(expected[i], item.BaseObject, SearchCatalogInfoToMockComparer.Instance));
        Assert.Equal(expected.Count, results.Count);
        Assert.False(PowerShell.HadErrors);
    }

    [Fact]
    public void GetSearchCatalog_WithFailures_Succeeds()
    {
        MockSearchManager searchManager = new();
        InterfaceChain.WithSearchManager(searchManager);
        List<ISearchCatalogManager> expected = new(searchManager.CatalogManagers);
        MockCatalogManager catalogManager = new() { NameInternal = "BadCatalog1" };
        catalogManager.AddException(".", new Exception());
        searchManager.CatalogManagers.Insert(1, catalogManager);
        InterfaceChain.Factory.SearchRegistryProvider.CatalogNames = searchManager.CatalogManagers.ConvertAll(c => c.NameInternal);
        InterfaceChain.Factory.SearchRegistryProvider.CatalogNames.Insert(2, "NotFound2");
        Collection<PSObject> results = InvokeScript("Get-SearchCatalog");
        Assert.False(InterfaceChain.HasWriteRecordings());
        Assert.All(results, (item, i) => Assert.Equal(expected[i], item.BaseObject, SearchCatalogInfoToMockComparer.Instance));
        Assert.Equal(expected.Count, results.Count);
        Assert.True(PowerShell.HadErrors);
        PSDataCollection<ErrorRecord> errors = PowerShell.Streams.Error;
        Assert.Collection(errors,
            e => Assert.Equal("BadCatalog1", e.TargetObject),
            e => Assert.Equal("NotFound2", e.TargetObject)
            );
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void GetSearchCatalog_HandlesFailures(ExceptionParam exceptionParam)
    {
        InterfaceChain.CatalogManager.AddException("^get_|^set_", exceptionParam.Exception);
        Collection<PSObject> results = InvokeScript("Get-SearchCatalog");
        Assert.False(InterfaceChain.HasWriteRecordings());
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }

    [Theory]
    [InlineData("-Catalog ")]
    [InlineData("-Catalog '' ")]
    [InlineData("$null ")]
    [InlineData("'' ")]
    public void GetSearchCatalog_ParameterValidation_Succeeds(string arguments)
    {
        AssertParameterValidation("Get-SearchCatalog " + arguments);
    }

    private class SetSearchCatalog_TheoryData : TheoryData<string, SearchCatalogInfo>
    {
        public SetSearchCatalog_TheoryData()
        {
            List<MockCatalogManager> catalogs = new MockSearchManager2().CatalogManagers;
            Add("-DiacriticSensitivity ",
                new SearchCatalogInfo(catalogs[0])
                {
                    DiacriticSensitivity = true
                });
            Add("-ConnectTimeout 111 -DataTimeout 333 -Catalog SecondCatalog ",
                new SearchCatalogInfo(catalogs[1])
                {
                    ConnectTimeout = 111,
                    DataTimeout = 333
                });
        }
    }

    [Theory]
    [ClassData(typeof(SetSearchCatalog_TheoryData))]
    public void SetSearchCatalog_Succeeds(string arguments, SearchCatalogInfo expectedInfo)
    {
        InterfaceChain.WithSearchManager(new MockSearchManager());
        MockCatalogManager catalogManager = Assert.Single(InterfaceChain.SearchManager.CatalogManagers, c => c.NameInternal == expectedInfo.Catalog);
        Collection<PSObject> results = InvokeScript("Set-SearchCatalog " + arguments);
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        Assert.Equal(expectedInfo, catalogManager, SearchCatalogInfoToMockComparer.Instance);
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void SetSearchCatalog_HandlesFailures(ExceptionParam exceptionParam)
    {
        InterfaceChain.CatalogManager.AddException("^get_|^set_", exceptionParam.Exception);
        Collection<PSObject> results = InvokeScript("Set-SearchCatalog -DiacriticSensitivity ");
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }


    [Theory]
    [InlineData("-Catalog ")]
    [InlineData("-Catalog '' ")]
    [InlineData("-ConnectTimeout -1 ")]
    [InlineData("-DataTimeout -1 ")]
    public void SetSearchCatalog_ParameterValidation_Succeeds(string arguments)
    {
        AssertParameterValidation("Set-SearchCatalog " + arguments);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("SecondCatalog", false)]
    [InlineData("ThirdCatalog", true)]
    public void ResetSearchCatalog_Succeeds(string? catalogName, bool positional)
    {
        string script = "Reset-SearchCatalog ";
        if (catalogName is null)
        {
            catalogName = SearchApiCommandBase.DefaultCatalogName;
        }
        else
        {
            if (!positional) script += "-Catalog ";
            script += $"'{catalogName}' ";
        }
        Collection<PSObject> results = InvokeScript(script);
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        Assert.Equal($"GetCatalog({catalogName})", Assert.Single(InterfaceChain.SearchManager.RecordedCalls));
        Assert.Equal("Reset()", Assert.Single(InterfaceChain.CatalogManager.RecordedCalls));
    }

    [Fact]
    public void ResetSearchCatalog_NoAdmin_Fails()
    {
        InterfaceChain.CatalogManager.AdminMode = false;
        Collection<PSObject> results = InvokeScript("Reset-SearchCatalog ");
        Assert.Empty(results);
        AssertUnauthorizedAccess();
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void ResetSearchCatalog_HandlesFailures(ExceptionParam exceptionParam)
    {
        InterfaceChain.CatalogManager.AddException("^Reset$", exceptionParam.Exception);
        Collection<PSObject> results = InvokeScript("Reset-SearchCatalog ");
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }

    [Theory]
    [InlineData("-Catalog ")]
    [InlineData("-Catalog '' ")]
    [InlineData("$null ")]
    [InlineData("'' ")]
    public void ResetSearchCatalog_ParameterValidation_Succeeds(string arguments)
    {
        AssertParameterValidation("Reset-SearchCatalog " + arguments);
    }
}
