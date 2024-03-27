// Copyright (c) 2023-2024 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Resets a catalog to the default search rules.
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
                scope.SaveAll();
            }
            catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
            {
                ThrowTerminatingError(rec);
            }
        }
    }
}
