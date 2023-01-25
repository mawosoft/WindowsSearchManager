// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// POCO for ISearchCatalogManager
/// Admin rights are only required to get the item currently indexed. This is considered of minor importance,
/// and insufficient rights will not throw.
/// </summary>
public sealed class SearchCatalogInfo : ICloneable
{
    public string? Catalog { get; set; }
    public uint ConnectTimeout { get; set; }
    public uint DataTimeout { get; set; }
    public bool DiacriticSensitivity { get; set; }
    public _CatalogStatus Status { get; private set; }
    public _CatalogPausedReason PausedReason { get; private set; }
    public int ItemCount { get; private set; }
    public int ItemsToIndexCount { get; private set; }
    public int NotificationQueueCount { get; private set; }
    public int HighPriorityQueueCount { get; private set; }
    public string? PathBeingIndexed { get; private set; }

    // TODO stringres
    private const string AdminRequired = "N/A: Requires admin rights.";

    public SearchCatalogInfo() { }

    internal SearchCatalogInfo(ISearchCatalogManager searchCatalog)
    {
        if (searchCatalog == null) throw new ArgumentNullException(nameof(searchCatalog));

        Catalog = searchCatalog.Name;
        ConnectTimeout = searchCatalog.ConnectTimeout;
        DataTimeout = searchCatalog.DataTimeout;
        DiacriticSensitivity = searchCatalog.DiacriticSensitivity != 0;
        searchCatalog.GetCatalogStatus(out _CatalogStatus status, out _CatalogPausedReason pausedReason);
        Status = status;
        PausedReason = pausedReason;
        ItemCount = searchCatalog.NumberOfItems();
        searchCatalog.NumberOfItemsToIndex(out int incremental, out int notificationQueue, out int highPriorityQueue);
        ItemsToIndexCount = incremental;
        NotificationQueueCount= notificationQueue;
        HighPriorityQueueCount= highPriorityQueue;
        try
        {
            PathBeingIndexed = searchCatalog.URLBeingIndexed();
        }
        catch (UnauthorizedAccessException)
        {
            PathBeingIndexed = AdminRequired;
        }
    }

    public object Clone() => MemberwiseClone();
}
