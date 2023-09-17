// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Contains settings and status for a search catalog.
/// </summary>
public sealed class SearchCatalogInfo : ICloneable
{
    /// <summary>Gets or sets the catalog name.</summary>
    /// <value>The catalog name.</value>
    public string? Catalog { get; set; }

    /// <summary>Gets or sets the connection time-out value for connecting to a store.</summary>
    /// <value>The connection time-out value in seconds.</value>
    public uint ConnectTimeout { get; set; }

    /// <summary>Gets or sets the data time-out value for transactions between the indexer and the search filter host.</summary>
    /// <value>The data time-out value in seconds.</value>
    public uint DataTimeout { get; set; }

    /// <summary>Gets or sets whether the catalog is sensitive to diacritics.</summary>
    /// <value><c>true</c> if the catalog is sensitive to diacritics, <c>false</c> otherwise.</value>
    public bool DiacriticSensitivity { get; set; }

    /// <summary>Gets a value indicating the current status of the catalog.</summary>
    /// <value>One of the enumeration values that indicates the catalog status.</value>
    public _CatalogStatus Status { get; }

    /// <summary>Gets a value indicating why the catalog is currently paused.</summary>
    /// <value>One of the enumeration values that indicates why the catalog is paused.</value>
    public _CatalogPausedReason PausedReason { get; }

    /// <summary>Gets the number of items in the catalog.</summary>
    /// <value>The number of items in the catalog.</value>
    public int ItemCount { get; }

    /// <summary>Gets the number of items to be indexed during the next incremental crawl.</summary>
    /// <value>The number of items to be indexed during the next incremental crawl.</value>
    public int ItemsToIndexCount { get; }

    /// <summary>Gets the number of items in the notification queue.</summary>
    /// <value>The number of items in the notification queue.</value>
    public int NotificationQueueCount { get; }

    /// <summary>Gets the number of items in the high-priority queue.</summary>
    /// <value>The number of items in the high-priority queue.</value>
    public int HighPriorityQueueCount { get; }

    /// <summary>Gets the path currently being indexed.</summary>
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
