// Copyright (c) Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchRuleInfoTests
{
    [Fact]
    public void Ctor_Defaults()
    {
        SearchRuleInfo info = new();
        Assert.Equal(4, info.GetType().GetProperties().Length);
        Assert.Null(info.Path);
        Assert.Equal(SearchRuleInfo.SearchRuleType.Exclude, info.RuleType);
        Assert.Equal(SearchRuleInfo.SearchRuleSet.User, info.RuleSet);
        Assert.Equal(SearchRuleInfo._FOLLOW_FLAGS.FF_INDEXCOMPLEXURLS, info.FollowFlags);
    }

    [Fact]
    public void Ctor_NullArgument_Throws()
    {
        Assert.Throws<ArgumentNullException>("searchScopeRule", () => new SearchRuleInfo(null!));
    }

    [Fact]
    public void Ctor_ISearchScopeRule_Succeeds()
    {
        MockSearchScopeRule mock = new()
        {
            PatternOrURL = @"c:\foo\bar",
            IsIncluded = 1,
            IsDefault = 1
        };
        SearchRuleInfo info = new(mock);
        Assert.Equal(mock, info, SearchRuleInfoToMockComparer.Instance);
    }

    [Fact]
    public void Clone_Succeeds()
    {
        SearchRuleInfo info = new()
        {
            Path = @"c:\foo\bar",
            RuleType = SearchRuleInfo.SearchRuleType.Include,
            RuleSet = SearchRuleInfo.SearchRuleSet.Default,
            FollowFlags = SearchRuleInfo._FOLLOW_FLAGS.FF_SUPPRESSINDEXING
        };
        SearchRuleInfo clone = (SearchRuleInfo)info.Clone();
        Assert.Equivalent(info, clone, strict: true);
    }
}
