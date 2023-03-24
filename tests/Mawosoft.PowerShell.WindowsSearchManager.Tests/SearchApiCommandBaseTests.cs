// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

// For theory data parameter MockInterfaceChain.
#pragma warning disable CA1062 // Validate arguments of public methods

[Collection(nameof(NoParallelTests))]
public class SearchApiCommandBaseTests : CommandTestBase
{
    protected readonly MockCommandRuntime CmdRuntime;
    protected readonly MockCommand Command;

    public SearchApiCommandBaseTests() : base()
    {
        CmdRuntime = new MockCommandRuntime();
        Command = new MockCommand(CmdRuntime);
    }

    private void AssertFailingInterfaceThrows(MockInterfaceChain chain, Action action)
    {
        if (chain.NullReferenceIndex >= 0)
        {
            if (chain.ExceptionParam.Exception is NullReferenceException)
            {
                Assert.Throws<NullReferenceException>(action);
            }
            else
            {
                COMException ex = Assert.Throws<COMException>(action);
                Assert.Equal(0, ex.HResult);
                Assert.Contains(MockInterfaceChain.InterfaceNames[chain.NullReferenceIndex], ex.Message, StringComparison.Ordinal);
            }
        }
        else if (chain.ExceptionIndex >= 0)
        {
            Exception ex = Assert.Throws(chain.ExceptionParam.Exception.GetType(), action);
            Assert.Same(chain.ExceptionParam.Exception, ex);
            if (chain.ExceptionParam.IsSearchApi)
            {
                ErrorRecord rec = Assert.Single(CmdRuntime.Errors);
                Assert.Same(ex, rec.Exception);
            }
            else
            {
                Assert.Empty(CmdRuntime.Errors);
            }
        }
    }

    public static IEnumerable<object[]> FailingInterfaceChainCollection(string interfaceName, bool noParents)
    {
        int chainIndex = MockInterfaceChain.InterfaceNames.IndexOf(interfaceName);
        if (chainIndex < 0)
        {
            throw new ArgumentException(null, nameof(interfaceName));
        };

        for (int i = noParents ? chainIndex : 0; i <= chainIndex; i++)
        {
            yield return new object[] { new MockInterfaceChain().WithNullReference(i) };
        }

        Exception_TheoryData exceptions = new();
        for (int i = noParents ? chainIndex : 1; i <= chainIndex; i++)
        {
            foreach (object[] e in exceptions)
            {
                yield return new object[] { new MockInterfaceChain().WithException(i, (ExceptionParam)e[0]) };
            }
        }
    }

