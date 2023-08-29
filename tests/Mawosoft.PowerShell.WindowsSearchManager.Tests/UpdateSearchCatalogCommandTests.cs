// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class UpdateSearchCatalogCommandTests : CommandTestBase
{
    [Theory]
    [InlineData(@"Update-SearchCatalog", "Reindex()", "")]
    [InlineData(@"Update-SearchCatalog -All", "Reindex()", "")]
    [InlineData(@"Update-SearchCatalog -RootPath x:\", "ReindexSearchRoot({0})", @"x:\")]
    [InlineData(@"Update-SearchCatalog -RootPath x:\, y:\", "ReindexSearchRoot({0})", @"x:\", @"y:\")]
    [InlineData(@"@([pscustomobject]@{ RootPath = 'x:\'}, [pscustomobject]@{ RootPath = 'y:\'}) | Update-SearchCatalog", "ReindexSearchRoot({0})", @"x:\", @"y:\")]
    [InlineData(@"Update-SearchCatalog -Path x:\foo", "ReindexMatchingURLs({0})", @"x:\foo")]
    [InlineData(@"Update-SearchCatalog -Path x:\foo, x:\bar", "ReindexMatchingURLs({0})", @"x:\foo", @"x:\bar")]
    [InlineData(@"Update-SearchCatalog x:\foo, x:\bar", "ReindexMatchingURLs({0})", @"x:\foo", @"x:\bar")]
    [InlineData(@"@('x:\foo', 'x:\bar') | Update-SearchCatalog", "ReindexMatchingURLs({0})", @"x:\foo", @"x:\bar")]
    [InlineData(@"@([pscustomobject]@{ Path = 'x:\foo'}, [pscustomobject]@{ Path = 'x:\bar'}) | Update-SearchCatalog", "ReindexMatchingURLs({0})", @"x:\foo", @"x:\bar")]
    public void Command_Succeeds(string script, string expectedMethodcall, params string[] expectedMethodParameters)
    {
        Collection<PSObject> results = InvokeScript(script);
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        Assert.All(InterfaceChain.CatalogManager.RecordedCalls,
            (item, i) => Assert.Equal(string.Format(expectedMethodcall, expectedMethodParameters[i]), item));
        Assert.Equal(expectedMethodParameters.Length, InterfaceChain.CatalogManager.RecordedCalls.Count);
        Assert.True(InterfaceChain.SingleHasWriteRecordings(InterfaceChain.CatalogManager));
    }

    [Fact]
    public void AllParameterSet_NoAdmin_FailsWithCustomMessage()
    {
        InterfaceChain.CatalogManager.AdminMode = false;
        Collection<PSObject> results = InvokeScript("Update-SearchCatalog -All ");
        Assert.Empty(results);
        AssertUnauthorizedAccess();
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void AllParameterSet_HandlesFailures(ExceptionParam exceptionParam)
    {
        InterfaceChain.CatalogManager.AddException("^Reindex$", exceptionParam.Exception);
        Collection<PSObject> results = InvokeScript("Update-SearchCatalog -All ");
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }

    [Theory]
    [InlineData("RootPath", true)]
    [InlineData("RootPath", false)]
    [InlineData("Path", true)]
    [InlineData("Path", false)]
    public void RootAndPathParameterSet_WithFailures_PartiallySucceeds(string parameterName, bool usePipeline)
    {
        string expectedMethodcall = parameterName == "Path" ? "ReindexMatchingURLs({0})" : "ReindexSearchRoot({0})";
        string exceptionRegex = parameterName == "Path" ? "" : "";

        InterfaceChain.CatalogManager.AddException("^ReindexSearchRoot$|^ReindexMatchingURLs$", new Exception(), 2, 4);
        string[] parameterValues = { @"x:\foo1", @"x:\foo2", @"x:\foo3", @"x:\foo4", @"x:\foo5" };
        string script = "Update-SearchCatalog";
        IEnumerable<PSObject>? input = null;
        if (usePipeline)
        {
            input = parameterValues.Select(p =>
            {
                PSObject o = new();
                o.Properties.Add(new PSNoteProperty(parameterName, p));
                return o;
            });
        }
        else
        {
            script += $" -{parameterName} {string.Join(",", parameterValues)} ";
        }
        Collection<PSObject> results = InvokeScript(script, input);
        Assert.Empty(results);
        Assert.True(PowerShell.HadErrors);
        Assert.All(InterfaceChain.CatalogManager.RecordedCalls,
            (item, i) => Assert.Equal(string.Format(expectedMethodcall, parameterValues[i]), item));
        Assert.Equal(parameterValues.Length, InterfaceChain.CatalogManager.RecordedCalls.Count);
        Assert.True(InterfaceChain.SingleHasWriteRecordings(InterfaceChain.CatalogManager));
        Assert.Collection(PowerShell.Streams.Error,
            e => Assert.Equal($@"SystemIndex {parameterName}=x:\foo2", e.TargetObject),
            e => Assert.Equal($@"SystemIndex {parameterName}=x:\foo4", e.TargetObject));
    }
}
