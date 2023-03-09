// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class MockCrawlScopeManager : ISearchCrawlScopeManager // TODO
{
    public virtual void AddDefaultScopeRule(string pszUrl, int fInclude, uint fFollowFlags) => throw new NotImplementedException();
    public virtual void AddRoot(CSearchRoot pSearchRoot) => throw new NotImplementedException();
    public virtual void RemoveRoot(string pszUrl) => throw new NotImplementedException();
    public virtual IEnumSearchRoots EnumerateRoots() => throw new NotImplementedException();
    public virtual void AddHierarchicalScope(string pszUrl, int fInclude, int fDefault, int fOverrideChildren) => throw new NotImplementedException();
    public virtual void AddUserScopeRule(string pszUrl, int fInclude, int fOverrideChildren, uint fFollowFlags) => throw new NotImplementedException();
    public virtual void RemoveScopeRule(string pszRule) => throw new NotImplementedException();
    public virtual IEnumSearchScopeRules EnumerateScopeRules() => throw new NotImplementedException();
    public virtual int HasParentScopeRule(string pszUrl) => throw new NotImplementedException();
    public virtual int HasChildScopeRule(string pszUrl) => throw new NotImplementedException();
    public virtual int IncludedInCrawlScope(string pszUrl) => throw new NotImplementedException();
    public virtual void IncludedInCrawlScopeEx(string pszUrl, out int pfIsIncluded, out CLUSION_REASON pReason) => throw new NotImplementedException();
    public virtual void RevertToDefaultScopes() => throw new NotImplementedException();
    public virtual void SaveAll() => throw new NotImplementedException();
    public virtual int GetParentScopeVersionId(string pszUrl) => throw new NotImplementedException();
    public virtual void RemoveDefaultScopeRule(string pszUrl) => throw new NotImplementedException();
}