    [Fact]
    public void EnsureNotNull_NullArgument_Throws()
    {
        COMException ex = Assert.Throws<COMException>(() => MockCommand.TestEnsureNotNull((ISearchManager)null!));
        Assert.Equal(0, ex.HResult); // special case
        Assert.Contains(nameof(ISearchManager), ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EnsureNotNull_NonNullArgument_Succeeds()
    {
        Assert.Equal(InterfaceChain.SearchManager, MockCommand.TestEnsureNotNull(InterfaceChain.SearchManager));
    }

    [Theory]
    [MemberData(nameof(FailingInterfaceChainCollection), nameof(ISearchManager), false)]
    public void CreateSearchManager_HandlesFailures(MockInterfaceChain chain)
    {
        SearchApiCommandBase.SearchManagerFactory = chain.Factory;
        AssertFailingInterfaceThrows(chain, () => Command.TestCreateSearchManager());
    }

    [Fact]
    public void CreateSearchManager_Succeeds()
    {
        Assert.Same(InterfaceChain.SearchManager, Command.TestCreateSearchManager());
    }

    [Theory]
    [MemberData(nameof(FailingInterfaceChainCollection), nameof(ISearchManager), false)]
    public void GetSearchManager2_HandlesFailures(MockInterfaceChain chain)
    {
        SearchApiCommandBase.SearchManagerFactory = chain.Factory;
        AssertFailingInterfaceThrows(chain, () => Command.TestGetSearchManager2());
    }

    [Fact]
    public void GetSearchManager2_MissingInterface_Throws()
    {
        InterfaceChain.WithSearchManager(new MockSearchManager());
        InvalidCastException ex = Assert.Throws<InvalidCastException>(Command.TestGetSearchManager2);
        Assert.Contains(nameof(ISearchManager2), ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GetSearchManager2_Succeeds()
    {
        Assert.Same(InterfaceChain.SearchManager, Command.TestGetSearchManager2());
    }

    [Fact]
    public void GetSearchManager2_ISearchManager_NullArgument_Throws()
    {
        Assert.Throws<ArgumentNullException>("searchManager", () => Command.TestGetSearchManager2(null!));
    }

    [Fact]
    public void GetSearchManager2_ISearchManager_MissingInterface_Throws()
    {
        InterfaceChain.WithSearchManager(new MockSearchManager());
        InvalidCastException ex = Assert.Throws<InvalidCastException>(() => Command.TestGetSearchManager2(InterfaceChain.SearchManager));
        Assert.Contains(nameof(ISearchManager2), ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GetSearchManager2_ISearchManager_Succeeds()
    {
        Assert.Same(InterfaceChain.SearchManager, Command.TestGetSearchManager2(InterfaceChain.SearchManager));
    }

    [Theory]
    [MemberData(nameof(FailingInterfaceChainCollection), nameof(ISearchCatalogManager), false)]
    public void GetCatalogManager_HandlesFailures(MockInterfaceChain chain)
    {
        SearchApiCommandBase.SearchManagerFactory = chain.Factory;
        AssertFailingInterfaceThrows(chain, () => Command.TestGetCatalogManager(chain.CatalogName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void GetCatalogManager_InvalidCatalogName_Throws(string catalogName)
    {
        Assert.Throws<ArgumentException>(nameof(catalogName), () => Command.TestGetCatalogManager(catalogName));
    }

    [Fact]
    public void GetCatalogManager_Succeeds()
    {
        Assert.Same(InterfaceChain.CatalogManager, Command.TestGetCatalogManager(InterfaceChain.CatalogName));
    }

    [Theory]
    [MemberData(nameof(FailingInterfaceChainCollection), nameof(ISearchCatalogManager), true)]
    public void GetCatalogManager_ISearchManager_HandlesFailures(MockInterfaceChain chain)
    {
        SearchApiCommandBase.SearchManagerFactory = chain.Factory;
        AssertFailingInterfaceThrows(chain, () => Command.TestGetCatalogManager(chain.SearchManager, chain.CatalogName));
    }

    [Fact]
    public void GetCatalogManager_ISearchManager_NullSearchManager_Throws()
    {
        Assert.Throws<ArgumentNullException>("searchManager", () => Command.TestGetCatalogManager(null!, InterfaceChain.CatalogName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void GetCatalogManager_ISearchManager_InvalidCatalogName_Throws(string catalogName)
    {
        Assert.Throws<ArgumentException>(nameof(catalogName), () => Command.TestGetCatalogManager(InterfaceChain.SearchManager, catalogName));
    }

    [Fact]
    public void GetCatalogManager_ISearchManager_Succeeds()
    {
        Assert.Same(InterfaceChain.CatalogManager, Command.TestGetCatalogManager(InterfaceChain.SearchManager, InterfaceChain.CatalogName));
    }

    [Theory]
    [MemberData(nameof(FailingInterfaceChainCollection), nameof(ISearchCrawlScopeManager), false)]
    public void GetCrawlScopeManager_HandlesFailures(MockInterfaceChain chain)
    {
        SearchApiCommandBase.SearchManagerFactory = chain.Factory;
        AssertFailingInterfaceThrows(chain, () => Command.TestGetCrawlScopeManager(chain.CatalogName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void GetCrawlScopeManager_InvalidCatalogName_Throws(string catalogName)
    {
        Assert.Throws<ArgumentException>(nameof(catalogName), () => Command.TestGetCrawlScopeManager(catalogName));
    }

    [Fact]
    public void GetCrawlScopeManager_Succeeds()
    {
        Assert.Same(InterfaceChain.ScopeManager, Command.TestGetCrawlScopeManager(InterfaceChain.CatalogName));
    }

    [Theory]
    [MemberData(nameof(FailingInterfaceChainCollection), nameof(ISearchCrawlScopeManager), true)]
    public void GetCrawlScopeManager_ISearchCatalogManager_HandlesFailures(MockInterfaceChain chain)
    {
        SearchApiCommandBase.SearchManagerFactory = chain.Factory;
        AssertFailingInterfaceThrows(chain, () => Command.TestGetCrawlScopeManager(chain.CatalogManager));
    }

    [Fact]
    public void GetCrawlScopeManager_ISearchCatalogManager_NullCatalogManager_Throws()
    {
        Assert.Throws<ArgumentNullException>("catalogManager", () => Command.TestGetCrawlScopeManager((ISearchCatalogManager)null!));
    }

    [Fact]
    public void GetCrawlScopeManager_ISearchCatalogManager_Succeeds()
    {
        Assert.Same(InterfaceChain.ScopeManager, Command.TestGetCrawlScopeManager(InterfaceChain.CatalogManager));
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void SaveCrawlScopeManager_HandlesFailures(ExceptionParam exceptionParam)
    {
        Exception exception = exceptionParam.Exception;
        InterfaceChain.ScopeManager.AddException("^SaveAll$", exception);
        Exception ex = Assert.Throws(exception.GetType(), () => Command.TestSaveCrawlScopeManager(InterfaceChain.ScopeManager));
        Assert.Same(exception, ex);
        if (exceptionParam.IsSearchApi)
        {
            ErrorRecord rec = Assert.Single(CmdRuntime.Errors);
            Assert.Same(ex, rec.Exception);
        }
        else
        {
            Assert.Empty(CmdRuntime.Errors);
        }
    }

    [Fact]
    public void SaveCrawlScopeManager_NullArgument_Succeeds()
    {
        Command.TestSaveCrawlScopeManager(null);
        Assert.DoesNotContain(InterfaceChain.ScopeManager.RecordedCalls, c => c.MethodName == "SaveAll");
    }

    [Fact]
    public void SaveCrawlScopeManager_Succeeds()
    {
        Command.TestSaveCrawlScopeManager(InterfaceChain.ScopeManager);
        Assert.Single(InterfaceChain.ScopeManager.RecordedCalls, c => c.MethodName == "SaveAll");
    }
}
