// Copyright (c) Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class AddSearchRuleCommandTests : CommandTestBase
{
    private Collection<PSObject> InvokeInputParameterSet(List<SearchRuleInfo> ruleInfos, string valueType, bool overrideChildren, bool usePipeline)
    {
        IList<object> inputValues;
        switch (valueType)
        {
            case nameof(SearchRuleInfo):
                inputValues = ruleInfos.ConvertAll(r => r.Clone());
                break;
            case nameof(PSObject):
                inputValues = ruleInfos.ConvertAll(r => (object)new PSObject(r.Clone()));
                break;
            case nameof(PSCustomObject):
                PSObject defaultInfo = new(new SearchRuleInfo());
                inputValues = new List<object>(ruleInfos.Count);
                foreach (SearchRuleInfo info in ruleInfos)
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

        if (usePipeline)
        {
            return InvokeScript(overrideChildren ? "Add-SearchRule -OverrideChildren" : "Add-SearchRule", inputValues);
        }
        else
        {
            object values = inputValues.Count == 1 ? inputValues[0] : inputValues;
            Dictionary<string, object> parameters = new() { { "InputObject", values } };
            if (overrideChildren) parameters.Add("OverrideChildren", true);
            return InvokeCommand("Add-SearchRule", parameters);
        }
    }

    private static List<string> GetExpectedMethodCalls(List<SearchRuleInfo> ruleInfos, bool overrideChildren)
        => ruleInfos.ConvertAll(r => r.RuleSet == SearchRuleInfo.SearchRuleSet.User
            ? $"AddUserScopeRule({r.Path},{(r.RuleType == SearchRuleInfo.SearchRuleType.Include ? 1 : 0)},{(overrideChildren ? 1 : 0)},{(uint)r.FollowFlags})"
            : $"AddDefaultScopeRule({r.Path},{(r.RuleType == SearchRuleInfo.SearchRuleType.Include ? 1 : 0)},{(uint)r.FollowFlags})");

    private void Assert_InvokeSucceeded(Collection<PSObject> results, IList<string> expectedMethodCalls)
    {
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        List<string> recordedCalls = InterfaceChain.ScopeManager.RecordedCalls;
        Assert.All(recordedCalls.Take(recordedCalls.Count - 1),
            (item, i) => Assert.Equal(expectedMethodCalls[i], item));
        Assert.Equal(expectedMethodCalls.Count, recordedCalls.Count - 1);
        Assert.Equal("SaveAll()", InterfaceChain.ScopeManager.RecordedCalls[recordedCalls.Count - 1]);
        Assert.True(InterfaceChain.SingleHasWriteRecordings(InterfaceChain.ScopeManager));
    }

    private static readonly List<SearchRuleInfo> s_ruleInfos =
    [
        new()
        {
            Path = @"x:\foo",
            RuleType = SearchRuleInfo.SearchRuleType.Include,
            RuleSet = SearchRuleInfo.SearchRuleSet.Default,
            FollowFlags = SearchRuleInfo._FOLLOW_FLAGS.FF_SUPPRESSINDEXING,
        },
        new()
        {
            Path = @"x:\foo\foo",
            RuleType = SearchRuleInfo.SearchRuleType.Exclude,
            RuleSet = SearchRuleInfo.SearchRuleSet.Default,
        },
        new()
        {
            Path = @"x:\foo\bar",
            RuleType = SearchRuleInfo.SearchRuleType.Exclude,
            RuleSet = SearchRuleInfo.SearchRuleSet.User,
        },
        new()
        {
            Path = @"x:\bar\foo",
            RuleType = SearchRuleInfo.SearchRuleType.Include,
            RuleSet = SearchRuleInfo.SearchRuleSet.User,
            FollowFlags = SearchRuleInfo._FOLLOW_FLAGS.FF_SUPPRESSINDEXING,
        },
        new()
        {
            Path = @"x:\foo\baz\bar",
            RuleType = SearchRuleInfo.SearchRuleType.Include,
            RuleSet = SearchRuleInfo.SearchRuleSet.User,
        }
    ];

    [Theory]
    [InlineData(@"Add-SearchRule -Path x:\foo -RuleType Include", @"AddUserScopeRule(x:\foo,1,0,1)")]
    [InlineData(@"Add-SearchRule -Path x:\foo -RuleType Exclude -OverrideChildren", @"AddUserScopeRule(x:\foo,0,1,1)")]
    [InlineData(@"Add-SearchRule -Path x:\foo, x:\bar -RuleType Include -OverrideChildren", @"AddUserScopeRule(x:\foo,1,1,1)", @"AddUserScopeRule(x:\bar,1,1,1)")]
    [InlineData(@"Add-SearchRule x:\foo, x:\bar Include Default", @"AddDefaultScopeRule(x:\foo,1,1)", @"AddDefaultScopeRule(x:\bar,1,1)")]
    [InlineData(@"@('x:\foo', 'x:\bar') | Add-SearchRule -RuleSet 1 -RuleType Exclude", @"AddDefaultScopeRule(x:\foo,0,1)", @"AddDefaultScopeRule(x:\bar,0,1)")]
    public void PathParameterSet_Succeeds(string script, params string[] expectedMethodCalls)
    {
        Collection<PSObject> results = InvokeScript(script);
        Assert_InvokeSucceeded(results, expectedMethodCalls);
    }

    [Theory]
    [InlineData(1, nameof(SearchRuleInfo), false)]
    [InlineData(1, nameof(PSObject), false)]
    [InlineData(1, nameof(PSCustomObject), false)]
    [InlineData(-1, nameof(SearchRuleInfo), false)]
    [InlineData(-1, nameof(PSObject), false)]
    [InlineData(-1, nameof(PSCustomObject), false)]
    [InlineData(-1, nameof(SearchRuleInfo), true)]
    [InlineData(-1, nameof(PSObject), true)]
    public void InputParameterSet_Succeeds(int valueCount, string valueType, bool usePipeline)
    {
        if (valueCount <= 0) valueCount = s_ruleInfos.Count;
        List<SearchRuleInfo> ruleInfos = s_ruleInfos.GetRange(0, valueCount);
        Collection<PSObject> results = InvokeInputParameterSet(ruleInfos, valueType, overrideChildren: false, usePipeline);
        Assert_InvokeSucceeded(results, GetExpectedMethodCalls(ruleInfos, overrideChildren: false));
    }

    [Theory]
    [InlineData(@"@('x:\foo', 'x:\bar') | Add-SearchRule -RuleSet Default -RuleType Exclude -OverrideChildren", 2, @"AddDefaultScopeRule(x:\foo,0,1)", @"AddDefaultScopeRule(x:\bar,0,1)")]
    public void PathParameterSet_RuleSetDefault_WithOverrideChildren_WritesWarnings(string script, int expectedWarningCount, params string[] expectedMethodCalls)
    {
        Collection<PSObject> results = InvokeScript(script);
        Assert_InvokeSucceeded(results, expectedMethodCalls);
        Assert.Equal(expectedWarningCount, PowerShell.Streams.Warning.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InputParameterSet_RuleSetDefault_WithOverrideChildren_WritesWarnings(bool usePipeline)
    {
        int expectedWarningCount = s_ruleInfos.Count(r => r.RuleSet == SearchRuleInfo.SearchRuleSet.Default);
        Assert.InRange(expectedWarningCount, 1, s_ruleInfos.Count - 1);
        Collection<PSObject> results = InvokeInputParameterSet(s_ruleInfos, nameof(PSObject), overrideChildren: true, usePipeline);
        Assert_InvokeSucceeded(results, GetExpectedMethodCalls(s_ruleInfos, overrideChildren: true));
        Assert.Equal(expectedWarningCount, PowerShell.Streams.Warning.Count);
    }

    public static readonly object?[][] HandlesFailures_TestData =
        new string[][]
        {
            [@"Add-SearchRule -Path x:\foo -RuleType Include ", "^AddUserScopeRule$"],
            [@"Add-SearchRule -Path x:\foo -RuleSet Default -RuleType Include ", "^AddDefaultScopeRule$"],
            [@"Add-SearchRule -Path x:\foo -RuleType Include ", "^SaveAll$"],
            [@"Add-SearchRule -Path x:\foo -RuleSet Default -RuleType Include ", "^SaveAll$"],
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
