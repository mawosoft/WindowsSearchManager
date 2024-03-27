// Copyright (c) 2023-2024 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class MockCommand : SearchApiCommandBase
{
    internal MockCommand(ICommandRuntime commandRuntime) : base() => CommandRuntime = commandRuntime;

    // Internal TestXxx wrappers for protected methods
    internal static T TestEnsureNotNull<T>(T value) where T : class
        => EnsureNotNull(value);
    internal ISearchManager TestCreateSearchManager() => CreateSearchManager();
    internal ISearchManager2 TestGetSearchManager2() => GetSearchManager2();
    internal ISearchManager2 TestGetSearchManager2(ISearchManager searchManager)
        => GetSearchManager2(searchManager);
    internal ISearchCatalogManager TestGetSearchCatalogManager(string catalogName)
        => GetSearchCatalogManager(catalogName);
    internal ISearchCatalogManager TestGetSearchCatalogManager(ISearchManager searchManager, string catalogName)
        => GetSearchCatalogManager(searchManager, catalogName);
    internal ISearchCrawlScopeManager TestGetCrawlScopeManager(string catalogName)
        => GetCrawlScopeManager(catalogName);
    internal ISearchCrawlScopeManager TestGetCrawlScopeManager(ISearchCatalogManager catalogManager)
        => GetCrawlScopeManager(catalogManager);
    internal void TestSaveCrawlScopeManager(ISearchCrawlScopeManager? scopeManager)
        => SaveCrawlScopeManager(scopeManager);
}
