// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchManagerCommandsTests : CommandTestBase
{
    private class MockSearchManagerWithGetSetException : MockSearchManager2
    {
        internal Exception? GetException { get; set; }
        internal Exception? SetException { get; set; }
        internal MockSearchManagerWithGetSetException(Exception? getException, Exception? setException) : base()
        {
            GetException = getException;
            SetException = setException;
        }

        public override void SetProxy(_PROXY_ACCESS sUseProxy, int fLocalByPassProxy, uint dwPortNumber, string pszProxyName, string pszByPassList)
        {
            if (SetException == null) base.SetProxy(sUseProxy, fLocalByPassProxy, dwPortNumber, pszProxyName, pszByPassList);
            else throw SetException;
        }
        public override string ProxyName => GetException == null ? base.ProxyName : throw GetException;
        public override string UserAgent
        {
            get => GetException == null ? base.UserAgent : throw GetException;
            set
            {
                if (SetException == null) base.UserAgent = value;
                else throw SetException;
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
        Assert.True(PowerShell.HadErrors);
        ErrorRecord errorRecord = Assert.Single(PowerShell.Streams.Error);
        Assert.IsType<UnauthorizedAccessException>(errorRecord.Exception);
        Assert.Equal(ErrorCategory.PermissionDenied, errorRecord.CategoryInfo.Category);
        Assert.NotEqual(errorRecord.Exception.Message, errorRecord.ErrorDetails.Message);
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void GetSearchManager_HandlesFailures(ExceptionParam exceptionParam, bool shouldHaveCustomDetails)
    {
        Exception exception = exceptionParam.Value;
        InterfaceChain.WithSearchManager(new MockSearchManagerWithGetSetException(exception, exception));
        PowerShell.AddScript("Get-SearchManager");
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.Empty(results);
        AssertSingleErrorRecord(exception, shouldHaveCustomDetails);
    }

    private class SetSearchManager_TheoryData : TheoryData<string, SearchManagerInfo>
    {
        public SetSearchManager_TheoryData()
        {
            Add("Set-SearchManager -UserAgent foo-agent ",
                new SearchManagerInfo(new MockSearchManager2())
                {
                    UserAgent = "foo-agent"
                });
            Add("Set-SearchManager -ProxyAccess PROXY_ACCESS_DIRECT ",
                new SearchManagerInfo(new MockSearchManager2())
                {
                    ProxyAccess = _PROXY_ACCESS.PROXY_ACCESS_DIRECT
                });
            Add("Set-SearchManager -UserAgent foo-agent -ProxyAccess PROXY_ACCESS_DIRECT ",
                new SearchManagerInfo(new MockSearchManager2())
                {
                    UserAgent = "foo-agent",
                    ProxyAccess = _PROXY_ACCESS.PROXY_ACCESS_DIRECT
                });
            Add("Set-SearchManager -ProxyAccess PROXY_ACCESS_PROXY -ProxyName bar.com -ProxyPortNumber 0x8080 -ProxyBypassLocal -ProxyBypassList buzz.com,baz.org",
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
    public void SetSearchManager_Succeeds(string script, SearchManagerInfo expectedInfo)
    {
        PowerShell.AddScript(script);
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
        Assert.True(PowerShell.HadErrors);
        ErrorRecord errorRecord = Assert.Single(PowerShell.Streams.Error);
        Assert.IsType<UnauthorizedAccessException>(errorRecord.Exception);
        Assert.Equal(ErrorCategory.PermissionDenied, errorRecord.CategoryInfo.Category);
        Assert.NotEqual(errorRecord.Exception.Message, errorRecord.ErrorDetails.Message);
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void SetSearchManager_HandlesGetFailures(ExceptionParam exceptionParam, bool shouldHaveCustomDetails)
    {
        Exception exception = exceptionParam.Value;
        InterfaceChain.WithSearchManager(new MockSearchManagerWithGetSetException(exception, null));
        PowerShell.AddScript("Set-SearchManager -ProxyAccess PROXY_ACCESS_DIRECT ");
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.Empty(results);
        AssertSingleErrorRecord(exception, shouldHaveCustomDetails);
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void SetSearchManager_HandlessetFailures(ExceptionParam exceptionParam, bool shouldHaveCustomDetails)
    {
        Exception exception = exceptionParam.Value;
        InterfaceChain.WithSearchManager(new MockSearchManagerWithGetSetException(null, exception));
        PowerShell.AddScript("Set-SearchManager -ProxyAccess PROXY_ACCESS_DIRECT ");
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.Empty(results);
        AssertSingleErrorRecord(exception, shouldHaveCustomDetails);
    }

    [Fact]
    public void SetSearchManager_ConfirmImpact_Medium()
    {
        AssertShouldProcess(typeof(SetSearchManagerCommand), ConfirmImpact.Medium);
    }

    [Theory]
    [InlineData("Set-SearchManager -UserAgent $null ")]
    // TODO add more
    public void SetSearchManager_ParameterValidation_Succeeds(string script)
    {
        AssertParameterValidation(script);
    }

    [Theory]
    [InlineData("Set-SearchManager -UserAgent foo-agent ")]
    [InlineData("Set-SearchManager -ProxyAccess PROXY_ACCESS_DIRECT ")]
    [InlineData("Set-SearchManager -UserAgent foo-agent -ProxyAccess PROXY_ACCESS_DIRECT ")]
    [InlineData("Set-SearchManager -ProxyAccess PROXY_ACCESS_PROXY -ProxyName bar.com -ProxyPortNumber 0x8080 -ProxyBypassLocal -ProxyBypassList buzz.com,baz.org")]
    public void SetSearchManager_WhatIf_Succeeds(string script)
    {
        SearchManagerInfo expectedInfo = new(InterfaceChain.SearchManager);
        PowerShell.AddScript(script + " -WhatIf");
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        Assert.Equal(expectedInfo, InterfaceChain.SearchManager, SearchManagerInfoToMockComparer.Instance);
    }
}
