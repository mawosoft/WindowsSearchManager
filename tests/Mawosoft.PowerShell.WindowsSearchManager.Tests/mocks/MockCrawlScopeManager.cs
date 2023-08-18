// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

#pragma warning disable CA1054 // URI-like parameters should not be strings

public class MockCrawlScopeManager : MockInterfaceBase, ISearchCrawlScopeManager
{
    internal MockCrawlScopeManager() { }

    internal MockCrawlScopeManager(IList<CSearchRoot> roots)
    {
        if (roots.Count != 0) ChildInterface = new MockEnumSearchRoots(roots);
    }

    internal MockCrawlScopeManager(IList<CSearchScopeRule> rules)
    {
        if (rules.Count != 0) ChildInterface = new MockEnumSearchScopeRules(rules);
    }

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
        MockEnumSearchRoots? enumerator = GetChildInterface() as MockEnumSearchRoots;
        enumerator?.ResetInternal();
        return enumerator!;
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
        MockEnumSearchScopeRules? enumerator = GetChildInterface() as MockEnumSearchScopeRules;
        enumerator?.ResetInternal();
        return enumerator!;
    }

    public virtual int HasParentScopeRule(string pszUrl) => throw new NotImplementedException();
    public virtual int HasChildScopeRule(string pszUrl) => throw new NotImplementedException();
    public virtual int IncludedInCrawlScope(string pszUrl) => throw new NotImplementedException();
    public virtual void IncludedInCrawlScopeEx(string pszUrl, out int pfIsIncluded, out CLUSION_REASON pReason) => throw new NotImplementedException();
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

    public virtual int GetParentScopeVersionId(string pszUrl) => throw new NotImplementedException();
    public virtual void RemoveDefaultScopeRule(string pszUrl)
    {
        RecordWrite(pszUrl);
        TailCall();
    }

    // Unused ISearchCrawlScopeManager members.

    [ExcludeFromCodeCoverage] public virtual void AddHierarchicalScope(string pszUrl, int fInclude, int fDefault, int fOverrideChildren) => throw new NotImplementedException();


}
