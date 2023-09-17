// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Reindexes a search catalog either completely or partially.
/// </summary>
/// <remarks>
/// Requires admin rights when using the <c>-All</c> switch.
/// </remarks>
[Cmdlet(VerbsData.Update, Nouns.SearchCatalog, DefaultParameterSetName = AllParameterSet,
        ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
public sealed class UpdateSearchCatalogCommand : SearchApiCommandBase
{
    private const string AllParameterSet = "AllParameterSet";
    private const string RootParameterSet = "RootParameterSet";
    private const string PathParameterSet = "PathParameterSet";

    [Parameter(Mandatory = false, ParameterSetName = AllParameterSet)]
    public SwitchParameter All { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = RootParameterSet, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty()]
    public string[]? RootPath { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = PathParameterSet, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty()]
    [SupportsWildcards()]
    public string[]? Path { get; set; }

    [Parameter]
    [ValidateNotNullOrEmpty()]
    public string Catalog { get; set; } = DefaultCatalogName;

    private ISearchCatalogManager? _catalogManager;
    private ISearchCatalogManager CatalogManager => _catalogManager ??= GetSearchCatalogManager(Catalog);

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Continue after WriteError.")]
    protected override void ProcessRecord()
    {
        if (ParameterSetName == AllParameterSet)
        {
            if (ShouldProcess(Catalog))
            {
                try
                {
                    CatalogManager.Reindex();
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
        else if (RootPath?.Length > 0)
        {
            foreach (string path in RootPath)
            {
                string target = $"{Catalog} {nameof(RootPath)}={path}";
                if (ShouldProcess(target))
                {
                    try
                    {
                        CatalogManager.ReindexSearchRoot(path);
                    }
                    catch (Exception ex)
                    {
                        ErrorRecord rec = new(ex, string.Empty, ErrorCategory.NotSpecified, target);
                        SearchApiErrorHelper.TrySetErrorDetails(rec);
                        WriteError(rec);
                    }
                }
            }
        }
        else if (Path?.Length > 0)
        {
            foreach (string path in Path)
            {
                string target = $"{Catalog} {nameof(Path)}={path}";
                if (ShouldProcess(target))
                {
                    try
                    {
                        CatalogManager.ReindexMatchingURLs(path);
                    }
                    catch (Exception ex)
                    {
                        ErrorRecord rec = new(ex, string.Empty, ErrorCategory.NotSpecified, target);
                        SearchApiErrorHelper.TrySetErrorDetails(rec);
                        WriteError(rec);
                    }
                }
            }
        }
    }
}
