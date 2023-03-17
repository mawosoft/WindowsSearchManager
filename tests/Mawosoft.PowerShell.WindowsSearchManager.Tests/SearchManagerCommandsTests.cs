// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchManagerCommandsTests : CommandTestBase
{
    private class MockSearchManagerWithGetSetException : MockSearchManager2
    {
        internal Exception? GetException { get; set; }
        internal Exception? SetException { get; set; }

        public override void SetProxy(_PROXY_ACCESS sUseProxy, int fLocalByPassProxy, uint dwPortNumber, string pszProxyName, string pszByPassList)
        {
            if (SetException != null) throw SetException;
            base.SetProxy(sUseProxy, fLocalByPassProxy, dwPortNumber, pszProxyName, pszByPassList);
        }
        public override string ProxyName => GetException == null ? base.ProxyName : throw GetException;
        public override string UserAgent
        {
            get => GetException == null ? base.UserAgent : throw GetException;
            set
            {
                if (SetException != null) throw SetException;
                base.UserAgent = value;
            }
        }
    }

    [Fact]
    public void GetSearchManager_Succeeds()
    {
        InterfaceChain.SearchManager.UseProxyInternal = _PROXY_ACCESS.PROXY_ACCESS_PROXY;
        InterfaceChain.SearchManager.LocalByPassInternal = 1;
        InterfaceChain.SearchManager.PortNumberInternal = 42;
        InterfaceChain.SearchManager.ProxyNameInternal = "fooproxy";
        InterfaceChain.SearchManager.ByPassListInternal = "barsite.com, buzzsite.com";
        PowerShell.AddScript("Get-SearchManager");
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.False(PowerShell.HadErrors);
        PSObject result = Assert.Single(results);
        SearchManagerInfo info = Assert.IsType<SearchManagerInfo>(result.BaseObject);
        Assert.Equal(InterfaceChain.SearchManager, info, SearchManagerInfoToMockComparer.Instance);
    }

    [Fact]
    public void GetSearchManager_NoAdmin_Fails()
    {
        InterfaceChain.SearchManager.NoAdmin = true;
        PowerShell.AddScript("Get-SearchManager");
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.Empty(results);
        AssertUnauthorizedAccess();
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void GetSearchManager_HandlesFailures(ExceptionParam exceptionParam)
    {
        InterfaceChain.WithSearchManager(new MockSearchManagerWithGetSetException()
        {
            GetException = exceptionParam.Value.Exception,
            SetException = exceptionParam.Value.Exception
        });
        PowerShell.AddScript("Get-SearchManager");
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }

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
        PowerShell.AddScript("Set-SearchManager " + arguments);
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        Assert.Equal(expectedInfo, InterfaceChain.SearchManager, SearchManagerInfoToMockComparer.Instance);
    }

    [Fact]
    public void SetSearchManager_NoAdmin_Fails()
    {
        InterfaceChain.SearchManager.NoAdmin = true;
        PowerShell.AddScript("Set-SearchManager -UserAgent foo ");
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.Empty(results);
        AssertUnauthorizedAccess();
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void SetSearchManager_HandlesGetFailures(ExceptionParam exceptionParam)
    {
        InterfaceChain.WithSearchManager(new MockSearchManagerWithGetSetException()
        {
            GetException = exceptionParam.Value.Exception
        });
        PowerShell.AddScript("Set-SearchManager -ProxyAccess PROXY_ACCESS_DIRECT ");
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void SetSearchManager_HandlesSetFailures(ExceptionParam exceptionParam)
    {
        InterfaceChain.WithSearchManager(new MockSearchManagerWithGetSetException()
        {
            SetException = exceptionParam.Value.Exception
        });
        PowerShell.AddScript("Set-SearchManager -ProxyAccess PROXY_ACCESS_DIRECT ");
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }

    [Fact]
    public void SetSearchManager_ConfirmImpact_Medium()
    {
        AssertConfirmImpact(typeof(SetSearchManagerCommand), ConfirmImpact.Medium);
    }

    [Theory]
    [InlineData("-UserAgent ")]
    [InlineData("-UserAgent '' ")]
    [InlineData("-ProxyAccess 3 ")]
    [InlineData("-ProxyName ")]
    [InlineData("-ProxyPortNumber -1 ")]
    [InlineData("-ProxyPortNumber 65536 ")]
    [InlineData("-ProxyBypassList ")]
    public void SetSearchManager_ParameterValidation_Succeeds(string arguments)
    {
        AssertParameterValidation("Set-SearchManager " + arguments);
    }

    [Theory]
    [InlineData("-UserAgent foo-agent ")]
    [InlineData("-ProxyAccess PROXY_ACCESS_DIRECT ")]
    [InlineData("-UserAgent foo-agent -ProxyAccess PROXY_ACCESS_DIRECT ")]
    [InlineData("-ProxyAccess PROXY_ACCESS_PROXY -ProxyName bar.com -ProxyPortNumber 0x8080 -ProxyBypassLocal -ProxyBypassList buzz.com,baz.org")]
    public void SetSearchManager_WhatIf_Succeeds(string arguments)
    {
        SearchManagerInfo expectedInfo = new(InterfaceChain.SearchManager);
        PowerShell.AddScript("Set-SearchManager " + arguments + " -WhatIf ");
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        Assert.Equal(expectedInfo, InterfaceChain.SearchManager, SearchManagerInfoToMockComparer.Instance);
    }
}
