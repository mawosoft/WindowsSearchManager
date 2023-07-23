// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Returns an object with settings and status infos for specified or all search catalogs.
/// </summary>
[Cmdlet(VerbsCommon.Get, Nouns.SearchCatalog)]
[OutputType(typeof(SearchCatalogInfo))]
public sealed class GetSearchCatalogCommand : SearchApiCommandBase
{
    [Parameter(Position = 0)]
    [ValidateNotNullOrEmpty()]
    public string? Catalog { get; set; }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Continue after WriteError.")]
    protected override void EndProcessing()
    {
        ISearchManager manager = CreateSearchManager();
        IEnumerable<string> catalogs;
        if (Catalog is not null)
        {
            catalogs = new[] { Catalog };
        }
        else
        {
            ISearchRegistryProvider registry = SearchManagerFactory.CreateSearchRegistryProvider();
            catalogs = registry.GetCatalogNames();
        }
        foreach (string catalog in catalogs)
        {
            try
            {
                WriteObject(new SearchCatalogInfo(EnsureNotNull(manager.GetCatalog(catalog))));
            }
            catch (Exception ex)
            {
                ErrorRecord rec = new(ex, string.Empty, ErrorCategory.NotSpecified, catalog);
                SearchApiErrorHelper.TrySetErrorDetails(rec);
                WriteError(rec);
            }
        }
    }
}

/// <summary>
/// Applies settings to a search catalog.
/// </summary>
/// <remarks>
/// <c>DiacriticSensitivity</c> seems to be the only changeable setting.
/// The timeout properties remain 0 after being set.
/// </remarks>
[Cmdlet(VerbsCommon.Set, Nouns.SearchCatalog, ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
public sealed class SetSearchCatalogCommand : SearchApiCommandBase
{
    [Parameter]
    public uint ConnectTimeout { get; set; }

    [Parameter]
    public uint DataTimeout { get; set; }

    [Parameter]
    public SwitchParameter DiacriticSensitivity { get; set; }

    [Parameter]
    [ValidateNotNullOrEmpty()]
    public string Catalog { get; set; } = DefaultCatalogName;

    protected override void EndProcessing()
    {
        if (ShouldProcess(Catalog))
        {
            ISearchCatalogManager catalog = GetCatalogManager(Catalog);
            try
            {
                if (MyInvocation.BoundParameters.ContainsKey(nameof(ConnectTimeout)))
                {
                    catalog.ConnectTimeout = ConnectTimeout;
                }
                if (MyInvocation.BoundParameters.ContainsKey(nameof(DataTimeout)))
                {
                    catalog.DataTimeout = DataTimeout;
                }
                if (MyInvocation.BoundParameters.ContainsKey(nameof(DiacriticSensitivity)))
                {
                    catalog.DiacriticSensitivity = DiacriticSensitivity ? 1 : 0;
                }
            }
            catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
            {
                ThrowTerminatingError(rec);
                throw; // Unreachable
            }
        }
    }
}

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
            ISearchCatalogManager catalog = GetCatalogManager(Catalog);
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
                throw; // Unreachable

            }
            catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
            {
                ThrowTerminatingError(rec);
                throw; // Unreachable
            }
        }
    }
}

/// <summary>
/// Updates a search catalog by reindexing it either completely or partially.
/// Requires Admin rights for complete reindexing.
/// </summary>
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
    private ISearchCatalogManager CatalogManager => _catalogManager ??= GetCatalogManager(Catalog);

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
                    throw; // Unreachable

                }
                catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
                {
                    ThrowTerminatingError(rec);
                    throw; // Unreachable
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
                throw; // Unreachable
            }
        }
    }
}

/// <summary>
/// Deletes a search catalog
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
                throw; // Unreachable
            }
        }
    }
}
