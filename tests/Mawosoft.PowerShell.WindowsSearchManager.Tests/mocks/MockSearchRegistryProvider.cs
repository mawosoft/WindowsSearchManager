// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

internal class MockSearchRegistryProvider : ISearchRegistryProvider // TODO
{
    public virtual IReadOnlyList<string> GetCatalogNames() => throw new NotImplementedException();
}
