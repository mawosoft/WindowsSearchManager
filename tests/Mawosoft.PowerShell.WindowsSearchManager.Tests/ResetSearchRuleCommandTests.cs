// Copyright (c) 2023-2024 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class ResetSearchRuleCommandTests : CommandTestBase
{
    [Fact]
    public void Command_Succeeds()
    {
        Collection<PSObject> results = InvokeScript("Reset-SearchRule ");
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        Assert.Collection(InterfaceChain.ScopeManager.RecordedCalls,
            c => Assert.Equal("RevertToDefaultScopes()", c),
            c => Assert.Equal("SaveAll()", c));
        Assert.True(InterfaceChain.SingleHasWriteRecordings(InterfaceChain.ScopeManager));
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void Command_HandlesFailures(ExceptionParam exceptionParam)
    {
        InterfaceChain.ScopeManager.AddException("^RevertToDefaultScopes$", exceptionParam.Exception);
        Collection<PSObject> results = InvokeScript("Reset-SearchRule ");
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }
}
