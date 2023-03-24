// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class MockInterfaceChain
{
    public static List<string> InterfaceNames { get; } = new()
    {
        nameof(ISearchManagerFactory),
        nameof(ISearchManager),
        nameof(ISearchCatalogManager),
        nameof(ISearchCrawlScopeManager),
    };

    internal MockSearchManagerFactory Factory { get; private set; }
    public MockSearchManager SearchManager { get; private set; }
    public MockCatalogManager CatalogManager { get; private set; }
    public MockCrawlScopeManager ScopeManager { get; private set; }

    public string CatalogName { get => CatalogManager.NameInternal; }

    public ExceptionParam ExceptionParam { get; private set; }

    public int ExceptionIndex { get; private set; } = -1;
    public int NullReferenceIndex { get; private set; } = -1;

    public MockInterfaceChain()
    {
        ScopeManager = new();
        CatalogManager = new(ScopeManager);
        SearchManager = new MockSearchManager2(CatalogManager);
        Factory = new(SearchManager);
    }

    public static int Count => 4;

    public MockInterfaceBase this[int index] => index switch
    {
        0 => Factory,
        1 => SearchManager,
        2 => CatalogManager,
        3 => ScopeManager,
        _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Expected: 0 <= index <= 3")
    };

    public override string? ToString() => ExceptionParam.Exception == null
            ? nameof(MockInterfaceChain)
            : $"{nameof(MockInterfaceChain)} with {ExceptionParam} for {InterfaceNames[Math.Max(NullReferenceIndex, ExceptionIndex)]}";

    public MockInterfaceChain WithSearchManager(MockSearchManager searchManager)
    {
        if (searchManager == null) throw new ArgumentNullException(nameof(searchManager));
        searchManager.ChildInterface = CatalogManager;
        SearchManager = searchManager;
        Factory.ChildInterface = SearchManager;
        return this;
    }

    public MockInterfaceChain WithCatalogManager(MockCatalogManager catalogManager)
    {
        if (catalogManager == null) throw new ArgumentNullException(nameof(catalogManager));
        catalogManager.ChildInterface = ScopeManager;
        CatalogManager = catalogManager;
        SearchManager.ChildInterface = CatalogManager;
        return this;
    }

    public MockInterfaceChain WithScopeManager(MockCrawlScopeManager scopeManager)
    {
        ScopeManager = scopeManager;
        CatalogManager.ChildInterface = ScopeManager;
        return this;
    }

    public MockInterfaceChain WithNullReference(int index)
    {
        if (ExceptionParam.Exception != null)
        {
            throw new InvalidOperationException("NullReference or Exception already initialized");
        }
        if (index == 0)
        {
            Factory = null!;
            ExceptionParam = new(new NullReferenceException());
        }
        else
        {
            this[index - 1].ChildInterface = null;
            ExceptionParam = new(new COMException(null, 0));
        }
        NullReferenceIndex = index;
        return this;
    }

    public MockInterfaceChain WithException(int index, ExceptionParam exceptionParam)
    {
        if (ExceptionParam.Exception != null)
        {
            throw new InvalidOperationException("NullReference or Exception already initialized");
        }
        this[index - 1].ChildInterface = exceptionParam.Exception;
        ExceptionParam = exceptionParam;
        ExceptionIndex = index;
        return this;
    }

    public void EnableRecording(bool enable)
    {
        bool disabled = !enable;
        if (Factory != null) Factory.RecordingDisabled = disabled;
        for (int i = 1; i < Count; i++) this[i].RecordingDisabled = disabled;
        SearchManager.CatalogManagers.ForEach(c => c.RecordingDisabled = disabled);
    }

    public bool HasRecordings()
    {
        if (Factory != null && Factory.RecordedCalls.Count > 0) return true;
        for (int i = 1; i < Count; i++) if (this[i].RecordedCalls.Count > 0) return true;
        return SearchManager.CatalogManagers.Find(c => c.RecordedCalls.Count > 0) != null;
    }
}
