// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class MockInterfaceChain
{
    internal static List<string> InterfaceNames { get; } = new()
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

    public string CatalogName => CatalogManager.NameInternal;

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

    public override string? ToString() => ExceptionParam.Exception is null
            ? nameof(MockInterfaceChain)
            : $"{nameof(MockInterfaceChain)} with {ExceptionParam} for {InterfaceNames[Math.Max(NullReferenceIndex, ExceptionIndex)]}";

    public MockInterfaceChain WithSearchManager(MockSearchManager searchManager)
    {
        if (searchManager is null) throw new ArgumentNullException(nameof(searchManager));
        searchManager.ChildInterface = CatalogManager;
        SearchManager = searchManager;
        Factory.ChildInterface = SearchManager;
        return this;
    }

    public MockInterfaceChain WithCatalogManager(MockCatalogManager catalogManager)
    {
        if (catalogManager is null) throw new ArgumentNullException(nameof(catalogManager));
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
        if (ExceptionParam.Exception is not null)
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
        if (ExceptionParam.Exception is not null)
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
        if (Factory is not null) Factory.RecordingDisabled = disabled;
        for (int i = 1; i < Count; i++) this[i].RecordingDisabled = disabled;
        SearchManager.CatalogManagers.ForEach(c => c.RecordingDisabled = disabled);
    }

    public bool HasRecordings()
    {
        if (Factory is not null && Factory.HasRecordings) return true;
        for (int i = 1; i < Count; i++) if (this[i].HasRecordings) return true;
        return SearchManager.CatalogManagers.Find(c => c.HasRecordings) is not null;
    }

    public bool HasWriteRecordings()
    {
        if (Factory is not null && Factory.HasWriteRecordings) return true;
        for (int i = 1; i < Count; i++) if (this[i].HasWriteRecordings) return true;
        return SearchManager.CatalogManagers.Find(c => c.HasWriteRecordings) is not null;
    }
}
