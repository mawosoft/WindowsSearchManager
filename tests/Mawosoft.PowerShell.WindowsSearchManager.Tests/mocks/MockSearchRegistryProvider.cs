// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

// Internal members are used to setup mock behavior.
internal class MockSearchRegistryProvider : ISearchRegistryProvider
{
    [SuppressMessage("Naming", "CA1721:Property names should not match get methods", Justification = "Internal properties used for mock setup")]
    internal List<string> CatalogNames { get; set; } = new() { "SystemIndex" };
    public virtual IReadOnlyList<string> GetCatalogNames() => CatalogNames;
}
