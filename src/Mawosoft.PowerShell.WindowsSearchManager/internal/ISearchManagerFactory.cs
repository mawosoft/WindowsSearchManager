// Copyright (c) Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Factory abstraction for creating instances of <see cref="ISearchManager"/> and
/// <see cref="ISearchRegistryProvider"/>.
/// </summary>
internal interface ISearchManagerFactory
{
    ISearchManager CreateSearchManager();
    ISearchRegistryProvider CreateSearchRegistryProvider();
}
