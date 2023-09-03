// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Returns a collection of objects representing the search rules of a catalog.
/// </summary>
[Cmdlet(VerbsCommon.Get, Nouns.SearchRule, ConfirmImpact = ConfirmImpact.None)]
[OutputType(typeof(SearchRuleInfo))]
public sealed class GetSearchRuleCommand : SearchApiCommandBase
{
    [Parameter(Position = 0)]
    [ValidateNotNullOrEmpty()]
    public string Catalog { get; set; } = DefaultCatalogName;

    protected override void EndProcessing()
    {
        ISearchCrawlScopeManager scope = GetCrawlScopeManager(Catalog);
        try
        {
            IEnumSearchScopeRules? rules = scope.EnumerateScopeRules();
            if (rules is null) return; // null -> none
            for (; ; )
            {
                uint fetched = 0;
                rules.Next(1, out CSearchScopeRule rule, ref fetched);
                if (fetched != 1 || rule is null) break;
                WriteObject(new SearchRuleInfo(rule));
            }
        }
        catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
        {
            ThrowTerminatingError(rec);
        }
    }
}

/// <summary>
/// Adds one or more search rules to a catalog.
/// </summary>
[Cmdlet(VerbsCommon.Add, Nouns.SearchRule, DefaultParameterSetName = PathParameterSet,
        ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
public sealed class AddSearchRuleCommand : SearchApiCommandBase
{
    private const string PathParameterSet = "PathParameterSet";
    private const string InputParameterSet = "InputParameterSet";

    [Parameter(Mandatory = true, ParameterSetName = PathParameterSet, Position = 0, ValueFromPipeline = true)]
    [ValidateNotNullOrEmpty()]
    public string[]? Path { get; set; }

    [Parameter(ParameterSetName = PathParameterSet, Position = 1)]
    public SearchRuleInfo.SearchRuleSet RuleSet { get; set; } = SearchRuleInfo.SearchRuleSet.User;

    [Parameter(Mandatory = true, ParameterSetName = PathParameterSet, Position = 2)]
    public SearchRuleInfo.SearchRuleType RuleType { get; set; }

    [Parameter(ParameterSetName = PathParameterSet)]
    public SwitchParameter OverrideChildren { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = InputParameterSet, ValueFromPipeline = true)]
    public SearchRuleInfo[]? InputObject { get; set; }

    [Parameter(Position = 3)]
    [ValidateNotNullOrEmpty()]
    public string Catalog { get; set; } = DefaultCatalogName;

    private ISearchCrawlScopeManager? _scopeManager;
    private ISearchCrawlScopeManager ScopeManager => _scopeManager ??= GetCrawlScopeManager(Catalog);

    protected override void ProcessRecord()
    {
        SearchRuleInfo[] infos;
        if (ParameterSetName == InputParameterSet)
        {
            if (!(InputObject?.Length > 0)) return;
            infos = InputObject;
        }
        else
        {
            if (!(Path?.Length > 0)) return;
            infos = new SearchRuleInfo[Path.Length];
            for (int i = 0; i < Path.Length; i++)
            {
                infos[i].Path = Path[i];
                infos[i].RuleType = RuleType;
                infos[i].RuleSet = RuleSet;
                infos[i].OverrideChildren = OverrideChildren;
            }
        }
        try
        {
            foreach (SearchRuleInfo info in infos)
            {
                string target = $"{Catalog} {info.RuleSet} {info.RuleType} {nameof(Path)}={info.Path}";
                if (ShouldProcess(target))
                {
                    if (info.RuleSet == SearchRuleInfo.SearchRuleSet.User)
                    {
                        ScopeManager.AddUserScopeRule(
                            info.Path,
                            (info.RuleType == SearchRuleInfo.SearchRuleType.Include) ? 1 : 0,
                            OverrideChildren ? 1 : 0,
                            (uint)info.FollowFlags);
                    }
                    else
                    {
                        if (info.OverrideChildren)
                        {
                            WriteWarning(SR.OverrideChildrenNotSupported + Environment.NewLine + "    " + target);
                        }
                        ScopeManager.AddDefaultScopeRule(
                            info.Path,
                            (info.RuleType == SearchRuleInfo.SearchRuleType.Include) ? 1 : 0,
                            (uint)info.FollowFlags);
                    }
                }
            }
        }
        catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
        {
            ThrowTerminatingError(rec);
        }
    }

    protected override void EndProcessing() => SaveCrawlScopeManager(_scopeManager);
}

/// <summary>
/// Remove one or more search rules from a catalog.
/// </summary>
[Cmdlet(VerbsCommon.Remove, Nouns.SearchRule, ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
public sealed class RemoveSearchRuleCommand : SearchApiCommandBase
{
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty()]
    public string[]? Path { get; set; }

    [Parameter(Position = 1)]
    public SearchRuleInfo.SearchRuleSet RuleSet { get; set; } = SearchRuleInfo.SearchRuleSet.User;

    [Parameter(Position = 2)]
    [ValidateNotNullOrEmpty()]
    public string Catalog { get; set; } = DefaultCatalogName;

    private ISearchCrawlScopeManager? _scopeManager;
    private ISearchCrawlScopeManager ScopeManager => _scopeManager ??= GetCrawlScopeManager(Catalog);

    protected override void ProcessRecord()
    {
        if (!(Path?.Length > 0)) return;

        try
        {
            foreach (string path in Path)
            {
                string target = $"{Catalog} {RuleSet} {nameof(Path)}={path}";
                if (ShouldProcess(target))
                {
                    if (RuleSet == SearchRuleInfo.SearchRuleSet.User)
                    {
                        ScopeManager.RemoveScopeRule(path);
                    }
                    else
                    {
                        ScopeManager.RemoveDefaultScopeRule(path);
                    }
                }
            }
        }
        catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
        {
            ThrowTerminatingError(rec);
        }
    }

    protected override void EndProcessing() => SaveCrawlScopeManager(_scopeManager);
}

/// <summary>
/// Resets a catalog to defaults search rules.
/// </summary>
[Cmdlet(VerbsCommon.Reset, Nouns.SearchRule, ConfirmImpact = ConfirmImpact.High, SupportsShouldProcess = true)]
public sealed class ResetSearchRuleCommand : SearchApiCommandBase
{
    [Parameter(Position = 0)]
    [ValidateNotNullOrEmpty()]
    public string Catalog { get; set; } = DefaultCatalogName;

    protected override void EndProcessing()
    {
        if (ShouldProcess(Catalog))
        {
            ISearchCrawlScopeManager scope = GetCrawlScopeManager(Catalog);
            try
            {
                scope.RevertToDefaultScopes();
            }
            catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
            {
                ThrowTerminatingError(rec);
            }
        }
    }
}

/// <summary>
/// Tests a path against the search rules in a catalog.
/// </summary>
[Cmdlet(VerbsDiagnostic.Test, Nouns.SearchRule, DefaultParameterSetName = IncludedParameterSet,
    ConfirmImpact = ConfirmImpact.None)]
[OutputType(typeof(bool), typeof(TestSearchRuleInfo))]
public sealed class TestSearchRuleCommand : SearchApiCommandBase
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

    [Parameter(Position = 1)]
    [ValidateNotNullOrEmpty()]
    public string Catalog { get; set; } = DefaultCatalogName;

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Continue after WriteError.")]
    protected override void ProcessRecord()
    {
        if (!(Path?.Length > 0)) return;

        ISearchCrawlScopeManager scope = GetCrawlScopeManager(Catalog);

        foreach (string path in Path)
        {
            try
            {
                switch (ParameterSetName)
                {
                    case IncludedParameterSet:
                        WriteObject(scope.IncludedInCrawlScope(path) != 0);
                        break;
                    case ChildScopeParameterSet:
                        WriteObject(scope.HasChildScopeRule(path) != 0);
                        break;
                    case ParentScopeParameterSet:
                        WriteObject(scope.HasParentScopeRule(path) != 0);
                        break;
                    default:
                        scope.IncludedInCrawlScopeEx(path, out int isIncluded, out CLUSION_REASON reason);
                        TestSearchRuleInfo info = new()
                        {
                            Path = path,
                            IsIncluded = isIncluded != 0,
                            Reason = reason,
                            HasChildScope = scope.HasChildScopeRule(path) != 0,
                            HasParentScope = scope.HasParentScopeRule(path) != 0,
                            ParentScopeVersiondId = scope.GetParentScopeVersionId(path)
                        };
                        WriteObject(info);
                        break;
                }
            }
            catch (Exception ex)
            {
                string target = $"{Catalog} {nameof(Path)}={path}";
                ErrorRecord rec = new(ex, string.Empty, ErrorCategory.NotSpecified, target);
                SearchApiErrorHelper.TrySetErrorDetails(rec);
                WriteError(rec);
            }
        }
    }
}
