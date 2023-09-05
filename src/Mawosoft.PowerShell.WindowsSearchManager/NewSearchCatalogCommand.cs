// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Creates a new search catalog.
/// </summary>
[Cmdlet(VerbsCommon.New, Nouns.SearchCatalog, ConfirmImpact = ConfirmImpact.Low, SupportsShouldProcess = true)]
public sealed class NewSearchCatalogCommand : SearchApiCommandBase
{
    [Parameter(Mandatory = true, Position = 0)]
    [ValidateNotNullOrEmpty()]
    public string? Catalog { get; set; }

    protected override void EndProcessing()
    {
        if (Catalog is not null && ShouldProcess(Catalog))
        {
            ISearchManager2 manager = GetSearchManager2();
            try
            {
                manager.CreateCatalog(Catalog, out _);
            }
            catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
            {
                ThrowTerminatingError(rec);
            }
        }
    }
}
