// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

[Collection(nameof(NoParallelTests))]
public class SearchApiCommandBaseTests
{
    private class MockCommand : SearchApiCommandBase
    {
        internal MockCommand() : base()
        {
            if (SearchManagerFactory == DefaultSearchManagerFactory.Instance)
            {
                SearchManagerFactory = new MockSearchManagerFactory();
            }
        }

        internal MockCommand(ISearchManagerFactory searchManagerFactory) : base()
            => SearchManagerFactory = searchManagerFactory;

        internal MockCommand(ICommandRuntime commandRuntime, ISearchManagerFactory searchManagerFactory) : base()
        {
            SearchManagerFactory = searchManagerFactory;
            CommandRuntime = commandRuntime;
        }

        // Internal TestXxx wrappers for protected methods
        internal static T TestEnsureNotNull<T>(T value) where T : class
            => EnsureNotNull(value);
        internal ISearchManager TestCreateSearchManager() => CreateSearchManager();
        internal ISearchManager2 TestGetSearchManager2() => GetSearchManager2();
        internal ISearchManager2 TestGetSearchManager2(ISearchManager searchManager)
            => GetSearchManager2(searchManager);
        internal ISearchCatalogManager TestGetCatalogManager(string catalogName)
            => GetCatalogManager(catalogName);
        internal ISearchCatalogManager TestGetCatalogManager(ISearchManager searchManager, string catalogName)
            => GetCatalogManager(searchManager, catalogName);
        internal ISearchCrawlScopeManager TestGetCrawlScopeManager(string catalogName)
            => GetCrawlScopeManager(catalogName);
        internal ISearchCrawlScopeManager TestGetCrawlScopeManager(ISearchCatalogManager catalogManager)
            => GetCrawlScopeManager(catalogManager);
        internal void TestSaveCrawlScopeManager(ISearchCrawlScopeManager? scopeManager)
            => SaveCrawlScopeManager(scopeManager);
    }

    private class Exception_TheoryData : TheoryData<ExceptionParam, bool>
    {
        public Exception_TheoryData()
        {
            Add(new ExceptionParam(new Exception()), false);
            Add(new ExceptionParam(new COMException()), false);
            Add(new ExceptionParam(new COMException(null, unchecked((int)0x80042103))), true);
        }
    }

    public static IEnumerable<object[]> InterfaceChainCollectionForFailures(Type interfaceType, bool noParents)
    {
        int chainIndex = MockInterfaceChain.TypeToIndex(interfaceType);
        if (chainIndex < 0)
        {
            throw new ArgumentException(null, nameof(interfaceType));
        }

        for (int i = noParents ? chainIndex : 0; i <= chainIndex; i++)
        {
            yield return new object[] { new MockInterfaceChain().WithNullReference(i) };
        }

        Exception_TheoryData exceptions = new();
        for (int i = noParents ? chainIndex : 1; i <= chainIndex; i++)
        {
            foreach (object[] e in exceptions)
            {
                yield return new object[] { new MockInterfaceChain().WithException(i, ((ExceptionParam)e[0]).Value, (bool)e[1]) };
            }
        }
    }

    [Fact]
    public void EnsureNotNull_NullArgument_Throws()
    {
        COMException ex = Assert.Throws<COMException>(() => MockCommand.TestEnsureNotNull((ISearchManager)null!));
        Assert.Equal(0, ex.HResult); // special case
        Assert.Contains(nameof(ISearchManager), ex.Message);
    }

    [Fact]
    public void EnsureNotNull_NonNullArgument_Succeeds()
    {
        MockSearchManager manager = new();
        Assert.NotNull(manager); // Paranoid
        Assert.Equal(manager, MockCommand.TestEnsureNotNull(manager));
    }

    [Theory]
    [MemberData(nameof(InterfaceChainCollectionForFailures), typeof(ISearchManager), false)]
    internal void CreateSearchManager_HandlesFailures(MockInterfaceChain chain)
    {
        MockCommandRuntime runtime = new();
        MockCommand command = new(runtime, chain.Factory);
        chain.AssertThrows(() => command.TestCreateSearchManager(), runtime);
    }

