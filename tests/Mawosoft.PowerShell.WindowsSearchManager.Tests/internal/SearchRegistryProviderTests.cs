// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchRegistryProviderTests
{
    // TODO not sure if reg entries exist on GitHub runners where WSearch is disabled by default

    [SkippableFact(SkipCondition.IsNetFramework)]
    public void GetCatalogNames_PSCore_Succeeds()
    {
        ISearchRegistryProvider srp = new SearchRegistryProviderPSCore();
        IReadOnlyList<string> names = srp.GetCatalogNames();
        Assert.NotEmpty(names);
        Assert.Contains(SearchApiCommandBase.DefaultCatalogName, names);
    }

    [SkippableFact(SkipCondition.IsNotNetFramework)]
    public void GetCatalogNames_PSDesktop_Succeeds()
    {
        ISearchRegistryProvider srp = new SearchRegistryProviderPSDesktop();
        IReadOnlyList<string> names = srp.GetCatalogNames();
        Assert.NotEmpty(names);
        Assert.Contains(SearchApiCommandBase.DefaultCatalogName, names);
    }
}
