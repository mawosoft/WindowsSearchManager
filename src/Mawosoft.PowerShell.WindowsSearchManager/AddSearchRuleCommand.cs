// Copyright (c) Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Adds search rules to a search catalog.
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

    [Parameter(Mandatory = true, ParameterSetName = PathParameterSet, Position = 1)]
    public SearchRuleInfo.SearchRuleType RuleType { get; set; }

    [Parameter(ParameterSetName = PathParameterSet, Position = 2)]
    public SearchRuleInfo.SearchRuleSet RuleSet { get; set; } = SearchRuleInfo.SearchRuleSet.User;

    [Parameter(Mandatory = true, ParameterSetName = InputParameterSet, ValueFromPipeline = true)]
    public SearchRuleInfo[]? InputObject { get; set; }

    [Parameter]
    public SwitchParameter OverrideChildren { get; set; }

    [Parameter]
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
                infos[i] = new()
                {
                    Path = Path[i],
                    RuleType = RuleType,
                    RuleSet = RuleSet
                };
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
                        if (OverrideChildren)
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
