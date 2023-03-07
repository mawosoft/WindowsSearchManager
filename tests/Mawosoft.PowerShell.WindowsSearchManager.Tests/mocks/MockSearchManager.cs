// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

// NotSupportedException: the original COM class does not support this member.
// NotImplementedException: the Mock is missing a implementation for this member.
//
// Internal members are used to setup mock behavior.

internal class MockSearchManager : ISearchManager, ISearchManager2
{
    // Simple properties representing data accessed via the public interface.

    internal string IndexerVersionStr { get; set; } = "10.0.1.2";
    internal (uint Major, uint Minor) Version { get; set; } = (10, 0);
    internal _PROXY_ACCESS UseProxyInternal { get; set; } = _PROXY_ACCESS.PROXY_ACCESS_PRECONFIG;
    internal int LocalByPassInternal { get; set; } = 0;
    internal uint PortNumberInternal { get; set; } = 0;
    internal string ProxyNameInternal { get; set; } = string.Empty;
    internal string ByPassListInternal { get; set; } = string.Empty;

    // ISearchManager

    public virtual void GetIndexerVersionStr(out string ppszVersionString) => ppszVersionString = IndexerVersionStr;
    public virtual void GetIndexerVersion(out uint pdwMajor, out uint pdwMinor) => (pdwMajor, pdwMinor) = Version;
    public virtual IntPtr GetParameter(string pszName) => throw new NotSupportedException();
    public virtual void SetParameter(string pszName, ref tag_inner_PROPVARIANT pValue) => throw new NotSupportedException();
    public virtual void SetProxy(_PROXY_ACCESS sUseProxy, int fLocalByPassProxy, uint dwPortNumber, string pszProxyName, string pszByPassList)
    {
        UseProxyInternal = sUseProxy;
        LocalByPassInternal = fLocalByPassProxy;
        PortNumberInternal = dwPortNumber;
        ProxyNameInternal = pszProxyName;
        ByPassListInternal = pszByPassList;
    }

    public virtual ISearchCatalogManager GetCatalog(string pszCatalog) => throw new NotImplementedException();

    public virtual string ProxyName => ProxyNameInternal;

    public virtual string BypassList => ByPassListInternal;

    public virtual string UserAgent { get; set; } = "Mozilla";

    public virtual _PROXY_ACCESS UseProxy => UseProxyInternal;

    public virtual int LocalBypass => LocalByPassInternal;

    public virtual uint PortNumber => PortNumberInternal;

    // ISearchManager2

    public virtual void CreateCatalog(string pszCatalog, out ISearchCatalogManager ppCatalogManager) => throw new NotImplementedException();
    public virtual void DeleteCatalog(string pszCatalog) => throw new NotImplementedException();
}
