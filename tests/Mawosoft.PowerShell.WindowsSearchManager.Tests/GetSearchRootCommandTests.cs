// Copyright (c) 2023-2024 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class GetSearchRootCommandTests : CommandTestBase
{
    private static readonly List<MockSearchRoot> s_roots =
    [
        new()
        {
            RootURL = @"fooprotocol://{bar-sid}/",
            ProvidesNotifications = 0,
            UseNotificationsOnly = 1,
            EnumerationDepth = 333,
            HostDepth = 222,
            AuthenticationType = _AUTH_TYPE.eAUTH_TYPE_BASIC
        },
        new()
        {
            RootURL = @"file:///c:\",
            IsHierarchical = 0,
            UseNotificationsOnly = 1,
            EnumerationDepth = 333,
            HostDepth = 222,
        },
        new()
        {
            RootURL = @"bar://foo/",
            FollowDirectories = 0,
            AuthenticationType = _AUTH_TYPE.eAUTH_TYPE_NTLM
        },
        new()
        {
            RootURL = @"x:\",
        },
        new()
        {
            RootURL = @"y:\",
        }
    ];

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(3, false)]
    [InlineData(3, true)]
    public void Command_Succeeds(int rootCount, bool pathOnly)
    {
        InterfaceChain.ScopeManager.Roots = new(s_roots.Take(rootCount));
        string script = $"Get-SearchRoot {(pathOnly ? "-PathOnly" : "")} ";
        Collection<PSObject> results = InvokeScript(script);
        Assert.False(PowerShell.HadErrors);
        Assert.False(InterfaceChain.HasWriteRecordings());
        if (pathOnly)
        {
            var expected = s_roots.Take(rootCount).Select(r => r.RootURL);
            var actual = results.Select(o => (string)o.BaseObject);
            Assert.Equal(expected, actual);
        }
        else
        {
            var expected = s_roots.Take(rootCount);
            var actual = results.Select(o => (SearchRootInfo)o.BaseObject);
            Assert.Equal(expected, actual, SearchRootInfoToMockComparer.Instance);
        }
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void Command_WithFailures_PartiallySucceeds(ExceptionParam exceptionParam)
    {
        InterfaceChain.ScopeManager.Roots = new(s_roots);
        int expectedCount = 2;
        Assert.InRange(expectedCount, expectedCount, s_roots.Count - 2);
        InterfaceChain.ScopeManager.Roots[expectedCount] = exceptionParam.Exception;
        Collection<PSObject> results = InvokeScript("Get-SearchRoot -PathOnly");
        Assert.True(PowerShell.HadErrors);
        Assert.False(InterfaceChain.HasWriteRecordings());
        var expected = s_roots.Take(expectedCount).Select(r => r.RootURL);
        var actual = results.Select(o => (string)o.BaseObject);
        Assert.Equal(expected, actual);
        AssertSingleErrorRecord(exceptionParam);
    }
}
