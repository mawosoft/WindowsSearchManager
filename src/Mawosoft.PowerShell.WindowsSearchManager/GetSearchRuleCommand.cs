// Copyright (c) Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Gets the search rules in effect for a search catalog.
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
