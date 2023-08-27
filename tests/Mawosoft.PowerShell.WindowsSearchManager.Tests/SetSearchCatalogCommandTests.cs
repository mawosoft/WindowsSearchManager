// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SetSearchCatalogCommandTests : CommandTestBase
{
    private class Succeeds_TheoryData : TheoryData<string, SearchCatalogInfo>
    {
        public Succeeds_TheoryData()
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
    [ClassData(typeof(Succeeds_TheoryData))]
    public void Command_Succeeds(string arguments, SearchCatalogInfo expectedInfo)
    {
        InterfaceChain.WithSearchManager(new MockSearchManager());
        MockCatalogManager catalogManager = Assert.Single(InterfaceChain.SearchManager.CatalogManagers, c => c.NameInternal == expectedInfo.Catalog);
        Collection<PSObject> results = InvokeScript("Set-SearchCatalog " + arguments);
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        Assert.True(InterfaceChain.SingleHasWriteRecordings(catalogManager));
        Assert.Equal(expectedInfo, catalogManager, SearchCatalogInfoToMockComparer.Instance);
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void Command_HandlesFailures(ExceptionParam exceptionParam)
    {
        InterfaceChain.CatalogManager.AddException("^get_|^set_", exceptionParam.Exception);
        Collection<PSObject> results = InvokeScript("Set-SearchCatalog -DiacriticSensitivity ");
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }
}
