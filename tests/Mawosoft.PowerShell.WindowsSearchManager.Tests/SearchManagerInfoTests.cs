// Copyright (c) Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchManagerInfoTests
{
    [Fact]
    public void Ctor_Defaults()
    {
        SearchManagerInfo info = new();
        Assert.Equal(9, info.GetType().GetProperties().Length);
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
    public void Ctor_NullArgument_Throws()
    {
        Assert.Throws<ArgumentNullException>("searchManager", () => new SearchManagerInfo(null!));
    }

    [Fact]
    public void Ctor_ISearchManager_Succeeds()
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
        Assert.Equal(mock, info, SearchManagerInfoToMockComparer.Instance);
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
        Assert.Equivalent(info, clone, strict: true);
    }
}
