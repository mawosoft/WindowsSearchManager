// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Remove one or more search roots from a catalog.
/// </summary>
[Cmdlet(VerbsCommon.Remove, Nouns.SearchRoot, ConfirmImpact = ConfirmImpact.High, SupportsShouldProcess = true)]
public sealed class RemoveSearchRootCommand : SearchApiCommandBase
{
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty()]
    public string[]? Path { get; set; }

    [Parameter(Position = 1)]
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
                string target = $"{Catalog} {nameof(Path)}={path}";
                if (ShouldProcess(target))
                {
                    ScopeManager.RemoveRoot(path);
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
