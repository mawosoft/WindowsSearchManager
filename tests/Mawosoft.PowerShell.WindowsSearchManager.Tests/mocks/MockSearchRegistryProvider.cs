// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

// Internal members are used to setup mock behavior.
internal class MockSearchRegistryProvider : ISearchRegistryProvider
{
    internal List<string> CatalogNames { get; set; } = new() { "SystemIndex" };
    public virtual IReadOnlyList<string> GetCatalogNames() => CatalogNames;
}
