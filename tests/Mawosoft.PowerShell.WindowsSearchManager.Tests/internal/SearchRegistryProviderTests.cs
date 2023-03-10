// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchRegistryProviderTests
{
    // On CI runners, WSearch is disabled by default, has never run, and therefore has no catalogs.
    [SkippableFact(SkipCondition.IsCIandWSearchDisabled)]
    public void GetCatalogNames_Succeeds()
    {
#if NETFRAMEWORK
        ISearchRegistryProvider srp = new SearchRegistryProviderPSDesktop();
#else
        ISearchRegistryProvider srp = new SearchRegistryProviderPSCore();
#endif
        IReadOnlyList<string> names = srp.GetCatalogNames();
        Assert.NotEmpty(names);
        Assert.Contains(SearchApiCommandBase.DefaultCatalogName, names);
    }
}
