// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class AddSearchRootCommandTests : CommandTestBase
{
    [Theory]
    [InlineData(@"Add-SearchRoot -Path x:\foo", @"x:\foo")]
    [InlineData(@"Add-SearchRoot -Path x:\foo, x:\bar", @"x:\foo", @"x:\bar")]
    [InlineData(@"Add-SearchRoot x:\foo, x:\bar", @"x:\foo", @"x:\bar")]
    [InlineData(@"@('x:\foo', 'x:\bar') | Add-SearchRoot", @"x:\foo", @"x:\bar")]
    public void PathParameterSet_Succeeds(string script, params string[] expectedMethodParameters)
    {
        Collection<PSObject> results = InvokeScript(script);
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        var recordedCallInfos = InterfaceChain.ScopeManager.RecordedCallInfos;
        Assert.All(recordedCallInfos.Take(recordedCallInfos.Count - 1),
            (item, i) =>
            {
                Assert.Equal("AddRoot", item.MethodName);
                ISearchRoot root = (Assert.Single(item.Parameters) as ISearchRoot)!;
                Assert.NotNull(root);
                SearchRootInfo expected = new() { Path = expectedMethodParameters[i] };
                SearchRootInfo actual = new(root);
                Assert.Equivalent(expected, actual, strict: true);
            });
        Assert.Equal(expectedMethodParameters.Length, recordedCallInfos.Count - 1);
        Assert.Equal("SaveAll()", InterfaceChain.ScopeManager.RecordedCalls[recordedCallInfos.Count - 1]);
        Assert.True(InterfaceChain.SingleHasWriteRecordings(InterfaceChain.ScopeManager));
    }

    private static readonly List<SearchRootInfo> s_rootInfos = new()
    {
        new()
        {
            Path = @"fooprotocol://{bar-sid}/",
            ProvidesNotifications = false,
            UseNotificationsOnly = true,
            EnumerationDepth = 333,
            HostDepth = 222,
            AuthenticationType = _AUTH_TYPE.eAUTH_TYPE_BASIC
        },
        new()
        {
            Path = @"file:///c:\",
            IsHierarchical = false,
            UseNotificationsOnly = true,
            EnumerationDepth = 333,
            HostDepth = 222,
        },
        new()
        {
            Path = @"bar://foo/",
            FollowDirectories = false,
            AuthenticationType = _AUTH_TYPE.eAUTH_TYPE_NTLM
        },
        new()
        {
            Path = @"x:\",
        },
        new()
        {
            Path = @"y:\",
        }
    };

    [Theory]
    [InlineData(1, nameof(SearchRootInfo), false)]
    [InlineData(1, nameof(PSObject), false)]
    [InlineData(1, nameof(PSCustomObject), false)]
    [InlineData(-1, nameof(SearchRootInfo), false)]
    [InlineData(-1, nameof(PSObject), false)]
    [InlineData(-1, nameof(PSCustomObject), false)]
    [InlineData(-1, nameof(SearchRootInfo), true)]
    [InlineData(-1, nameof(PSObject), true)]
    public void InputParameterSet_Succeeds(int valueCount, string valueType, bool usePipeline)
    {
        if (valueCount <= 0) valueCount = s_rootInfos.Count;
        List<SearchRootInfo> expected = s_rootInfos.GetRange(0, valueCount);
        IList<object> inputValues;
        switch (valueType)
        {
            case nameof(SearchRootInfo):
                inputValues = s_rootInfos.GetRange(0, valueCount).ConvertAll(r => r.Clone());
                break;
            case nameof(PSObject):
                inputValues = s_rootInfos.GetRange(0, valueCount).ConvertAll(r => (object)new PSObject(r.Clone()));
                break;
            case nameof(PSCustomObject):
                PSObject defaultInfo = new(new SearchRootInfo());
                inputValues = new List<object>(valueCount);
                foreach (SearchRootInfo info in expected)
                {
                    PSObject source = new(info);
                    PSObject result = new();
                    foreach (var p in source.Properties)
                    {
                        if (!p.Value.Equals(defaultInfo.Properties[p.Name].Value))
                        {
                            result.Properties.Add(new PSNoteProperty(p.Name, p.Value));
                        }
                    }
                    inputValues.Add(result);
                }
                break;
            default:
                throw new ArgumentException(null, nameof(valueType));
        }

        Collection<PSObject> results;
        if (usePipeline)
        {
            results = InvokeScript("Add-SearchRoot", inputValues);
        }
        else
        {
            object values = valueCount == 1 ? inputValues[0] : inputValues;
            Dictionary<string, object> parameters = new() { { "InputObject", values } };
            results = InvokeCommand("Add-SearchRoot", parameters);
        }
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        var recordedCallInfos = InterfaceChain.ScopeManager.RecordedCallInfos;
        Assert.All(recordedCallInfos.Take(recordedCallInfos.Count - 1),
            (item, i) =>
            {
                Assert.Equal("AddRoot", item.MethodName);
                ISearchRoot root = (Assert.Single(item.Parameters) as ISearchRoot)!;
                Assert.NotNull(root);
                SearchRootInfo actual = new(root);
                Assert.Equivalent(expected[i], actual, strict: true);
            });
        Assert.Equal(expected.Count, recordedCallInfos.Count - 1);
        Assert.Equal("SaveAll()", InterfaceChain.ScopeManager.RecordedCalls[recordedCallInfos.Count - 1]);
        Assert.True(InterfaceChain.SingleHasWriteRecordings(InterfaceChain.ScopeManager));
    }

    public static readonly object?[][] HandlesFailures_TestData =
        new string[][]
        {
            new string[] { @"Add-SearchRoot -Path x:\foo ", "^AddRoot$" },
            new string[] { @"Add-SearchRoot -Path x:\foo ", "^SaveAll$" }
        }
        .CrossJoin(new Exception_TheoryData())
        .ToArray();

    [Theory]
    [MemberData(nameof(HandlesFailures_TestData))]
    public void Command_HandlesFailures(string script, string exceptionRegex, ExceptionParam exceptionParam)
    {
        InterfaceChain.ScopeManager.AddException(exceptionRegex, exceptionParam.Exception);
        Collection<PSObject> results = InvokeScript(script);
        Assert.Empty(results);
        if (exceptionRegex.IndexOf("SaveAll", StringComparison.Ordinal) < 0)
        {
            Assert.DoesNotContain("SaveAll()", InterfaceChain.ScopeManager.RecordedCalls);
        }
        AssertSingleErrorRecord(exceptionParam);
    }
}
