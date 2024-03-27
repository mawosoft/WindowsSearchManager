// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class RemoveSearchRootCommandTests : CommandTestBase
{
    [Theory]
    [InlineData(@"Remove-SearchRoot -Path x:\foo", @"x:\foo")]
    [InlineData(@"Remove-SearchRoot -Path x:\foo, x:\bar", @"x:\foo", @"x:\bar")]
    [InlineData(@"Remove-SearchRoot x:\foo, x:\bar", @"x:\foo", @"x:\bar")]
    [InlineData(@"@('x:\foo', 'x:\bar') | Remove-SearchRoot", @"x:\foo", @"x:\bar")]
    [InlineData(@"@([pscustomobject]@{ Path = 'x:\foo'}, [pscustomobject]@{ Path = 'x:\bar'}) | Remove-SearchRoot", @"x:\foo", @"x:\bar")]
    [InlineData(@"@([Mawosoft.PowerShell.WindowsSearchManager.SearchRootInfo]@{ Path = 'x:\foo'}, [Mawosoft.PowerShell.WindowsSearchManager.SearchRootInfo]@{ Path = 'x:\bar'}) | Remove-SearchRoot", @"x:\foo", @"x:\bar")]
    public void Command_Succeeds(string script, params string[] expectedMethodParameters)
    {
        Collection<PSObject> results = InvokeScript(script);
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        List<string> recordedCalls = InterfaceChain.ScopeManager.RecordedCalls;
        Assert.All(recordedCalls.Take(recordedCalls.Count - 1),
            (item, i) => Assert.Equal($"RemoveRoot({expectedMethodParameters[i]})", item));
        Assert.Equal(expectedMethodParameters.Length, recordedCalls.Count - 1);
        Assert.Equal("SaveAll()", recordedCalls[recordedCalls.Count - 1]);
        Assert.True(InterfaceChain.SingleHasWriteRecordings(InterfaceChain.ScopeManager));
    }

    public static readonly object?[][] HandlesFailures_TestData =
        new string[][]
        {
            [@"Remove-SearchRoot -Path x:\foo ", "^RemoveRoot$"],
            [@"Remove-SearchRoot -Path x:\foo ", "^SaveAll$"]
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
