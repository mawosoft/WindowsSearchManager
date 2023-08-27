// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class GetSearchCatalogCommandTests : CommandTestBase
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("SecondCatalog", false)]
    [InlineData("ThirdCatalog", true)]
    public void Command_Succeeds(string? catalogName, bool positional)
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
        Assert.False(PowerShell.HadErrors);
        Assert.False(InterfaceChain.HasWriteRecordings());
        Assert.All(results, (item, i) => Assert.Equal(expected[i], item.BaseObject, SearchCatalogInfoToMockComparer.Instance));
        Assert.Equal(expected.Count, results.Count);
    }

    [Fact]
    public void Command_WithFailures_PartiallySucceeds()
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
        Assert.True(PowerShell.HadErrors);
        Assert.False(InterfaceChain.HasWriteRecordings());
        Assert.All(results, (item, i) => Assert.Equal(expected[i], item.BaseObject, SearchCatalogInfoToMockComparer.Instance));
        Assert.Equal(expected.Count, results.Count);
        PSDataCollection<ErrorRecord> errors = PowerShell.Streams.Error;
        Assert.Collection(errors,
            e => Assert.Equal("BadCatalog1", e.TargetObject),
            e => Assert.Equal("NotFound2", e.TargetObject)
            );
    }
}
