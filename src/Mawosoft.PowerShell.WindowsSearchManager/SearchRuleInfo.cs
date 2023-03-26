// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// POCO for CSearchScopeRule
/// </summary>
public sealed class SearchRuleInfo : ICloneable
{
    // TODO Un-nest enums?
    public enum SearchRuleType { Exclude, Include }
    public enum SearchRuleSet { User, Default }
    /// <summary>
    /// _FOLLOW_FLAGS is defined in searchapi.h, but missing from Interop.SearchAPI.
    /// </summary>
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Definition from searchapi.h")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Definition from searchapi.h")]
    [Flags]
    public enum _FOLLOW_FLAGS { FF_INDEXCOMPLEXURLS = 1, FF_SUPPRESSINDEXING = 2 }

    public string? Path { get; set; }
    public SearchRuleType RuleType { get; set; }
    public SearchRuleSet RuleSet { get; set; }
    public _FOLLOW_FLAGS FollowFlags { get; set; }
    public bool OverrideChildren { get; set; }

    public SearchRuleInfo() => FollowFlags = _FOLLOW_FLAGS.FF_INDEXCOMPLEXURLS;

    internal SearchRuleInfo(ISearchScopeRule searchScopeRule)
    {
        if (searchScopeRule == null) throw new ArgumentNullException(nameof(searchScopeRule));

        Path = searchScopeRule.PatternOrURL;
        RuleType = searchScopeRule.IsIncluded == 0 ? SearchRuleType.Exclude : SearchRuleType.Include;
        RuleSet = searchScopeRule.IsDefault == 0 ? SearchRuleSet.User : SearchRuleSet.Default;
        // searchScopeRule.FollowFlags is not implemented here. FF_INDEXCOMPLEXURLS almost aways will be correct.
        // However, when adding a rule, the flags can be passed to ICrawlScopeManager.AddXxxScopeRule().
        // TODO We could look-up the registry keys 'Suppress' (and maybe 'NoContent'?) for the rule.
        FollowFlags = _FOLLOW_FLAGS.FF_INDEXCOMPLEXURLS;
    }

    public object Clone() => MemberwiseClone();
}
