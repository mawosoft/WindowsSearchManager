// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class MockInterfaceChain
{
    internal MockSearchManagerFactory Factory { get; private set; }
    public MockSearchManager SearchManager { get; private set; }
    public MockCatalogManager CatalogManager { get; private set; }
    public MockCrawlScopeManager ScopeManager { get; private set; }
    public string CatalogName { get => CatalogManager.Name; }
    public Exception? Exception { get; private set; }
    public Type? ExceptionType { get; private set; }
    public bool ShouldHaveErrorRecord { get; private set; }
    public int NullReferenceIndex { get; private set; } = -1;
    public int ExceptionIndex { get; private set; } = -1;

    public MockInterfaceChain()
    {
        ScopeManager = new();
        CatalogManager = new(ScopeManager);
        SearchManager = new MockSearchManager2(CatalogManager);
        Factory = new(SearchManager);
    }

    public MockInterfaceChain WithSearchManager(MockSearchManager searchManager)
    {
        searchManager.CatalogManager = CatalogManager;
        SearchManager = searchManager;
        Factory.SearchManager = SearchManager;
        return this;
    }

    public MockInterfaceChain WithCatalogManager(MockCatalogManager catalogManager)
    {
        catalogManager.ScopeManager = ScopeManager;
        CatalogManager = catalogManager;
        SearchManager.CatalogManager = CatalogManager;
        return this;
    }

    public MockInterfaceChain WithScopeManager(MockCrawlScopeManager scopeManager)
    {
        ScopeManager = scopeManager;
        CatalogManager.ScopeManager = ScopeManager;
        return this;
    }

    public MockInterfaceChain WithNullReference(int chainIndex)
    {
        if (ExceptionType != null)
        {
            throw new InvalidOperationException("NullReference or Exception already initialized");
        }
        switch (chainIndex)
        {
            case 0: Factory = null!; ExceptionType = typeof(NullReferenceException); break;
            case 1: Factory.SearchManager = null!; ExceptionType = typeof(COMException); break;
            case 2: SearchManager.CatalogManager = null; ExceptionType = typeof(COMException); break;
            case 3: CatalogManager.ScopeManager = null; ExceptionType = typeof(COMException); break;
            default:
                throw new ArgumentOutOfRangeException(nameof(chainIndex), chainIndex,
                                                      "Expected: 0 <= value <= 3");
        }
        NullReferenceIndex = chainIndex;
        return this;
    }

    public MockInterfaceChain WithException(int chainIndex, ExceptionParam exceptionParam)
    {
        if (ExceptionType != null)
        {
            throw new InvalidOperationException("NullReference or Exception already initialized");
        }
        if (exceptionParam == null)
        {
            throw new ArgumentNullException(nameof(exceptionParam));
        }
        (Exception exception, bool isCustom) = exceptionParam.Value;

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
        ExceptionType = exception.GetType();
        ShouldHaveErrorRecord = isCustom;
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

    public static Type? IndexToInterface(int chainIndex) => chainIndex switch
    {
        0 => typeof(ISearchManagerFactory),
        1 => typeof(ISearchManager),
        2 => typeof(ISearchCatalogManager),
        3 => typeof(ISearchCrawlScopeManager),
        _ => null
    };

    public override string ToString()
    {
        List<string> nondefaultMocks = new();
        Type t;
        if (Factory != null && (t = Factory.GetType()) != typeof(MockSearchManagerFactory)) nondefaultMocks.Add(t.Name);
        if ((t = SearchManager.GetType()) != typeof(MockSearchManager2)) nondefaultMocks.Add(t.Name);
        if ((t = CatalogManager.GetType()) != typeof(MockCatalogManager)) nondefaultMocks.Add(t.Name);
        if ((t = ScopeManager.GetType()) != typeof(MockCrawlScopeManager)) nondefaultMocks.Add(t.Name);
        string s = GetType().Name;
        if (nondefaultMocks.Count > 0) s += $" [{string.Join(",", nondefaultMocks)}]";
        if (ExceptionType != null)
        {
            s += $" {ExceptionType.Name}";
            if (Exception != null) s += $"(0x{Exception.HResult:X8})";
            if (ShouldHaveErrorRecord) s += " [ErrorRecord]";
            string? f = IndexToInterface(Math.Max(NullReferenceIndex, ExceptionIndex))?.Name;
            if (f != null) s += $" for {f}";
        }
        return s;
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
            Assert.Contains(IndexToInterface(NullReferenceIndex)!.Name, ex.Message);
        }
        if (ShouldHaveErrorRecord)
        {
            ErrorRecord rec = Assert.Single(runtime.Errors);
            Assert.Same(ex, rec.Exception);
        }
        else
        {
            Assert.Empty(runtime.Errors);
        }
    }
}
