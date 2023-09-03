// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class NewSearchCatalogCommandTests : CommandTestBase
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Command_Succeeds(bool positional)
    {
        string catalogName = "SecondCatalog";
        string script = $"New-SearchCatalog {(positional ? "" : "-Catalog")} {catalogName} ";
        Collection<PSObject> results = InvokeScript(script);
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        Assert.Equal($"CreateCatalog({catalogName})", Assert.Single(InterfaceChain.SearchManager.RecordedCalls));
        Assert.True(InterfaceChain.SingleHasWriteRecordings(InterfaceChain.SearchManager));
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void Command_HandlesFailures(ExceptionParam exceptionParam)
    {
        InterfaceChain.SearchManager.AddException("^CreateCatalog$", exceptionParam.Exception);
        Collection<PSObject> results = InvokeScript("New-SearchCatalog -Catalog SomeCatalog ");
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }
}
