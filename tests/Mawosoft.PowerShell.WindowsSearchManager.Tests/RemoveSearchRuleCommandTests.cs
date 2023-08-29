// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class RemoveSearchRuleCommandTests : CommandTestBase
{
    [Theory]
    [InlineData(@"Remove-SearchRule -Path x:\foo", "RemoveScopeRule({0})", @"x:\foo")]
    [InlineData(@"Remove-SearchRule -Path x:\foo -RuleSet User", "RemoveScopeRule({0})", @"x:\foo")]
    [InlineData(@"Remove-SearchRule -Path x:\foo, x:\bar -RuleSet Default", "RemoveDefaultScopeRule({0})", @"x:\foo", @"x:\bar")]
    [InlineData(@"Remove-SearchRule x:\foo, x:\bar Default", "RemoveDefaultScopeRule({0})", @"x:\foo", @"x:\bar")]
    [InlineData(@"@('x:\foo', 'x:\bar') | Remove-SearchRule", "RemoveScopeRule({0})", @"x:\foo", @"x:\bar")]
    [InlineData(@"@([pscustomobject]@{ Path = 'x:\foo'}, [pscustomobject]@{ Path = 'x:\bar'}) | Remove-SearchRule -RuleSet Default", "RemoveDefaultScopeRule({0})", @"x:\foo", @"x:\bar")]
    public void Command_Succeeds(string script, string expectedMethodcall, params string[] expectedMethodParameters)
    {
        Collection<PSObject> results = InvokeScript(script);
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        List<string> recordedCalls = InterfaceChain.ScopeManager.RecordedCalls;
        Assert.All(recordedCalls.Take(recordedCalls.Count - 1),
            (item, i) => Assert.Equal(string.Format(expectedMethodcall, expectedMethodParameters[i]), item));
        Assert.Equal(expectedMethodParameters.Length, recordedCalls.Count - 1);
        Assert.Equal("SaveAll()", recordedCalls[recordedCalls.Count - 1]);
        Assert.True(InterfaceChain.SingleHasWriteRecordings(InterfaceChain.ScopeManager));
    }

    public static readonly object?[][] HandlesFailures_TestData =
        new string[][]
        {
            new string[] { @"Remove-SearchRule -Path x:\foo ", "^RemoveScopeRule$" },
            new string[] { @"Remove-SearchRule x:\foo Default ", "^RemoveDefaultScopeRule$" },
            new string[] { @"Remove-SearchRule -Path x:\foo ", "^SaveAll$" }
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
        AssertSingleErrorRecord(exceptionParam);
    }
}
