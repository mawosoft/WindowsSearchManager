// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class GetSearchRuleCommandTests : CommandTestBase
{
    private static readonly List<MockSearchScopeRule> s_rules =
    [
        new()
        {
            PatternOrURL = @"x:\foo",
            IsIncluded = 1,
            IsDefault = 1
        },
        new()
        {
            PatternOrURL = @"x:\bar\foo",
            IsDefault = 1
        },
        new()
        {
            PatternOrURL = @"x:\foo\bar",
            IsIncluded = 1,
        },
        new()
        {
            PatternOrURL = @"x:\foo\bar\foo"
        },
        new()
        {
            PatternOrURL = @"x:\foo\bar\baz"
        }
    ];

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public void Command_Succeeds(int ruleCount)
    {
        InterfaceChain.ScopeManager.Rules = new(s_rules.Take(ruleCount));
        Collection<PSObject> results = InvokeScript("Get-SearchRule ");
        Assert.False(PowerShell.HadErrors);
        Assert.False(InterfaceChain.HasWriteRecordings());
        var expected = s_rules.Take(ruleCount);
        var actual = results.Select(o => (SearchRuleInfo)o.BaseObject);
        Assert.Equal(expected, actual, SearchRuleInfoToMockComparer.Instance);
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void Command_WithFailures_PartiallySucceeds(ExceptionParam exceptionParam)
    {
        InterfaceChain.ScopeManager.Rules = new(s_rules);
        int expectedCount = 2;
        Assert.InRange(expectedCount, expectedCount, s_rules.Count - 2);
        InterfaceChain.ScopeManager.Rules[expectedCount] = exceptionParam.Exception;
        Collection<PSObject> results = InvokeScript("Get-SearchRule ");
        Assert.True(PowerShell.HadErrors);
        Assert.False(InterfaceChain.HasWriteRecordings());
        var expected = s_rules.Take(expectedCount);
        var actual = results.Select(o => (SearchRuleInfo)o.BaseObject);
        Assert.Equal(expected, actual, SearchRuleInfoToMockComparer.Instance);
        AssertSingleErrorRecord(exceptionParam);
    }
}
