// Copyright (c) 2023 Matthias Wolf, Mawosoft.

using Microsoft.Win32;

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Implementation of <see cref="ISearchRegistryProvider"/> for PowerShell Core.
/// </summary>
internal class SearchRegistryProviderPSCore : SearchRegistryProviderBase, ISearchRegistryProvider
{
    /// <inheritdoc/>
    public IReadOnlyList<string> GetCatalogNames()
    {
        using (RegistryKey? subkey = Registry.LocalMachine.OpenSubKey(CatalogListWindowsCatalogs))
        {
            return subkey?.GetSubKeyNames() ?? Array.Empty<string>();
        }
    }
}
