// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class MockEnumSearchScopeRules : MockInterfaceBase, IEnumSearchScopeRules
{
    private int _index;
    internal List<CSearchScopeRule> Items;
    internal MockEnumSearchScopeRules(IList<CSearchScopeRule> items) => Items = new(items);
    internal void ResetInternal() => _index = 0;

    public void Next(uint celt, out CSearchScopeRule pprgelt, ref uint pceltFetched)
    {
        if (celt != 1) throw new NotImplementedException();
        pprgelt = null!;
        pceltFetched = 0;
        if (_index < Items.Count)
        {
            pprgelt = Items[_index++];
            pceltFetched++;
        }
        Record(celt, pprgelt, pceltFetched);
        TailCall();
    }

    // Unused IEnumSearchScopeRules members.

    [ExcludeFromCodeCoverage] public void Skip(uint celt) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public void Reset() => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public IEnumSearchScopeRules Clone() => throw new NotImplementedException();
}
