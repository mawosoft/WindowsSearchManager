// Copyright (c) Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class RemoveSearchCatalogCommandTests : CommandTestBase
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Command_Succeeds(bool positional)
    {
        string catalogName = "SecondCatalog";
        string script = $"Remove-SearchCatalog {(positional ? "" : "-Catalog")} {catalogName} ";
        Collection<PSObject> results = InvokeScript(script);
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        Assert.Equal($"DeleteCatalog({catalogName})", Assert.Single(InterfaceChain.SearchManager.RecordedCalls));
        Assert.True(InterfaceChain.SingleHasWriteRecordings(InterfaceChain.SearchManager));
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void Command_HandlesFailures(ExceptionParam exceptionParam)
    {
        InterfaceChain.SearchManager.AddException("^DeleteCatalog$", exceptionParam.Exception);
        Collection<PSObject> results = InvokeScript("Remove-SearchCatalog -Catalog SomeCatalog ");
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }
}
