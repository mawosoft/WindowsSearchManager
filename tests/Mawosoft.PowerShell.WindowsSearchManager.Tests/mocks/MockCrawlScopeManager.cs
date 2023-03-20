﻿// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class MockCrawlScopeManager : MockInterfaceBase, ISearchCrawlScopeManager
{
    // TODO we need roots and rules for enum and queries
    public virtual void AddDefaultScopeRule(string pszUrl, int fInclude, uint fFollowFlags) => Record(pszUrl, fInclude, fFollowFlags);
    public virtual void AddRoot(CSearchRoot pSearchRoot) => throw new NotImplementedException(); // TODO Record tostring
    public virtual void RemoveRoot(string pszUrl) => Record(pszUrl);
    public virtual IEnumSearchRoots EnumerateRoots() => throw new NotImplementedException();
    public virtual void AddHierarchicalScope(string pszUrl, int fInclude, int fDefault, int fOverrideChildren) => Record(pszUrl, fInclude, fDefault, fOverrideChildren);
    public virtual void AddUserScopeRule(string pszUrl, int fInclude, int fOverrideChildren, uint fFollowFlags) => Record(pszUrl, fInclude, fOverrideChildren, fFollowFlags);
    public virtual void RemoveScopeRule(string pszRule) => Record(pszRule);
    public virtual IEnumSearchScopeRules EnumerateScopeRules() => throw new NotImplementedException();
    public virtual int HasParentScopeRule(string pszUrl) => throw new NotImplementedException();
    public virtual int HasChildScopeRule(string pszUrl) => throw new NotImplementedException();
    public virtual int IncludedInCrawlScope(string pszUrl) => throw new NotImplementedException();
    public virtual void IncludedInCrawlScopeEx(string pszUrl, out int pfIsIncluded, out CLUSION_REASON pReason) => throw new NotImplementedException();
    public virtual void RevertToDefaultScopes() => Record();
    public virtual void SaveAll() => Record();
    public virtual int GetParentScopeVersionId(string pszUrl) => throw new NotImplementedException();
    public virtual void RemoveDefaultScopeRule(string pszUrl) => Record(pszUrl);
}
