// Copyright (c) 2023-2024 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Deletes the specified search catalog.
/// </summary>
[Cmdlet(VerbsCommon.Remove, Nouns.SearchCatalog, ConfirmImpact = ConfirmImpact.High, SupportsShouldProcess = true)]
public sealed class RemoveSearchCatalogCommand : SearchApiCommandBase
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
                manager.DeleteCatalog(Catalog);
            }
            catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
            {
                ThrowTerminatingError(rec);
            }
        }
    }
}