    [Fact]
    public void CreateSearchManager_Succeeds()
    {
        MockInterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Same(chain.SearchManager, command.TestCreateSearchManager());
    }

    [Theory]
    [MemberData(nameof(InterfaceChainCollectionForFailures), typeof(ISearchManager), false)]
    internal void GetSearchManager2_HandlesFailures(MockInterfaceChain chain)
    {
        MockCommandRuntime runtime = new();
        MockCommand command = new(runtime, chain.Factory);
        chain.AssertThrows(() => command.TestGetSearchManager2(), runtime);
    }

    [Fact]
    internal void GetSearchManager2_MissingInterface_Throws()
    {
        MockCommandRuntime runtime = new();
        MockInterfaceChain chain = new MockInterfaceChain().WithSearchManager(new MockSearchManager());
        MockCommand command = new(runtime, chain.Factory);
        InvalidCastException ex = Assert.Throws<InvalidCastException>(command.TestGetSearchManager2);
        Assert.Contains(nameof(ISearchManager2), ex.Message);
    }

    [Fact]
    public void GetSearchManager2_Succeeds()
    {
        MockInterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Same(chain.SearchManager, command.TestGetSearchManager2());
    }

    [Fact]
    internal void GetSearchManager2_ISearchManager_NullArgument_Throws()
    {
        MockCommandRuntime runtime = new();
        MockInterfaceChain chain = new();
        MockCommand command = new(runtime, chain.Factory);
        Assert.Throws<ArgumentNullException>("searchManager", () => command.TestGetSearchManager2(null!));
    }

    [Fact]
    internal void GetSearchManager2_ISearchManager_MissingInterface_Throws()
    {
        MockCommandRuntime runtime = new();
        MockInterfaceChain chain = new MockInterfaceChain().WithSearchManager(new MockSearchManager());
        MockCommand command = new(runtime, chain.Factory);
        InvalidCastException ex = Assert.Throws<InvalidCastException>(() => command.TestGetSearchManager2(chain.SearchManager));
        Assert.Contains(nameof(ISearchManager2), ex.Message);
    }

    [Fact]
    public void GetSearchManager2_ISearchManager_Succeeds()
    {
        MockInterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Same(chain.SearchManager, command.TestGetSearchManager2(chain.SearchManager));
    }

