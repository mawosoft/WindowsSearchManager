// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class ResetSearchCatalogCommandTests : CommandTestBase
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("SecondCatalog", false)]
    [InlineData("ThirdCatalog", true)]
    public void ResetSearchCatalog_Succeeds(string? catalogName, bool positional)
    {
        string script = "Reset-SearchCatalog ";
        if (catalogName is null)
        {
            catalogName = SearchApiCommandBase.DefaultCatalogName;
        }
        else
        {
            if (!positional) script += "-Catalog ";
            script += $"'{catalogName}' ";
        }
        Collection<PSObject> results = InvokeScript(script);
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        Assert.Equal($"GetCatalog({catalogName})", Assert.Single(InterfaceChain.SearchManager.RecordedCalls));
        Assert.Equal("Reset()", Assert.Single(InterfaceChain.CatalogManager.RecordedCalls));
    }

    [Fact]
    public void ResetSearchCatalog_NoAdmin_Fails()
    {
        InterfaceChain.CatalogManager.AdminMode = false;
        Collection<PSObject> results = InvokeScript("Reset-SearchCatalog ");
        Assert.Empty(results);
        AssertUnauthorizedAccess();
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void ResetSearchCatalog_HandlesFailures(ExceptionParam exceptionParam)
    {
        InterfaceChain.CatalogManager.AddException("^Reset$", exceptionParam.Exception);
        Collection<PSObject> results = InvokeScript("Reset-SearchCatalog ");
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }
}
