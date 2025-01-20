// Copyright (c) Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Contains information about a search rule in a search catalog.
/// </summary>
public sealed class SearchRuleInfo : ICloneable
{
    /// <summary>
    /// Specifies the type of a search rule.
    /// </summary>
    public enum SearchRuleType
    {
        /// <summary>
        /// The rule excludes items from indexing.
        /// </summary>
        Exclude,

        /// <summary>
        /// The rule includes items into the index.
        /// </summary>
        Include
    }

    /// <summary>
    /// Specifies the rule set a search rule belongs to.
    /// </summary>
    public enum SearchRuleSet
    {
        /// <summary>
        /// The search rule belongs to the user scope.
        /// </summary>
        User,

        /// <summary>
        /// The search rule belongs to the default scope.
        /// </summary>
        Default
    }

    /// <summary>
    /// Specifies whether to follow complex URLs and whether a URL is to be indexed or just followed.
    /// </summary>
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Definition from searchapi.h")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Definition from searchapi.h")]
    [Flags]
    public enum _FOLLOW_FLAGS
    {
        /// <summary>
        /// Complex URLs should be indexed.
        /// </summary>
        FF_INDEXCOMPLEXURLS = 1,
        /// <summary>
        /// Follow but do not index this URL.
        /// </summary>
        FF_SUPPRESSINDEXING = 2
    }

    /// <value>The URL, path, or pattern of this search rule.</value>
    public string? Path { get; set; }

    /// <value>One of the enumeration values indicating whether this is an inclusion or exclusion rule.</value>
    public SearchRuleType RuleType { get; set; }

    /// <value>One of the enumeration values indicating whether this is a user scope or default scope rule.</value>
    public SearchRuleSet RuleSet { get; set; }

    /// <value>
    /// A combination of enumeration values to indicate whether to follow complex URLs and whether
    /// a URL is to be indexed or just followed.
    /// </value>
    public _FOLLOW_FLAGS FollowFlags { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchRuleInfo"/> class.
    /// </summary>
    public SearchRuleInfo() => FollowFlags = _FOLLOW_FLAGS.FF_INDEXCOMPLEXURLS;

    internal SearchRuleInfo(ISearchScopeRule searchScopeRule)
    {
        if (searchScopeRule is null) throw new ArgumentNullException(nameof(searchScopeRule));

        Path = searchScopeRule.PatternOrURL;
        RuleType = searchScopeRule.IsIncluded == 0 ? SearchRuleType.Exclude : SearchRuleType.Include;
        RuleSet = searchScopeRule.IsDefault == 0 ? SearchRuleSet.User : SearchRuleSet.Default;
        // searchScopeRule.FollowFlags is not implemented here. FF_INDEXCOMPLEXURLS almost aways will be correct.
        // However, when adding a rule, the flags can be passed to ICrawlScopeManager.AddXxxScopeRule().
        // TODO We could look-up the registry keys 'Suppress' (and maybe 'NoContent'?) for the rule.
        FollowFlags = _FOLLOW_FLAGS.FF_INDEXCOMPLEXURLS;
    }

    /// <summary>
    /// Creates a shallow copy of the <see cref="SearchRuleInfo"/> instance.
    /// </summary>
    /// <returns>A shallow copy of the <see cref="SearchRuleInfo"/> instance.</returns>
    public object Clone() => MemberwiseClone();
}
