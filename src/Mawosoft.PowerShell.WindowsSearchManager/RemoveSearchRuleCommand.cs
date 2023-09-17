// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Deletes the specified search rules from a search catalog.
/// </summary>
[Cmdlet(VerbsCommon.Remove, Nouns.SearchRule, ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
public sealed class RemoveSearchRuleCommand : SearchApiCommandBase
{
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty()]
    public string[]? Path { get; set; }

    [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
    public SearchRuleInfo.SearchRuleSet RuleSet { get; set; } = SearchRuleInfo.SearchRuleSet.User;

    [Parameter]
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
