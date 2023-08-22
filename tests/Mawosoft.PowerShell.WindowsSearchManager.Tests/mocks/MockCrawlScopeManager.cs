// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

#pragma warning disable CA1054 // URI-like parameters should not be strings

[SuppressMessage("Design", "CA1033:Interface methods should be callable by child types",
    Justification = "Multiple interfaces with same method names.")]
public class MockCrawlScopeManager : MockInterfaceBase, ISearchCrawlScopeManager, IEnumSearchRoots, IEnumSearchScopeRules
{
    internal List<object?>? Roots { get; set; }
    internal int RootsEnumIndex { get; set; }
    internal List<object?>? Rules { get; set; }
    internal int RulesEnumIndex { get; set; }
    internal List<object?>? TestRuleInfos { get; set; }
    internal int TestRuleInfosEnumIndex { get; set; }

    // ISearchCrawlScopeManager

    public virtual void AddDefaultScopeRule(string pszUrl, int fInclude, uint fFollowFlags)
    {
        RecordWrite(pszUrl, fInclude, fFollowFlags);
        TailCall();
    }

    public virtual void AddRoot(CSearchRoot pSearchRoot)
    {
        RecordWrite(pSearchRoot);
        TailCall();
    }

    public virtual void RemoveRoot(string pszUrl)
    {
        RecordWrite(pszUrl);
        TailCall();
    }

    public virtual IEnumSearchRoots EnumerateRoots()
    {
        RecordRead();
        RootsEnumIndex = 0;
        return Roots?.Count > 0 ? this : null!;
    }

    public virtual void AddUserScopeRule(string pszUrl, int fInclude, int fOverrideChildren, uint fFollowFlags)
    {
        RecordWrite(pszUrl, fInclude, fOverrideChildren, fFollowFlags);
        TailCall();
    }

    public virtual void RemoveScopeRule(string pszRule)
    {
        RecordWrite(pszRule);
        TailCall();
    }
    public virtual IEnumSearchScopeRules EnumerateScopeRules()
    {
        RecordRead();
        RulesEnumIndex = 0;
        return Rules?.Count > 0 ? this : null!;
    }

    public virtual int HasParentScopeRule(string pszUrl)
    {
        RecordRead(pszUrl);
        return GetTestRuleInfo()?.HasParentScope is true ? 1 : 0;
    }

    public virtual int HasChildScopeRule(string pszUrl)
    {
        RecordRead(pszUrl);
        return GetTestRuleInfo()?.HasChildScope is true ? 1 : 0;
    }

    public virtual int IncludedInCrawlScope(string pszUrl)
    {
        RecordRead(pszUrl);
        return GetTestRuleInfo(forceMoveNext: true)?.IsIncluded is true ? 1 : 0;
    }

    public virtual void IncludedInCrawlScopeEx(string pszUrl, out int pfIsIncluded, out CLUSION_REASON pReason)
    {
        RecordRead(pszUrl);
        pfIsIncluded = default;
        pReason = default;
        TestSearchRuleInfo? info = GetTestRuleInfo(forceMoveNext: true);
        if (info is not null)
        {
            pfIsIncluded = info.IsIncluded ? 1 : 0;
            pReason = info.Reason;
        }
    }

    public virtual void RevertToDefaultScopes()
    {
        RecordWrite();
        TailCall();
    }
    public virtual void SaveAll()
    {
        RecordWrite();
        TailCall();
    }

    public virtual int GetParentScopeVersionId(string pszUrl)
    {
        RecordRead(pszUrl);
        return GetTestRuleInfo()?.ParentScopeVersiondId ?? 0;
    }

    public virtual void RemoveDefaultScopeRule(string pszUrl)
    {
        RecordWrite(pszUrl);
        TailCall();
    }

    private TestSearchRuleInfo? GetTestRuleInfo(bool forceMoveNext = false)
    {
        if (!(TestRuleInfos?.Count > TestRuleInfosEnumIndex)) return null;
        object? value = TestRuleInfos[TestRuleInfosEnumIndex];
        if (forceMoveNext
            || RecordedCallInfos.Find(c => c.MethodName == nameof(IncludedInCrawlScopeEx)) is null)
        {
            TestRuleInfosEnumIndex++;
        }
        if (value is Exception ex) throw ex;
        return value as TestSearchRuleInfo;
    }

    // IEnumSearchRoots

    public void Next(uint celt, out CSearchRoot rgelt, ref uint pceltFetched)
    {
        RecordRead(celt, nameof(CSearchRoot));
        if (celt != 1) throw new NotImplementedException();
        rgelt = null!;
        pceltFetched = 0;
        if (Roots?.Count > RootsEnumIndex)
        {
            object? value = Roots[RootsEnumIndex++];
            if (value is Exception ex) throw ex;
            rgelt = (value as CSearchRoot)!;
            pceltFetched++;
        }
    }

    // IEnumSearchScopeRules

    public void Next(uint celt, out CSearchScopeRule pprgelt, ref uint pceltFetched)
    {
        RecordRead(celt, nameof(CSearchScopeRule));
        if (celt != 1) throw new NotImplementedException();
        pprgelt = null!;
        pceltFetched = 0;
        if (Rules?.Count > RulesEnumIndex)
        {
            object? value = Rules[RulesEnumIndex++];
            if (value is Exception ex) throw ex;
            pprgelt = (value as CSearchScopeRule)!;
            pceltFetched++;
        }
    }

    // Unused ISearchCrawlScopeManager members.

    [ExcludeFromCodeCoverage] public virtual void AddHierarchicalScope(string pszUrl, int fInclude, int fDefault, int fOverrideChildren) => throw new NotImplementedException();

    // Unused IEnumSearchRoots and IEnumSearchScopeRules members.

    [ExcludeFromCodeCoverage] public void Skip(uint celt) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public void Reset() => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] IEnumSearchRoots IEnumSearchRoots.Clone() => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] IEnumSearchScopeRules IEnumSearchScopeRules.Clone() => throw new NotImplementedException();
}
