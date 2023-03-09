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

    internal class InterfaceChain
    {
        public MockSearchManagerFactory Factory { get; private set; }
        public MockSearchManager SearchManager { get; }
        public MockCatalogManager CatalogManager { get; }
        public MockCrawlScopeManager ScopeManager { get; }
        public string CatalogName { get; }
        public Exception? Exception { get; private set; }
        public Type? ExceptionType { get; private set; }
        public bool ShouldHaveErrorRecord { get; private set; }
        public int NullReferenceIndex { get; private set; } = -1;
        public int ExceptionIndex { get; private set; } = -1;

        public InterfaceChain() : this(false, null) { }
        public InterfaceChain(bool withoutISearchManager2) : this(withoutISearchManager2, null) { }
        public InterfaceChain(bool withoutISearchManager2, string? catalogName)
        {
            CatalogName = catalogName ?? SearchApiCommandBase.DefaultCatalogName;
            ScopeManager = new();
            CatalogManager = new(CatalogName, ScopeManager);
            SearchManager = withoutISearchManager2 ? new MockSearchManager(CatalogManager) : new MockSearchManager2(CatalogManager);
            Factory = new(SearchManager);
        }

        public InterfaceChain WithNullReference(int chainIndex)
        {
            if (NullReferenceIndex >= 0 || ExceptionIndex >= 0)
            {
                throw new InvalidOperationException("NullReference or Exception already initialized");
            }
            ExceptionType = typeof(COMException);
            switch (chainIndex)
            {
                case 0: Factory = null!; ExceptionType = typeof(NullReferenceException); break;
                case 1: Factory.SearchManager = null; break;
                case 2: SearchManager.CatalogManager = null; break;
                case 3: CatalogManager.ScopeManager = null; break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(chainIndex), chainIndex,
                                                          "Expected: 0 <= value <= 3");
            }
            NullReferenceIndex = chainIndex;
            return this;
        }

        public InterfaceChain WithException(int chainIndex, Exception exception, bool shouldHaveErrorRecord)
        {
            if (NullReferenceIndex >= 0 || ExceptionIndex >= 0)
            {
                throw new InvalidOperationException("NullReference or Exception already initialized");
            }
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            switch (chainIndex)
            {
                case 1: Factory.SearchManagerException = exception; break;
                case 2: SearchManager.CatalogManagerException = exception; break;
                case 3: CatalogManager.ScopeManagerException = exception; break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(chainIndex), chainIndex,
                                                               "Expected: 1 <= value <= 3");
            }
            Exception = exception;
            ShouldHaveErrorRecord = shouldHaveErrorRecord;
            ExceptionIndex = chainIndex;
            return this;
        }

        public static int TypeToIndex(Type interfaceType)
        {
            if (typeof(ISearchManagerFactory).IsAssignableFrom(interfaceType)) return 0;
            else if (typeof(ISearchManager).IsAssignableFrom(interfaceType)) return 1;
            else if (typeof(ISearchCatalogManager).IsAssignableFrom(interfaceType)) return 2;
            else if (typeof(ISearchCrawlScopeManager).IsAssignableFrom(interfaceType)) return 3;
            return -1;
        }

        public static Type? IndexToType(int chainIndex) => chainIndex switch
        {
            0 => typeof(ISearchManagerFactory),
            1 => typeof(ISearchManager),
            2 => typeof(ISearchCatalogManager),
            3 => typeof(ISearchCrawlScopeManager),
            _ => null
        };

        public override string ToString()
        {
            int index = Math.Max(NullReferenceIndex, ExceptionIndex);
            if (index < 0)
            {
                return SearchManager is ISearchManager2 ? "default" : "default w/o ISearchManager2";
            }
            else
            {
                return $"{IndexToType(index)?.Name} {ExceptionType?.Name ?? Exception?.GetType().Name}{(ShouldHaveErrorRecord ? " (wrapped)" : "")}";
            }
        }

        public void AssertThrows(Action action, MockCommandRuntime runtime)
        {
            Type t = ExceptionType ?? Exception?.GetType() ?? throw new InvalidOperationException("InterfaceChain is not set-up to throw.");
            Exception ex = Assert.Throws(t, action);
            if (Exception != null)
            {
                Assert.Same(Exception, ex);
            }
            else if (NullReferenceIndex >= 0 && ex is COMException comException && comException.HResult == 0)
            {
                Assert.Contains(IndexToType(NullReferenceIndex)!.Name, ex.Message);
            }
            if (ShouldHaveErrorRecord)
            {
                Assert.Single(runtime.Errors);
            }
            else
            {
                Assert.Empty(runtime.Errors);
            }

        }
    }

    public static IEnumerable<object[]> InterfaceChainCollectionForFailures(Type interfaceType, bool noParents)
    {
        int chainIndex = InterfaceChain.TypeToIndex(interfaceType);
        if (chainIndex < 0)
        {
            throw new ArgumentException(null, nameof(interfaceType));
        }

        for (int i = noParents ? chainIndex : 0; i <= chainIndex; i++)
        {
            yield return new object[] { new InterfaceChain().WithNullReference(i) };
        }

        for (int i = noParents ? chainIndex : 1; i <= chainIndex; i++)
        {
            yield return new object[] { new InterfaceChain().WithException(i, new Exception(), false) };
            yield return new object[] { new InterfaceChain().WithException(i, new COMException(), false) };
            yield return new object[] { new InterfaceChain().WithException(i, new COMException(null, unchecked((int)0x80042103)), true) }; // custom COM message
        }
    }


    [Theory]
    [MemberData(nameof(InterfaceChainCollectionForFailures), typeof(ISearchManager), false)]
    internal void CreateSearchManager_HandlesFailures(InterfaceChain chain)
    {
        MockCommandRuntime runtime = new();
        MockCommand command = new(runtime, chain.Factory);
        chain.AssertThrows(() => command.TestCreateSearchManager(), runtime);
    }

    [Fact]
    public void CreateSearchManager_Succeeds()
    {
        InterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Same(chain.SearchManager, command.TestCreateSearchManager());
    }

    [Theory]
    [MemberData(nameof(InterfaceChainCollectionForFailures), typeof(ISearchManager), false)]
    internal void GetSearchManager2_HandlesFailures(InterfaceChain chain)
    {
        MockCommandRuntime runtime = new();
        MockCommand command = new(runtime, chain.Factory);
        chain.AssertThrows(() => command.TestGetSearchManager2(), runtime);
    }

    [Fact]
    internal void GetSearchManager2_MissingInterface_Throws()
    {
        MockCommandRuntime runtime = new();
        InterfaceChain chain = new(withoutISearchManager2: true);
        MockCommand command = new(runtime, chain.Factory);
        InvalidCastException ex = Assert.Throws<InvalidCastException>(command.TestGetSearchManager2);
        Assert.Contains(nameof(ISearchManager2), ex.Message);
    }

    [Fact]
    public void GetSearchManager2_Succeeds()
    {
        InterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Same(chain.SearchManager, command.TestGetSearchManager2());
    }

    [Fact]
    internal void GetSearchManager2_ISearchManager_NullArgument_Throws()
    {
        MockCommandRuntime runtime = new();
        InterfaceChain chain = new();
        MockCommand command = new(runtime, chain.Factory);
        Assert.Throws<ArgumentNullException>("searchManager", () => command.TestGetSearchManager2(null!));
    }

    [Fact]
    internal void GetSearchManager2_ISearchManager_MissingInterface_Throws()
    {
        MockCommandRuntime runtime = new();
        InterfaceChain chain = new(withoutISearchManager2: true);
        MockCommand command = new(runtime, chain.Factory);
        InvalidCastException ex = Assert.Throws<InvalidCastException>(() => command.TestGetSearchManager2(chain.SearchManager));
        Assert.Contains(nameof(ISearchManager2), ex.Message);
    }

    [Fact]
    public void GetSearchManager2_ISearchManager_Succeeds()
    {
        InterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Same(chain.SearchManager, command.TestGetSearchManager2(chain.SearchManager));
    }

    [Theory]
    [MemberData(nameof(InterfaceChainCollectionForFailures), typeof(ISearchCatalogManager), false)]
    internal void GetCatalogManager_HandlesFailures(InterfaceChain chain)
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
        InterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Throws<ArgumentException>(nameof(catalogName), () => command.TestGetCatalogManager(catalogName));
    }

    [Fact]
    public void GetCatalogManager_Succeeds()
    {
        InterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Same(chain.CatalogManager, command.TestGetCatalogManager(chain.CatalogName));
    }

    [Theory]
    [MemberData(nameof(InterfaceChainCollectionForFailures), typeof(ISearchCatalogManager), true)]
    internal void GetCatalogManager_ISearchManager_HandlesFailures(InterfaceChain chain)
    {
        MockCommandRuntime runtime = new();
        MockCommand command = new(runtime, chain.Factory);
        chain.AssertThrows(() => command.TestGetCatalogManager(chain.SearchManager, chain.CatalogName), runtime);
    }

    [Fact]
    internal void GetCatalogManager_ISearchManager_NullSearchManager_Throws()
    {
        MockCommandRuntime runtime = new();
        InterfaceChain chain = new();
        MockCommand command = new(runtime, chain.Factory);
        Assert.Throws<ArgumentNullException>("searchManager", () => command.TestGetCatalogManager(null!, chain.CatalogName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    internal void GetCatalogManager_ISearchManager_InvalidCatalogName_Throws(string catalogName)
    {
        InterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Throws<ArgumentException>(nameof(catalogName), () => command.TestGetCatalogManager(chain.SearchManager, catalogName));
    }

    [Fact]
    public void GetCatalogManager_ISearchManager_Succeeds()
    {
        InterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Same(chain.CatalogManager, command.TestGetCatalogManager(chain.SearchManager, chain.CatalogName));
    }

    [Theory]
    [MemberData(nameof(InterfaceChainCollectionForFailures), typeof(ISearchCrawlScopeManager), false)]
    internal void GetCrawlScopeManager_HandlesFailures(InterfaceChain chain)
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
        InterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Throws<ArgumentException>(nameof(catalogName), () => command.TestGetCrawlScopeManager(catalogName));
    }

    [Fact]
    public void GetCrawlScopeManager_Succeeds()
    {
        InterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Same(chain.ScopeManager, command.TestGetCrawlScopeManager(chain.CatalogName));
    }

    [Theory]
    [MemberData(nameof(InterfaceChainCollectionForFailures), typeof(ISearchCrawlScopeManager), true)]
    internal void GetCrawlScopeManager_ISearchCatalogManager_HandlesFailures(InterfaceChain chain)
    {
        MockCommandRuntime runtime = new();
        MockCommand command = new(runtime, chain.Factory);
        chain.AssertThrows(() => command.TestGetCrawlScopeManager(chain.CatalogManager), runtime);
    }

    [Fact]
    internal void GetCrawlScopeManager_ISearchCatalogManager_NullCatalogManager_Throws()
    {
        MockCommandRuntime runtime = new();
        InterfaceChain chain = new();
        MockCommand command = new(runtime, chain.Factory);
        Assert.Throws<ArgumentNullException>("catalogManager", () => command.TestGetCrawlScopeManager((ISearchCatalogManager)null!));
    }

    [Fact]
    public void GetCrawlScopeManager_ISearchCatalogManager_Succeeds()
    {
        InterfaceChain chain = new();
        MockCommand command = new(chain.Factory);
        Assert.Same(chain.ScopeManager, command.TestGetCrawlScopeManager(chain.CatalogManager));
    }

    // TODO SaveCrawlScopeManager Tests 
    [Theory]
    [InlineData(false)]
    public void SaveCrawlScopeManager_HandlesFailures(bool dummy)
    {
        _ = dummy;
    }

    [Fact]
    public void SaveCrawlScopeManager_NullArgument_Succeeds()
    {
    }

    [Fact]
    public void SaveCrawlScopeManager_Succeeds()
    {
    }
}
