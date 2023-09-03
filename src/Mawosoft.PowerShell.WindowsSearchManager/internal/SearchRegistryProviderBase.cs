// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Base class for implementations of <see cref="ISearchRegistryProvider"/>.
/// </summary>
internal abstract class SearchRegistryProviderBase
{
    // Some registry keys for Windows Search.
    // HKLM\SOFTWARE\Microsoft\Windows Search\* is the main location, but there are a lot of other related places,
    // including, but not limited to:
    //   HKLM\SOFTWARE\Microsoft\Windows\Windows Search\Preferences
    //   HKCU\SOFTWARE\Microsoft\Windows Search\
    //   HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Search
    //   HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\SearchSettings
    //   HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Search
    //   HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\SearchPlatform\Preferences
    public const string WindowsSearch = @"SOFTWARE\Microsoft\Windows Search";
    public const string CatalogListWindowsCatalogs = WindowsSearch + @"\CatalogList\Applications\Windows\Catalogs";
}
