// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchRegistryProviderTests
{
    // On CI runners, WSearch is disabled by default, has never run, and therefore has no catalogs.

    [SkippableFact(SkipCondition.IsNetFramework, SkipCondition.IsCIandWSearchDisabled)]
    public void GetCatalogNames_PSCore_Succeeds()
    {
        ISearchRegistryProvider srp = new SearchRegistryProviderPSCore();
        IReadOnlyList<string> names = srp.GetCatalogNames();
        Assert.NotEmpty(names);
        Assert.Contains(SearchApiCommandBase.DefaultCatalogName, names);
    }

    [SkippableFact(SkipCondition.IsNotNetFramework, SkipCondition.IsCIandWSearchDisabled)]
    public void GetCatalogNames_PSDesktop_Succeeds()
    {
        ISearchRegistryProvider srp = new SearchRegistryProviderPSDesktop();
        IReadOnlyList<string> names = srp.GetCatalogNames();
        Assert.NotEmpty(names);
        Assert.Contains(SearchApiCommandBase.DefaultCatalogName, names);
    }
}
