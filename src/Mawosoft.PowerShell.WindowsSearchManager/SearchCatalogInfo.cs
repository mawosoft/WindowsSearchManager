// Copyright (c) 2023-2024 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Contains settings and status for a search catalog.
/// </summary>
public sealed class SearchCatalogInfo : ICloneable
{
    /// <value>The catalog name.</value>
    public string? Catalog { get; set; }

    /// <value>The time, in seconds, that the indexer should wait for a connection response from a server or data store.</value>
    public uint ConnectTimeout { get; set; }

    /// <value>The time, in seconds, that the indexer should wait for a data transaction.</value>
    public uint DataTimeout { get; set; }

    /// <value><c>true</c> if the catalog should differentiate words with diacritics. <c>false</c> if the catalog should ignore diacritics.</value>
    public bool DiacriticSensitivity { get; set; }

    /// <value>One of the enumeration values that indicates the current catalog status.</value>
    public _CatalogStatus Status { get; }

    /// <value>One of the enumeration values that indicates why the catalog is paused.</value>
    public _CatalogPausedReason PausedReason { get; }

    /// <value>The number of items in the catalog.</value>
    public int ItemCount { get; }

    /// <value>The number of items to be indexed during the next incremental crawl.</value>
    public int ItemsToIndexCount { get; }

    /// <value>The number of items in the notification queue.</value>
    public int NotificationQueueCount { get; }

    /// <value>The number of items in the high-priority queue.</value>
    public int HighPriorityQueueCount { get; }

    /// <value>The path currently being indexed.</value>
    public string? PathBeingIndexed { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchCatalogInfo"/> class.
    /// </summary>
    public SearchCatalogInfo() { }

    internal SearchCatalogInfo(ISearchCatalogManager searchCatalog)
    {
        if (searchCatalog is null) throw new ArgumentNullException(nameof(searchCatalog));

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
        NotificationQueueCount = notificationQueue;
        HighPriorityQueueCount = highPriorityQueue;
        try
        {
            PathBeingIndexed = searchCatalog.URLBeingIndexed();
        }
        catch (UnauthorizedAccessException)
        {
            PathBeingIndexed = SR.NA_AdminRequired;
        }
    }

    /// <summary>
    /// Creates a shallow copy of the <see cref="SearchCatalogInfo"/> instance.
    /// </summary>
    /// <returns>A shallow copy of the <see cref="SearchCatalogInfo"/> instance.</returns>
    public object Clone() => MemberwiseClone();
}
