// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

#pragma warning disable CA1054 // URI-like parameters should not be strings
#pragma warning disable CA1055 // URI-like return values should not be strings
#pragma warning disable CA1056 // URI-like properties should not be strings

public class MockCatalogManager : MockInterfaceBase, ISearchCatalogManager
{
    internal MockCatalogManager() : this(new MockCrawlScopeManager()) { }
    internal MockCatalogManager(ISearchCrawlScopeManager scopeManager) : base()
    {
        ChildInterface = scopeManager;
        AdminMethodRegex = "^Reindex$|^Reset$|^URLBeingIndexed$";
    }

    // Simple properties representing data accessed via the public interface.

    internal string NameInternal { get; set; } = "SystemIndex";
    internal uint ConnectTimeoutInternal { get; set; }
    internal uint DataTimeoutInternal { get; set; }
    internal int DiacriticSensitivityInternal { get; set; }
    internal _CatalogStatus StatusInternal { get; set; } = _CatalogStatus.CATALOG_STATUS_PAUSED;
    internal _CatalogPausedReason PausedReasonInternal { get; set; } = _CatalogPausedReason.CATALOG_PAUSED_REASON_USER_ACTIVE;
    internal int NumberOfItemsInternal { get; set; } = 1000;
    internal (int Items, int Notifications, int HighPrio) NumberOfItemsToIndexInternal { get; set; } = (100, 300, 1);
    internal string URLBeingIndexedInternal { get; set; } = @"C:\foo\bar";

    // ISearchCatalogManager

    public virtual ISearchCrawlScopeManager GetCrawlScopeManager()
    {
        Record();
        return (GetChildInterface() as ISearchCrawlScopeManager)!;
    }

    public virtual string Name
    {
        get
        {
            Record();
            return NameInternal;
        }
    }

    public virtual uint ConnectTimeout
    {
        get
        {
            Record();
            return ConnectTimeoutInternal;
        }
        set
        {
            Record(value);
            ConnectTimeoutInternal = value;
        }
    }

    public virtual uint DataTimeout
    {
        get
        {
            Record();
            return DataTimeoutInternal;
        }
        set
        {
            Record(value);
            DataTimeoutInternal = value;
        }
    }

    public virtual int DiacriticSensitivity
    {
        get
        {
            Record();
            return DiacriticSensitivityInternal;
        }
        set
        {
            Record(value);
            DiacriticSensitivityInternal = value;
        }
    }

    public virtual void GetCatalogStatus(out _CatalogStatus pStatus, out _CatalogPausedReason pPausedReason)
    {
        Record();
        pStatus = StatusInternal;
        pPausedReason = PausedReasonInternal;
    }

    public virtual void Reset()
    {
        Record();
        TailCall();
    }

    public virtual void Reindex()
    {
        Record();
        TailCall();
    }

    public virtual void ReindexMatchingURLs(string pszPattern)
    {
        Record(pszPattern);
        TailCall();
    }

    public virtual void ReindexSearchRoot(string pszRootURL)
    {
        Record(pszRootURL);
        TailCall();
    }

    public virtual int NumberOfItems()
    {
        Record();
        return NumberOfItemsInternal;
    }

    public virtual void NumberOfItemsToIndex(out int plIncrementalCount,
        out int plNotificationQueue, out int plHighPriorityQueue)
    {
        Record();
        (plIncrementalCount, plNotificationQueue, plHighPriorityQueue) = NumberOfItemsToIndexInternal;
    }

    public virtual string URLBeingIndexed()
    {
        Record();
        return URLBeingIndexedInternal;
    }

    // Unused ISearchCatalogManager members.

    public virtual ISearchPersistentItemsChangedSink GetPersistentItemsChangedSink() => throw new NotImplementedException();
    public virtual void GetItemsChangedSink(ISearchNotifyInlineSite pISearchNotifyInlineSite, ref Guid riid, out IntPtr ppv, out Guid pGUIDCatalogResetSignature, out Guid pGUIDCheckPointSignature, out uint pdwLastCheckPointNumber) => throw new NotImplementedException();
    public virtual ISearchQueryHelper GetQueryHelper() => throw new NotImplementedException();

    // ISearchCatalogManager members not supported by the original COM class.

    public virtual IntPtr GetParameter(string pszName) => throw new NotSupportedException();
    public virtual void SetParameter(string pszName, ref tag_inner_PROPVARIANT pValue) => throw new NotSupportedException();
    public virtual uint GetURLIndexingState(string pszUrl) => throw new NotSupportedException();
    public virtual void RegisterViewForNotification(string pszView, ISearchViewChangedSink pViewChangedSink, out uint pdwCookie) => throw new NotSupportedException();
    public virtual void UnregisterViewForNotification(uint dwCookie) => throw new NotSupportedException();
    public virtual void SetExtensionClusion(string pszExtension, int fExclude) => throw new NotSupportedException();
    public virtual IEnumString EnumerateExcludedExtensions() => throw new NotSupportedException();
}
