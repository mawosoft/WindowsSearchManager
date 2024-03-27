// Copyright (c) 2023-2024 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class ResetSearchCatalogCommandTests : CommandTestBase
{
    [Theory]
    [InlineData(null)]
    [InlineData("SecondCatalog")]
    public void Command_Succeeds(string? catalogName)
    {
        string script = "Reset-SearchCatalog ";
        if (catalogName is null)
        {
            catalogName = SearchApiCommandBase.DefaultCatalogName;
        }
        else
        {
            script += $"-Catalog '{catalogName}' ";
        }
        Collection<PSObject> results = InvokeScript(script);
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        Assert.Equal($"GetCatalog({catalogName})", Assert.Single(InterfaceChain.SearchManager.RecordedCalls));
        Assert.Equal("Reset()", Assert.Single(InterfaceChain.CatalogManager.RecordedCalls));
        Assert.True(InterfaceChain.SingleHasWriteRecordings(InterfaceChain.CatalogManager));
    }

    [Fact]
    public void Command_NoAdmin_FailsWithCustomMessage()
    {
        InterfaceChain.CatalogManager.AdminMode = false;
        Collection<PSObject> results = InvokeScript("Reset-SearchCatalog ");
        Assert.Empty(results);
        AssertUnauthorizedAccess();
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void Command_HandlesFailures(ExceptionParam exceptionParam)
    {
        InterfaceChain.CatalogManager.AddException("^Reset$", exceptionParam.Exception);
        Collection<PSObject> results = InvokeScript("Reset-SearchCatalog ");
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }
}