    [Theory]
    [MemberData(nameof(InterfaceChainCollectionForFailures), typeof(ISearchCatalogManager), false)]
    internal void GetCatalogManager_HandlesFailures(MockInterfaceChain chain)
    {
        MockCommandRuntime runtime = new();
        MockCommand command = new(runtime, chain.Factory);
        chain.AssertThrows(() => command.TestGetCatalogManager(chain.CatalogName), runtime);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    internal void GetCatalogManager_InvalidCatalogName_Throws(string catalogName)
    {
        MockInterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Throws<ArgumentException>(nameof(catalogName), () => command.TestGetCatalogManager(catalogName));
    }

    [Fact]
    public void GetCatalogManager_Succeeds()
    {
        MockInterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Same(chain.CatalogManager, command.TestGetCatalogManager(chain.CatalogName));
    }

    [Theory]
    [MemberData(nameof(InterfaceChainCollectionForFailures), typeof(ISearchCatalogManager), true)]
    internal void GetCatalogManager_ISearchManager_HandlesFailures(MockInterfaceChain chain)
    {
        MockCommandRuntime runtime = new();
        MockCommand command = new(runtime, chain.Factory);
        chain.AssertThrows(() => command.TestGetCatalogManager(chain.SearchManager, chain.CatalogName), runtime);
    }

    [Fact]
    internal void GetCatalogManager_ISearchManager_NullSearchManager_Throws()
    {
        MockCommandRuntime runtime = new();
        MockInterfaceChain chain = new();
        MockCommand command = new(runtime, chain.Factory);
        Assert.Throws<ArgumentNullException>("searchManager", () => command.TestGetCatalogManager(null!, chain.CatalogName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    internal void GetCatalogManager_ISearchManager_InvalidCatalogName_Throws(string catalogName)
    {
        MockInterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Throws<ArgumentException>(nameof(catalogName), () => command.TestGetCatalogManager(chain.SearchManager, catalogName));
    }

    [Fact]
    public void GetCatalogManager_ISearchManager_Succeeds()
    {
        MockInterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Same(chain.CatalogManager, command.TestGetCatalogManager(chain.SearchManager, chain.CatalogName));
    }

    [Theory]
    [MemberData(nameof(InterfaceChainCollectionForFailures), typeof(ISearchCrawlScopeManager), false)]
    internal void GetCrawlScopeManager_HandlesFailures(MockInterfaceChain chain)
    {
        MockCommandRuntime runtime = new();
        MockCommand command = new(runtime, chain.Factory);
        chain.AssertThrows(() => command.TestGetCrawlScopeManager(chain.CatalogName), runtime);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    internal void GetCrawlScopeManager_InvalidCatalogName_Throws(string catalogName)
    {
        MockInterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Throws<ArgumentException>(nameof(catalogName), () => command.TestGetCrawlScopeManager(catalogName));
    }

    [Fact]
    public void GetCrawlScopeManager_Succeeds()
    {
        MockInterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Same(chain.ScopeManager, command.TestGetCrawlScopeManager(chain.CatalogName));
    }

    [Theory]
    [MemberData(nameof(InterfaceChainCollectionForFailures), typeof(ISearchCrawlScopeManager), true)]
    internal void GetCrawlScopeManager_ISearchCatalogManager_HandlesFailures(MockInterfaceChain chain)
    {
        MockCommandRuntime runtime = new();
        MockCommand command = new(runtime, chain.Factory);
        chain.AssertThrows(() => command.TestGetCrawlScopeManager(chain.CatalogManager), runtime);
    }

    [Fact]
    internal void GetCrawlScopeManager_ISearchCatalogManager_NullCatalogManager_Throws()
    {
        MockCommandRuntime runtime = new();
        MockInterfaceChain chain = new();
        MockCommand command = new(runtime, chain.Factory);
        Assert.Throws<ArgumentNullException>("catalogManager", () => command.TestGetCrawlScopeManager((ISearchCatalogManager)null!));
    }

    [Fact]
    public void GetCrawlScopeManager_ISearchCatalogManager_Succeeds()
    {
        MockInterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Same(chain.ScopeManager, command.TestGetCrawlScopeManager(chain.CatalogManager));
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void SaveCrawlScopeManager_HandlesFailures(ExceptionParam exceptionParam, bool shouldHaveErrorRecord)
    {
        Exception exception = exceptionParam.Value;
        MockCommandRuntime runtime = new();
        MockInterfaceChain chain = new();
        MockCommand command = new(runtime, chain.Factory);
        chain.ScopeManager.SaveAllException = exception;
        Exception ex = Assert.Throws(exception.GetType(), () => command.TestSaveCrawlScopeManager(chain.ScopeManager));
        Assert.Same(exception, ex);
        if (shouldHaveErrorRecord)
        {
            ErrorRecord rec = Assert.Single(runtime.Errors);
            Assert.Same(ex, rec.Exception);
        }
        else
        {
            Assert.Empty(runtime.Errors);
        }
    }

    [Fact]
    public void SaveCrawlScopeManager_NullArgument_Succeeds()
    {
        MockCommandRuntime runtime = new();
        MockInterfaceChain chain = new();
        MockCommand command = new(runtime, chain.Factory);
        command.TestSaveCrawlScopeManager(null);
        Assert.Equal(0, chain.ScopeManager.SaveAllCallCount);
    }

    [Fact]
    public void SaveCrawlScopeManager_Succeeds()
    {
        MockCommandRuntime runtime = new();
        MockInterfaceChain chain = new();
        MockCommand command = new(runtime, chain.Factory);
        command.TestSaveCrawlScopeManager(chain.ScopeManager);
        Assert.Equal(1, chain.ScopeManager.SaveAllCallCount);
    }
}
