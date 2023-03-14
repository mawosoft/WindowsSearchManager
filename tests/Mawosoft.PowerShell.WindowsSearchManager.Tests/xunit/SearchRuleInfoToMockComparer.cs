// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchRuleInfoToMockComparer : IEqualityComparer, IEqualityComparer<object>
{
    public static readonly SearchRuleInfoToMockComparer Instance = new();

    private SearchRuleInfoToMockComparer() { }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0019:Use pattern matching", Justification = "Does not work for this use case.")]
    public new bool Equals(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;
        SearchRuleInfo? info;
        MockSearchScopeRule? mock;
        if ((info = x as SearchRuleInfo) == null)
        {
            if ((info = y as SearchRuleInfo) == null) return x.Equals(y);
            mock = x as MockSearchScopeRule;
        }
        else
        {
            mock = y as MockSearchScopeRule;
        }
        if (mock == null) return x.Equals(y);

        if (mock.PatternOrURL != info.Path) return false;
        if ((mock.IsIncluded == 0 ? SearchRuleInfo.SearchRuleType.Exclude : SearchRuleInfo.SearchRuleType.Include) != info.RuleType) return false;
        if ((mock.IsDefault == 0 ? SearchRuleInfo.SearchRuleSet.User : SearchRuleInfo.SearchRuleSet.Default) != info.RuleSet) return false;
        if (SearchRuleInfo._FOLLOW_FLAGS.FF_INDEXCOMPLEXURLS != info.FollowFlags) return false;
        if (info.OverrideChildren) return false;
        return true;
    }

    public int GetHashCode(object obj) => obj?.GetHashCode() ?? 0;
}
