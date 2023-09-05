// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Resets a search catalog by completely rebuilding the index database. Requires Admin rights.
/// </summary>
[Cmdlet(VerbsCommon.Reset, Nouns.SearchCatalog, ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
public sealed class ResetSearchCatalogCommand : SearchApiCommandBase
{
    [Parameter(Position = 0)]
    [ValidateNotNullOrEmpty()]
    public string Catalog { get; set; } = DefaultCatalogName;

    protected override void EndProcessing()
    {
        if (ShouldProcess(Catalog))
        {
            ISearchCatalogManager catalog = GetSearchCatalogManager(Catalog);
            try
            {
                catalog.Reset();
            }
            catch (UnauthorizedAccessException ex)
            {
                ThrowTerminatingError(
                    new ErrorRecord(ex, string.Empty, ErrorCategory.PermissionDenied, null)
                    {
                        ErrorDetails = new(SR.AdminRequiredForOperation)
                    });
            }
            catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
            {
                ThrowTerminatingError(rec);
            }
        }
    }
}
