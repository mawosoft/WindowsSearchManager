// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchManagerInfoTests
{
    [Fact]
    public void ctor_Defaults()
    {
        SearchManagerInfo info = new();
        Assert.Null(info.Version);
        Assert.Equal(0u, info.MajorVersion);
        Assert.Equal(0u, info.MinorVersion);
        Assert.Null(info.UserAgent);
        Assert.Equal(_PROXY_ACCESS.PROXY_ACCESS_PRECONFIG, info.ProxyAccess);
        Assert.Null(info.ProxyName);
        Assert.Equal(0u, info.ProxyPortNumber);
        Assert.False(info.ProxyBypassLocal);
        Assert.Null(info.ProxyBypassList);
    }

    [Fact]
    public void ctor_NullArguments_Throws()
    {
        Assert.Throws<ArgumentNullException>("searchManager", () => new SearchManagerInfo(null!));
    }

    [Fact]
    public void ctor_ISearchManager_Succeeds()
    {
        MockSearchManager mock = new()
        {
            UseProxyInternal = _PROXY_ACCESS.PROXY_ACCESS_PROXY,
            LocalByPassInternal = 1,
            PortNumberInternal = 111,
            ProxyNameInternal = "foo",
            ByPassListInternal = "bar,baz"
        };
        SearchManagerInfo info = new(mock);
        Assert.Equal(mock.IndexerVersionStr, info.Version);
        Assert.Equal(mock.Version.Major, info.MajorVersion);
        Assert.Equal(mock.Version.Minor, info.MinorVersion);
        Assert.Equal(mock.UserAgent, info.UserAgent);
        Assert.Equal(mock.UseProxyInternal, info.ProxyAccess);
        Assert.Equal(mock.ProxyNameInternal, info.ProxyName);
        Assert.Equal(mock.PortNumberInternal, info.ProxyPortNumber);
        Assert.Equal(mock.LocalByPassInternal != 0, info.ProxyBypassLocal);
        Assert.Equal(mock.ByPassListInternal, info.ProxyBypassList);
    }

    [Fact]
    public void Clone_Succeeds()
    {
        MockSearchManager mock = new()
        {
            UseProxyInternal = _PROXY_ACCESS.PROXY_ACCESS_PROXY,
            LocalByPassInternal = 1,
            PortNumberInternal = 111,
            ProxyNameInternal = "foo",
            ByPassListInternal = "bar,baz"
        };
        SearchManagerInfo info = new(mock);
        SearchManagerInfo clone = (SearchManagerInfo)info.Clone();
        Assert.Equal(info, clone, ShallowFieldComparer.Instance);
    }
}
