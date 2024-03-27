// Copyright (c) 2023-2024 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SetSearchManagerCommandTests : CommandTestBase
{
    private class Succeeds_TheoryData : TheoryData<string, SearchManagerInfo>
    {
        public Succeeds_TheoryData()
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
    [ClassData(typeof(Succeeds_TheoryData))]
    public void Command_Succeeds(string arguments, SearchManagerInfo expectedInfo)
    {
        Collection<PSObject> results = InvokeScript("Set-SearchManager " + arguments);
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        Assert.True(InterfaceChain.SingleHasWriteRecordings(InterfaceChain.SearchManager));
        Assert.Equal(expectedInfo, InterfaceChain.SearchManager, SearchManagerInfoToMockComparer.Instance);
    }

    [Fact]
    public void Command_NoAdmin_FailsWithCustomMessage()
    {
        InterfaceChain.SearchManager.AdminMode = false;
        Collection<PSObject> results = InvokeScript("Set-SearchManager -UserAgent foo ");
        Assert.Empty(results);
        AssertUnauthorizedAccess();
    }

    public static readonly object?[][] HandlesFailures_TestData =
        new string[] { "^get_", "^set_|^SetProxy$" }
        .CrossJoin(new Exception_TheoryData())
        .ToArray();

    [Theory]
    [MemberData(nameof(HandlesFailures_TestData))]
    public void Command_HandlesFailures(string exceptionRegex, ExceptionParam exceptionParam)
    {
        InterfaceChain.SearchManager.AddException(exceptionRegex, exceptionParam.Exception);
        Collection<PSObject> results = InvokeScript("Set-SearchManager -ProxyAccess PROXY_ACCESS_DIRECT ");
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }
}
