// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

// Internal members are used to setup mock behavior.
internal class MockSearchManagerFactory : ISearchManagerFactory
{
    internal ISearchManager? SearchManager { get; set; }
    internal Exception? SearchManagerException { get; set; }
    internal ISearchRegistryProvider SearchRegistryProvider { get; set; } = new MockSearchRegistryProvider();

    internal MockSearchManagerFactory() : this(new MockSearchManager2()) { }
    internal MockSearchManagerFactory(ISearchManager? searchManager) => SearchManager = searchManager;

    public virtual ISearchManager CreateSearchManager()
        => SearchManagerException == null ? SearchManager! : throw SearchManagerException;

    public virtual ISearchRegistryProvider CreateSearchRegistryProvider() => SearchRegistryProvider;
}
