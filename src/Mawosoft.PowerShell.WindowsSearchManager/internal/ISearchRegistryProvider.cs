// Copyright (c) Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Interface for accessing Windows Search registry entries.
/// </summary>
internal interface ISearchRegistryProvider
{
    /// <summary>
    /// Returns a list of registered search catalog names.
    /// </summary>
    IReadOnlyList<string> GetCatalogNames();
}
