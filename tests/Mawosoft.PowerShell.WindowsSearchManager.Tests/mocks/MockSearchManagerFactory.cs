// Copyright (c) Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

internal class MockSearchManagerFactory : MockInterfaceBase, ISearchManagerFactory
{
    internal MockSearchRegistryProvider SearchRegistryProvider { get; set; } = new MockSearchRegistryProvider();

    internal MockSearchManagerFactory() : this(new MockSearchManager2()) { }
    internal MockSearchManagerFactory(MockSearchManager searchManager) => ChildInterface = searchManager;

    public virtual ISearchManager CreateSearchManager()
    {
        RecordRead();
        return (GetChildInterface() as ISearchManager)!;
    }

    public virtual ISearchRegistryProvider CreateSearchRegistryProvider()
    {
        RecordRead();
        return SearchRegistryProvider;
    }
}
