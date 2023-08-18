// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class MockSearchManager : MockInterfaceBase, ISearchManager
{
    internal MockSearchManager() : this(new MockCatalogManager())
    {
        CatalogManagers = new()
            {
                new MockCatalogManager(),
                new MockCatalogManager()
                {
                    NameInternal = "SecondCatalog",
                    StatusInternal = _CatalogStatus.CATALOG_STATUS_INCREMENTAL_CRAWL,
                    PausedReasonInternal = _CatalogPausedReason.CATALOG_PAUSED_REASON_NONE
                },
                new MockCatalogManager()
                {
                    NameInternal = "ThirdCatalog",
                    NumberOfItemsInternal = 2222,
                    DiacriticSensitivityInternal = 1
                }
            };
    }

    internal MockSearchManager(ISearchCatalogManager catalogManager) : base()
    {
        ChildInterface = catalogManager;
        AdminMethodRegex = "^get_|^set_|^SetProxy$";
    }

    // In case of multiple catalogs.
    internal List<MockCatalogManager> CatalogManagers { get; set; } = new();

    // SearchAPI HRRESULT
    internal const int MSS_E_CATALOGNOTFOUND = unchecked((int)0x80042103);

    // Simple properties representing data accessed via the public interface.

    internal string IndexerVersionStrInternal { get; set; } = "10.0.1.2";
    internal (uint Major, uint Minor) VersionInternal { get; set; } = (10, 0);
    internal string UserAgentInternal { get; set; } = "Mozilla";
    internal _PROXY_ACCESS UseProxyInternal { get; set; } = _PROXY_ACCESS.PROXY_ACCESS_PRECONFIG;
    internal int LocalByPassInternal { get; set; }
    internal uint PortNumberInternal { get; set; }
    internal string ProxyNameInternal { get; set; } = string.Empty;
    internal string ByPassListInternal { get; set; } = string.Empty;

    // ISearchManager

    public virtual ISearchCatalogManager GetCatalog(string pszCatalog)
    {
        RecordRead(pszCatalog);
        object? catalog = GetChildInterface();
        if (catalog is null || CatalogManagers.Count == 0)
        {
            return (catalog as ISearchCatalogManager)!;
        }
        for (int i = 0; i < CatalogManagers.Count; i++)
        {
            if (pszCatalog == CatalogManagers[i].NameInternal) return CatalogManagers[i];
        }
        throw new COMException(null, MSS_E_CATALOGNOTFOUND);
    }

    public virtual string ProxyName
    {
        get
        {
            RecordRead();
            return ProxyNameInternal;
        }
    }

    public virtual string BypassList
    {
        get
        {
            RecordRead();
            return ByPassListInternal;
        }
    }

    public virtual string UserAgent
    {
        get
        {
            RecordRead();
            return UserAgentInternal;
        }
        set
        {
            RecordWrite(value);
            UserAgentInternal = value;
        }
    }

    public virtual _PROXY_ACCESS UseProxy
    {
        get
        {
            RecordRead();
            return UseProxyInternal;
        }
    }

    public virtual int LocalBypass
    {
        get
        {
            RecordRead();
            return LocalByPassInternal;
        }
    }

    public virtual uint PortNumber
    {
        get
        {
            RecordRead();
            return PortNumberInternal;
        }
    }

    public virtual void GetIndexerVersionStr(out string ppszVersionString)
    {
        RecordRead();
        ppszVersionString = IndexerVersionStrInternal;
    }

    public virtual void GetIndexerVersion(out uint pdwMajor, out uint pdwMinor)
    {
        RecordRead();
        (pdwMajor, pdwMinor) = VersionInternal;
    }

    public virtual void SetProxy(_PROXY_ACCESS sUseProxy, int fLocalByPassProxy, uint dwPortNumber, string pszProxyName, string pszByPassList)
    {
        RecordWrite(sUseProxy, fLocalByPassProxy, dwPortNumber, pszProxyName, pszByPassList);
        UseProxyInternal = sUseProxy;
        LocalByPassInternal = fLocalByPassProxy;
        PortNumberInternal = dwPortNumber;
        ProxyNameInternal = pszProxyName;
        ByPassListInternal = pszByPassList;
    }

    // ISearchManager members not supported by the original COM class.

    [ExcludeFromCodeCoverage] public virtual IntPtr GetParameter(string pszName) => throw new NotSupportedException();
    [ExcludeFromCodeCoverage] public virtual void SetParameter(string pszName, ref tag_inner_PROPVARIANT pValue) => throw new NotSupportedException();
}

internal class MockSearchManager2 : MockSearchManager, ISearchManager2
{
    internal MockSearchManager2() : base() { }
    internal MockSearchManager2(ISearchCatalogManager catalogManager) : base(catalogManager) { }

    // ISearchManager2

    public virtual void CreateCatalog(string pszCatalog, out ISearchCatalogManager ppCatalogManager)
    {
        RecordWrite(pszCatalog);
        ppCatalogManager = new MockCatalogManager()
        {
            NameInternal = pszCatalog
        };
    }

    public virtual void DeleteCatalog(string pszCatalog)
    {
        RecordWrite(pszCatalog);
        TailCall();
    }
}
