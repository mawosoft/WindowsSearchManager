// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Returns a collection of objects representing the search rules of a catalog.
/// </summary>
[Cmdlet(VerbsCommon.Get, Nouns.SearchRule)]
[OutputType(typeof(SearchRuleInfo))]
public sealed class GetSearchRuleCommand : DefaultCatalogCommandBase
{
    protected override void EndProcessing()
    {
        CSearchManager manager = new();
        ISearchCatalogManager catalog = manager.GetCatalog(Catalog);
        ISearchCrawlScopeManager scope = catalog.GetCrawlScopeManager();
        IEnumSearchScopeRules rules = scope.EnumerateScopeRules();
        for (; ; )
        {
            uint fetched = 0;
            rules.Next(1, out CSearchScopeRule rule, ref fetched);
            if (fetched != 1 || rule == null) break;
            SearchRuleInfo info = new(rule);
            WriteObject(info);
        }
    }
}

/// <summary>
/// Adds one or more search rules to a catalog.
/// </summary>
[Cmdlet(VerbsCommon.Add, Nouns.SearchRule, DefaultParameterSetName = PathParameterSet,
        ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
public sealed class AddSearchRuleCommand : DefaultCatalogCommandBase
{
    private const string PathParameterSet = "PathParameterSet";
    private const string InputParameterSet = "InputParameterSet";

    [Parameter(Mandatory = true, ParameterSetName = PathParameterSet, Position = 0, ValueFromPipeline = true)]
    [ValidateNotNullOrEmpty()]
    public string[]? Path { get; set; }

    [Parameter(ParameterSetName = PathParameterSet)]
    public SearchRuleInfo.SearchRuleSet RuleSet { get; set; } = SearchRuleInfo.SearchRuleSet.User;

    [Parameter(Mandatory = true, ParameterSetName = PathParameterSet)]
    public SearchRuleInfo.SearchRuleType RuleType { get; set; }

    [Parameter(ParameterSetName = PathParameterSet)]
    public SwitchParameter OverrideChildren { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = InputParameterSet, ValueFromPipeline = true)]
    public SearchRuleInfo[]? InputObject { get; set; }

    private ISearchCrawlScopeManager? _scopeManager;
    private ISearchCrawlScopeManager ScopeManager { get => _scopeManager ??= new CSearchManager().GetCatalog(Catalog).GetCrawlScopeManager(); }

    protected override void ProcessRecord()
    {
        if (ParameterSetName == InputParameterSet)
        {
            if (InputObject?.Length > 0)
            {
                foreach (SearchRuleInfo info in InputObject)
                {
                    // TODO stringres?
                    string target = $"{Catalog} {info.RuleSet} {info.RuleType} {nameof(SearchRootInfo.Path)}={info.Path}";
                    if (ShouldProcess(target))
                    {
                        if (info.RuleSet == SearchRuleInfo.SearchRuleSet.Default)
                        {
                            if (info.OverrideChildren)
                            {
                                // TODO stringres? more details (path etc)?
                                WriteWarning("OverrideChildren not supported for Default rule set.");
                            }
                            ScopeManager.AddDefaultScopeRule(info.Path, (info.RuleType == SearchRuleInfo.SearchRuleType.Include) ? 1 : 0, (uint)info.FollowFlags);
                        }
                        else
                        {
                            ScopeManager.AddUserScopeRule(info.Path, (info.RuleType == SearchRuleInfo.SearchRuleType.Include) ? 1 : 0, OverrideChildren ? 1 : 0, (uint)info.FollowFlags);
                        }
                    }
                }
            }
        }
        else if (Path?.Length > 0)
        {
            foreach (string path in Path)
            {
                // TODO stringres?
                string target = $"{Catalog} {RuleSet} {RuleType} {nameof(Path)}={path}";
                if (ShouldProcess(target))
                {
                    if (RuleSet == SearchRuleInfo.SearchRuleSet.Default)
                    {
                        if (OverrideChildren)
                        {
                            // TODO stringres? more details (path etc)?
                            WriteWarning("OverrideChildren not supported for Default rule set.");
                        }
                        ScopeManager.AddDefaultScopeRule(path, (RuleType == SearchRuleInfo.SearchRuleType.Include) ? 1 : 0, (uint)SearchRuleInfo._FOLLOW_FLAGS.FF_INDEXCOMPLEXURLS);
                    }
                    else
                    {
                        ScopeManager.AddUserScopeRule(path, (RuleType == SearchRuleInfo.SearchRuleType.Include) ? 1 : 0, OverrideChildren ? 1 : 0, (uint)SearchRuleInfo._FOLLOW_FLAGS.FF_INDEXCOMPLEXURLS);
                    }
                }
            }
        }
    }

    protected override void EndProcessing()
    {
        // Only !=null if something has been added/removed.
        _scopeManager?.SaveAll();
    }
}

/// <summary>
/// Remove one or more search rules from a catalog.
/// </summary>
[Cmdlet(VerbsCommon.Remove, Nouns.SearchRule, ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
public sealed class RemoveSearchRuleCommand : DefaultCatalogCommandBase
{
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty()]
    public string[]? Path { get; set; }

    [Parameter]
    public SearchRuleInfo.SearchRuleSet RuleSet { get; set; } = SearchRuleInfo.SearchRuleSet.User;

    private ISearchCrawlScopeManager? _scopeManager;
    private ISearchCrawlScopeManager ScopeManager { get => _scopeManager ??= new CSearchManager().GetCatalog(Catalog).GetCrawlScopeManager(); }

    protected override void ProcessRecord()
    {
        if (Path?.Length > 0)
        {
            foreach (string path in Path)
            {
                // TODO stringres?
                string target = $"{Catalog} {RuleSet} {nameof(Path)}={path}";
                if (ShouldProcess(target))
                {
                    if (RuleSet == SearchRuleInfo.SearchRuleSet.Default)
                    {
                        ScopeManager.RemoveDefaultScopeRule(path);
                    }
                    else
                    {
                        ScopeManager.RemoveScopeRule(path);
                    }
                }
            }
        }
    }

    protected override void EndProcessing()
    {
        // Only !=null if something has been added/removed.
        _scopeManager?.SaveAll();
    }
}

/// <summary>
/// Resets a catalog to defaults search rules.
/// </summary>
[Cmdlet(VerbsCommon.Reset, Nouns.SearchRule, ConfirmImpact = ConfirmImpact.High, SupportsShouldProcess = true)]
public sealed class ResetSearchRuleCommand : DefaultCatalogCommandBase
{
    protected override void EndProcessing()
    {
        if (ShouldProcess(Catalog))
        {
            CSearchManager manager = new();
            ISearchCatalogManager catalog = manager.GetCatalog(Catalog);
            ISearchCrawlScopeManager scope = catalog.GetCrawlScopeManager();
            scope.RevertToDefaultScopes();
        }
    }
}

/// <summary>
/// Tests a path against the search rules in a catalog.
/// </summary>
[Cmdlet(VerbsDiagnostic.Test, Nouns.SearchRule, DefaultParameterSetName = IncludedParameterSet)]
[OutputType(typeof(bool), typeof(TestSearchRuleInfo))]
public sealed class TestSearchRuleCommand : DefaultCatalogCommandBase
{
    private const string IncludedParameterSet = "IncludedParameterSet";
    private const string ChildScopeParameterSet = "ChildScopeParameterSet";
    private const string ParentScopeParameterSet = "ParentScopeParameterSet";
    private const string DetailedParameterSet = "DetailedParameterSet";

    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty()]
    public string[]? Path { get; set; }

    [Parameter(Mandatory = false, ParameterSetName = IncludedParameterSet)]
    public SwitchParameter IsIncluded { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = ChildScopeParameterSet)]
    public SwitchParameter HasChildScope { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = ParentScopeParameterSet)]
    public SwitchParameter HasParentScope { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = DetailedParameterSet)]
    public SwitchParameter Detailed { get; set; }

    private ISearchCrawlScopeManager? _scopeManager;
    private ISearchCrawlScopeManager ScopeManager { get => _scopeManager ??= new CSearchManager().GetCatalog(Catalog).GetCrawlScopeManager(); }

    protected override void ProcessRecord()
    {
        if (Path?.Length > 0)
        {
            foreach (string path in Path)
            {
                switch (ParameterSetName)
                {
                    case IncludedParameterSet:
                        WriteObject(ScopeManager.IncludedInCrawlScope(path) != 0);
                        break;
                    case ChildScopeParameterSet:
                        WriteObject(ScopeManager.HasChildScopeRule(path) != 0);
                        break;
                    case ParentScopeParameterSet:
                        WriteObject(ScopeManager.HasParentScopeRule(path) != 0);
                        break;
                    default:
                        ScopeManager.IncludedInCrawlScopeEx(path, out int isIncluded, out CLUSION_REASON reason);
                        TestSearchRuleInfo info = new()
                        {
                            Path = path,
                            IsIncluded = isIncluded != 0,
                            Reason = reason,
                            HasChildScope = ScopeManager.HasChildScopeRule(path) != 0,
                            HasParentScope = ScopeManager.HasParentScopeRule(path) != 0,
                            ParentScopeVersiondId = ScopeManager.GetParentScopeVersionId(path)
                        };
                        WriteObject(info);
                        break;
                }
            }
        }
    }
}
