// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

// Exceptions NotImplemented vs NotSupported
// - NotSupported: the original COM class does not support this member.
// - NotImplemented: the Mock is missing a implementation for this member
//
// Internal members are used to setup mock behavior.
// Everything is virtual to allow minimal changes per testcase.

internal class MockCatalogManager : ISearchCatalogManager
{
    // If true will throw if member access requires admin rights
    internal bool NoAdmin { get; set; }

    internal virtual _CatalogStatus Status { get; set; } = _CatalogStatus.CATALOG_STATUS_PAUSED;
    internal virtual _CatalogPausedReason PausedReason { get; set; } = _CatalogPausedReason.CATALOG_PAUSED_REASON_USER_ACTIVE;
    internal virtual int NumberOfItemsInternal { get; set; } = 1000;
    internal virtual (int Items, int Notifications, int HighPrio) NumberOfItemsToIndexInternal { get; set; } = (100, 300, 1);
    internal virtual string URLBeingIndexedInternal { get; set; } = @"C:\foo\bar";

    public virtual IntPtr GetParameter(string pszName) => throw new NotSupportedException();
    public virtual void SetParameter(string pszName, ref tag_inner_PROPVARIANT pValue) => throw new NotSupportedException();
    public virtual void GetCatalogStatus(out _CatalogStatus pStatus, out _CatalogPausedReason pPausedReason)
    {
        pStatus = Status;
        pPausedReason = PausedReason;
    }
    public virtual void Reset() => throw new NotImplementedException();
    public virtual void Reindex() => throw new NotImplementedException();
    public virtual void ReindexMatchingURLs(string pszPattern) => throw new NotImplementedException();
    public virtual void ReindexSearchRoot(string pszRootURL) => throw new NotImplementedException();
    public virtual int NumberOfItems() => NumberOfItemsInternal;
    public virtual void NumberOfItemsToIndex(out int plIncrementalCount,
        out int plNotificationQueue, out int plHighPriorityQueue)
        => (plIncrementalCount, plNotificationQueue, plHighPriorityQueue) = NumberOfItemsToIndexInternal;
    public virtual string URLBeingIndexed()
    {
        if (NoAdmin) throw new UnauthorizedAccessException();
        return URLBeingIndexedInternal;
    }

    public virtual uint GetURLIndexingState(string pszUrl) => throw new NotSupportedException();
    public virtual ISearchPersistentItemsChangedSink GetPersistentItemsChangedSink() => throw new NotImplementedException();
    public virtual void RegisterViewForNotification(string pszView, ISearchViewChangedSink pViewChangedSink, out uint pdwCookie) => throw new NotSupportedException();
    public virtual void GetItemsChangedSink(ISearchNotifyInlineSite pISearchNotifyInlineSite, ref Guid riid, out IntPtr ppv, out Guid pGUIDCatalogResetSignature, out Guid pGUIDCheckPointSignature, out uint pdwLastCheckPointNumber) => throw new NotImplementedException();
    public virtual void UnregisterViewForNotification(uint dwCookie) => throw new NotSupportedException();
    public virtual void SetExtensionClusion(string pszExtension, int fExclude) => throw new NotSupportedException();
    public virtual IEnumString EnumerateExcludedExtensions() => throw new NotSupportedException();
    public virtual ISearchQueryHelper GetQueryHelper() => throw new NotImplementedException();
    public virtual ISearchCrawlScopeManager GetCrawlScopeManager() => throw new NotImplementedException();

    public virtual string Name { get; internal set; } = "SystemIndex";
    public virtual uint ConnectTimeout { get ; set; }
    public virtual uint DataTimeout { get; set; }
    public virtual int DiacriticSensitivity { get; set; }
}
