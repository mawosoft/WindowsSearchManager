// Copyright (c) Matthias Wolf, Mawosoft.

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
        RecordRead();
        return (GetChildInterface() as ISearchCrawlScopeManager)!;
    }

    public virtual string Name
    {
        get
        {
            RecordRead();
            return NameInternal;
        }
    }

    public virtual uint ConnectTimeout
    {
        get
        {
            RecordRead();
            return ConnectTimeoutInternal;
        }
        set
        {
            RecordWrite(value);
            ConnectTimeoutInternal = value;
        }
    }

    public virtual uint DataTimeout
    {
        get
        {
            RecordRead();
            return DataTimeoutInternal;
        }
        set
        {
            RecordWrite(value);
            DataTimeoutInternal = value;
        }
    }

    public virtual int DiacriticSensitivity
    {
        get
        {
            RecordRead();
            return DiacriticSensitivityInternal;
        }
        set
        {
            RecordWrite(value);
            DiacriticSensitivityInternal = value;
        }
    }

    public virtual void GetCatalogStatus(out _CatalogStatus pStatus, out _CatalogPausedReason pPausedReason)
    {
        RecordRead();
        pStatus = StatusInternal;
        pPausedReason = PausedReasonInternal;
    }

    public virtual void Reset()
    {
        RecordWrite();
        TailCall();
    }

    public virtual void Reindex()
    {
        RecordWrite();
        TailCall();
    }

    public virtual void ReindexMatchingURLs(string pszPattern)
    {
        RecordWrite(pszPattern);
        TailCall();
    }

    public virtual void ReindexSearchRoot(string pszRootURL)
    {
        RecordWrite(pszRootURL);
        TailCall();
    }

    public virtual int NumberOfItems()
    {
        RecordRead();
        return NumberOfItemsInternal;
    }

    public virtual void NumberOfItemsToIndex(out int plIncrementalCount,
        out int plNotificationQueue, out int plHighPriorityQueue)
    {
        RecordRead();
        (plIncrementalCount, plNotificationQueue, plHighPriorityQueue) = NumberOfItemsToIndexInternal;
    }

    public virtual string URLBeingIndexed()
    {
        RecordRead();
        return URLBeingIndexedInternal;
    }

    // Unused ISearchCatalogManager members.

    [ExcludeFromCodeCoverage] public virtual ISearchPersistentItemsChangedSink GetPersistentItemsChangedSink() => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public virtual void GetItemsChangedSink(ISearchNotifyInlineSite pISearchNotifyInlineSite, ref Guid riid, out IntPtr ppv, out Guid pGUIDCatalogResetSignature, out Guid pGUIDCheckPointSignature, out uint pdwLastCheckPointNumber) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public virtual ISearchQueryHelper GetQueryHelper() => throw new NotImplementedException();

    // ISearchCatalogManager members not supported by the original COM class.

    [ExcludeFromCodeCoverage] public virtual IntPtr GetParameter(string pszName) => throw new NotSupportedException();
    [ExcludeFromCodeCoverage] public virtual void SetParameter(string pszName, ref tag_inner_PROPVARIANT pValue) => throw new NotSupportedException();
    [ExcludeFromCodeCoverage] public virtual uint GetURLIndexingState(string pszUrl) => throw new NotSupportedException();
    [ExcludeFromCodeCoverage] public virtual void RegisterViewForNotification(string pszView, ISearchViewChangedSink pViewChangedSink, out uint pdwCookie) => throw new NotSupportedException();
    [ExcludeFromCodeCoverage] public virtual void UnregisterViewForNotification(uint dwCookie) => throw new NotSupportedException();
    [ExcludeFromCodeCoverage] public virtual void SetExtensionClusion(string pszExtension, int fExclude) => throw new NotSupportedException();
    [ExcludeFromCodeCoverage] public virtual IEnumString EnumerateExcludedExtensions() => throw new NotSupportedException();
}
