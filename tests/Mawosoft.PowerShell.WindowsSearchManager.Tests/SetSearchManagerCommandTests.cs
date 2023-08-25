// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SetSearchManagerCommandTests : CommandTestBase
{
    private class SetSearchManager_TheoryData : TheoryData<string, SearchManagerInfo>
    {
        public SetSearchManager_TheoryData()
        {
            Add("-UserAgent foo-agent ",
                new SearchManagerInfo(new MockSearchManager2())
                {
                    UserAgent = "foo-agent"
                });
            Add("-ProxyAccess PROXY_ACCESS_DIRECT ",
                new SearchManagerInfo(new MockSearchManager2())
                {
                    ProxyAccess = _PROXY_ACCESS.PROXY_ACCESS_DIRECT
                });
            Add("-UserAgent foo-agent -ProxyAccess PROXY_ACCESS_DIRECT ",
                new SearchManagerInfo(new MockSearchManager2())
                {
                    UserAgent = "foo-agent",
                    ProxyAccess = _PROXY_ACCESS.PROXY_ACCESS_DIRECT
                });
            Add("-ProxyAccess PROXY_ACCESS_PROXY -ProxyName bar.com -ProxyPortNumber 0x8080 -ProxyBypassLocal -ProxyBypassList buzz.com,baz.org",
                new SearchManagerInfo(new MockSearchManager2())
                {
                    ProxyAccess = _PROXY_ACCESS.PROXY_ACCESS_PROXY,
                    ProxyName = "bar.com",
                    ProxyPortNumber = 0x8080,
                    ProxyBypassLocal = true,
                    ProxyBypassList = "buzz.com,baz.org"
                });
        }
    }

    [Theory]
    [ClassData(typeof(SetSearchManager_TheoryData))]
    public void SetSearchManager_Succeeds(string arguments, SearchManagerInfo expectedInfo)
    {
        Collection<PSObject> results = InvokeScript("Set-SearchManager " + arguments);
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        Assert.Equal(expectedInfo, InterfaceChain.SearchManager, SearchManagerInfoToMockComparer.Instance);
    }

    [Fact]
    public void SetSearchManager_NoAdmin_Fails()
    {
        InterfaceChain.SearchManager.AdminMode = false;
        Collection<PSObject> results = InvokeScript("Set-SearchManager -UserAgent foo ");
        Assert.Empty(results);
        AssertUnauthorizedAccess();
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void SetSearchManager_HandlesGetFailures(ExceptionParam exceptionParam)
    {
        InterfaceChain.SearchManager.AddException("^get_", exceptionParam.Exception);
        Collection<PSObject> results = InvokeScript("Set-SearchManager -ProxyAccess PROXY_ACCESS_DIRECT ");
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void SetSearchManager_HandlesSetFailures(ExceptionParam exceptionParam)
    {
        InterfaceChain.SearchManager.AddException("^set_|^SetProxy$", exceptionParam.Exception);
        Collection<PSObject> results = InvokeScript("Set-SearchManager -ProxyAccess PROXY_ACCESS_DIRECT ");
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }
}
