// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class MockEnumSearchRoots : MockInterfaceBase, IEnumSearchRoots
{
    private int _index;
    internal List<CSearchRoot> Items;
    internal MockEnumSearchRoots(IList<CSearchRoot> items) => Items = new(items);
    internal void ResetInternal() => _index = 0;

    public void Next(uint celt, out CSearchRoot rgelt, ref uint pceltFetched)
    {
        if (celt != 1) throw new NotImplementedException();
        rgelt = null!;
        pceltFetched = 0;
        if (_index < Items.Count)
        {
            rgelt = Items[_index++];
            pceltFetched++;
        }
        RecordRead(celt, rgelt, pceltFetched);
        TailCall();
    }

    // Unused IEnumSearchRoots members.

    [ExcludeFromCodeCoverage] public void Skip(uint celt) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public void Reset() => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public IEnumSearchRoots Clone() => throw new NotImplementedException();
}
