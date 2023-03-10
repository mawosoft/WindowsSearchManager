// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class MockInterfaceChain
{
    internal MockSearchManagerFactory Factory { get; private set; }
    public MockSearchManager SearchManager { get; }
    public MockCatalogManager CatalogManager { get; }
    public MockCrawlScopeManager ScopeManager { get; }
    public string CatalogName { get; }
    public Exception? Exception { get; private set; }
    public Type? ExceptionType { get; private set; }
    public bool ShouldHaveErrorRecord { get; private set; }
    public int NullReferenceIndex { get; private set; } = -1;
    public int ExceptionIndex { get; private set; } = -1;

    public MockInterfaceChain() : this(false, null) { }
    public MockInterfaceChain(bool withoutISearchManager2) : this(withoutISearchManager2, null) { }
    public MockInterfaceChain(bool withoutISearchManager2, string? catalogName)
    {
        CatalogName = catalogName ?? SearchApiCommandBase.DefaultCatalogName;
        ScopeManager = new();
        CatalogManager = new(CatalogName, ScopeManager);
        SearchManager = withoutISearchManager2 ? new MockSearchManager(CatalogManager) : new MockSearchManager2(CatalogManager);
        Factory = new(SearchManager);
    }

    public MockInterfaceChain WithNullReference(int chainIndex)
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

    public MockInterfaceChain WithException(int chainIndex, Exception exception, bool shouldHaveErrorRecord)
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
            ErrorRecord rec = Assert.Single(runtime.Errors);
            Assert.Same(ex, rec.Exception);
        }
        else
        {
            Assert.Empty(runtime.Errors);
        }
    }
}
