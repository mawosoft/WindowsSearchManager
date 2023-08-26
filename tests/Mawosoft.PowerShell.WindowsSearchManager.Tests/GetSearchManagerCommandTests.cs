// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class GetSearchManagerCommandTests : CommandTestBase
{
    [Fact]
    public void GetSearchManager_Succeeds()
    {
        InterfaceChain.SearchManager.UseProxyInternal = _PROXY_ACCESS.PROXY_ACCESS_PROXY;
        InterfaceChain.SearchManager.LocalByPassInternal = 1;
        InterfaceChain.SearchManager.PortNumberInternal = 42;
        InterfaceChain.SearchManager.ProxyNameInternal = "fooproxy";
        InterfaceChain.SearchManager.ByPassListInternal = "barsite.com, buzzsite.com";
        Collection<PSObject> results = InvokeScript("Get-SearchManager");
        Assert.False(InterfaceChain.HasWriteRecordings());
        Assert.False(PowerShell.HadErrors);
        PSObject result = Assert.Single(results);
        SearchManagerInfo info = Assert.IsType<SearchManagerInfo>(result.BaseObject);
        Assert.Equal(InterfaceChain.SearchManager, info, SearchManagerInfoToMockComparer.Instance);
    }

    [Fact]
    public void GetSearchManager_NoAdmin_Fails()
    {
        InterfaceChain.SearchManager.AdminMode = false;
        Collection<PSObject> results = InvokeScript("Get-SearchManager");
        Assert.False(InterfaceChain.HasWriteRecordings());
        Assert.Empty(results);
        AssertUnauthorizedAccess();
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void GetSearchManager_HandlesFailures(ExceptionParam exceptionParam)
    {
        InterfaceChain.SearchManager.AddException("^get_|^set_|^SetProxy$", exceptionParam.Exception);
        Collection<PSObject> results = InvokeScript("Get-SearchManager");
        Assert.False(InterfaceChain.HasWriteRecordings());
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }
}
