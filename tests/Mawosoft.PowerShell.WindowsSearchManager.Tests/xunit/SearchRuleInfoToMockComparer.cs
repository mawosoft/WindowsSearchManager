// Copyright (c) 2023-2024 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchRuleInfoToMockComparer : IEqualityComparer, IEqualityComparer<object>
{
    public static readonly SearchRuleInfoToMockComparer Instance = new();

    private SearchRuleInfoToMockComparer() { }

    public new bool Equals(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        SearchRuleInfo? info;
        MockSearchScopeRule? mock;
        if ((info = x as SearchRuleInfo) is null)
        {
            if ((info = y as SearchRuleInfo) is null) return x.Equals(y);
            mock = x as MockSearchScopeRule;
        }
        else
        {
            mock = y as MockSearchScopeRule;
        }
        if (mock is null) return x.Equals(y);

        if (mock.PatternOrURL != info.Path) return false;
        if ((mock.IsIncluded == 0 ? SearchRuleInfo.SearchRuleType.Exclude : SearchRuleInfo.SearchRuleType.Include) != info.RuleType) return false;
        if ((mock.IsDefault == 0 ? SearchRuleInfo.SearchRuleSet.User : SearchRuleInfo.SearchRuleSet.Default) != info.RuleSet) return false;
        if (SearchRuleInfo._FOLLOW_FLAGS.FF_INDEXCOMPLEXURLS != info.FollowFlags) return false;
        return true;
    }

    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "Only used for Asser.Equal().")]
    [ExcludeFromCodeCoverage]
    public int GetHashCode(object obj) => throw new NotImplementedException();
}
