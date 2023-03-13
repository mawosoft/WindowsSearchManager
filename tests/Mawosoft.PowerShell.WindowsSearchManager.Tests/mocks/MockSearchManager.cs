// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

// NotSupportedException: the original COM class does not support this member.
// NotImplementedException: the Mock is missing a implementation for this member.
//
// Internal members are used to setup mock behavior.

public class MockSearchManager : ISearchManager
{
    internal ISearchCatalogManager? CatalogManager { get; set; }
    internal Exception? CatalogManagerException { get; set; }
    // If true will throw if member access requires admin rights.
    internal bool NoAdmin { get; set; }

    internal MockSearchManager() : this(new MockCatalogManager()) { }
    internal MockSearchManager(ISearchCatalogManager? catalogManager) => CatalogManager = catalogManager;

    // Simple properties representing data accessed via the public interface.

    internal string IndexerVersionStr { get; set; } = "10.0.1.2";
    internal (uint Major, uint Minor) Version { get; set; } = (10, 0);
    internal string UserAgentInternal { get; set; } = "Mozilla";
    internal _PROXY_ACCESS UseProxyInternal { get; set; } = _PROXY_ACCESS.PROXY_ACCESS_PRECONFIG;
    internal int LocalByPassInternal { get; set; } = 0;
    internal uint PortNumberInternal { get; set; } = 0;
    internal string ProxyNameInternal { get; set; } = string.Empty;
    internal string ByPassListInternal { get; set; } = string.Empty;

    // ISearchManager

    public virtual void GetIndexerVersionStr(out string ppszVersionString) => ppszVersionString = IndexerVersionStr;
    public virtual void GetIndexerVersion(out uint pdwMajor, out uint pdwMinor) => (pdwMajor, pdwMinor) = Version;
    public virtual void SetProxy(_PROXY_ACCESS sUseProxy, int fLocalByPassProxy, uint dwPortNumber, string pszProxyName, string pszByPassList)
    {
        if (NoAdmin) throw new UnauthorizedAccessException();
        UseProxyInternal = sUseProxy;
        LocalByPassInternal = fLocalByPassProxy;
        PortNumberInternal = dwPortNumber;
        ProxyNameInternal = pszProxyName;
        ByPassListInternal = pszByPassList;
    }

    public virtual ISearchCatalogManager GetCatalog(string pszCatalog)
        => CatalogManagerException == null ? CatalogManager! : throw CatalogManagerException;

    public virtual string ProxyName => !NoAdmin ? ProxyNameInternal : throw new UnauthorizedAccessException();

    public virtual string BypassList => !NoAdmin ? ByPassListInternal : throw new UnauthorizedAccessException();

    public virtual string UserAgent
    {
        get => !NoAdmin ? UserAgentInternal : throw new UnauthorizedAccessException();
        set => UserAgentInternal = !NoAdmin ? value : throw new UnauthorizedAccessException();
    }

    public virtual _PROXY_ACCESS UseProxy => !NoAdmin ? UseProxyInternal : throw new UnauthorizedAccessException();

    public virtual int LocalBypass => !NoAdmin ? LocalByPassInternal : throw new UnauthorizedAccessException();

    public virtual uint PortNumber => !NoAdmin ? PortNumberInternal : throw new UnauthorizedAccessException();

    public virtual IntPtr GetParameter(string pszName) => throw new NotSupportedException();
    public virtual void SetParameter(string pszName, ref tag_inner_PROPVARIANT pValue) => throw new NotSupportedException();
}

internal class MockSearchManager2 : MockSearchManager, ISearchManager2
{
    internal MockSearchManager2() : base() { }
    internal MockSearchManager2(ISearchCatalogManager? catalogManager) : base(catalogManager) { }

    // ISearchManager2

    public virtual void CreateCatalog(string pszCatalog, out ISearchCatalogManager ppCatalogManager) => throw new NotImplementedException();
    public virtual void DeleteCatalog(string pszCatalog) => throw new NotImplementedException();
}
