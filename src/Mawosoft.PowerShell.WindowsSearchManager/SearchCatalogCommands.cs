// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Returns an object with settings and status infos for a search catalog.
/// </summary>
[Cmdlet(VerbsCommon.Get, Nouns.SearchCatalog)]
[OutputType(typeof(SearchCatalogInfo))]
public sealed class GetSearchCatalogCommand : DefaultCatalogCommandBase
{
    protected override void EndProcessing()
    {
        CSearchManager manager = new();
        ISearchCatalogManager catalog = manager.GetCatalog(Catalog);
        SearchCatalogInfo info = new(catalog);
        WriteObject(info);
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
public sealed class SetSearchCatalogCommand : DefaultCatalogCommandBase
{
    [Parameter]
    public uint ConnectTimeout { get; set; }

    [Parameter]
    public uint DataTimeout { get; set; }

    [Parameter]
    public SwitchParameter DiacriticSensitivity { get; set; }

    protected override void EndProcessing()
    {
        if (ShouldProcess(Catalog))
        {
            CSearchManager manager = new();
            ISearchCatalogManager catalog = manager.GetCatalog(Catalog);
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
    }
}

/// <summary>
/// Resets a search catalog by completely rebuilding the index database. Requires Admin rights.
/// </summary>
[Cmdlet(VerbsCommon.Reset, Nouns.SearchCatalog, ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
public sealed class ResetSearchCatalogCommand : DefaultCatalogCommandBase
{
    protected override void EndProcessing()
    {
        if (ShouldProcess(Catalog))
        {
            CSearchManager manager = new();
            ISearchCatalogManager catalog = manager.GetCatalog(Catalog);
            catalog.Reset();
        }
    }
}

/// <summary>
/// Updates a search catalog by reindexing it either completely or partially.
/// Requires Admin rights for complete reindexing.
/// </summary>
[Cmdlet(VerbsData.Update, Nouns.SearchCatalog, DefaultParameterSetName = AllParameterSet,
        ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
public sealed class UpdateSearchCatalogCommand : DefaultCatalogCommandBase
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
    // TODO Prioritize (ISearchCatalogManager2) switch in PathParameterSet?

    private ISearchCatalogManager? _catalogManager;
    private ISearchCatalogManager CatalogManager { get => _catalogManager ??= new CSearchManager().GetCatalog(Catalog); }

    protected override void ProcessRecord()
    {
        if (ParameterSetName == AllParameterSet)
        {
            if (ShouldProcess(Catalog))
            {
                CatalogManager.Reindex();
            }
        }
        else if (RootPath?.Length > 0)
        {
            foreach (string path in RootPath)
            {
                // TODO stringres?
                string target = $"{Catalog} {nameof(RootPath)}={path}";
                if (ShouldProcess(target))
                {
                    CatalogManager.ReindexSearchRoot(path);
                }
            }
        }
        else if (Path?.Length > 0)
        {
            foreach (string path in Path)
            {
                // TODO stringres?
                string target = $"{Catalog} {nameof(Path)}={path}";
                if (ShouldProcess(target))
                {
                    // TODO error 0x80040D07 if path is excluded from index.
                    // Can be confusing when using nonexisting path like c:\foo which indeed is not included unless c:\ is.
                    CatalogManager.ReindexMatchingURLs(path);
                }
            }
        }
    }
}

/// <summary>
/// Creates a new search catalog.
/// </summary>
[Cmdlet(VerbsCommon.New, Nouns.SearchCatalog, ConfirmImpact = ConfirmImpact.Low, SupportsShouldProcess = true)]
public sealed class NewSearchCatalogCommand : Cmdlet
{
    [Parameter(Mandatory = true)]
    [ValidateNotNullOrEmpty()]
    public string? Catalog { get; set; }

    protected override void EndProcessing()
    {
        if (Catalog != null && ShouldProcess(Catalog))
        {
            ISearchManager2 manager = new CSearchManager() as ISearchManager2 ?? throw new NotSupportedException();
            manager.CreateCatalog(Catalog, out ISearchCatalogManager catalog);
        }
    }
}

/// <summary>
/// Deletes a search catalog
/// </summary>
[Cmdlet(VerbsCommon.Remove, Nouns.SearchCatalog, ConfirmImpact = ConfirmImpact.High, SupportsShouldProcess = true)]
public sealed class RemoveSearchCatalogCommand : Cmdlet
{
    [Parameter(Mandatory = true)]
    [ValidateNotNullOrEmpty()]
    public string? Catalog { get; set; }

    protected override void EndProcessing()
    {
        if (Catalog != null && ShouldProcess(Catalog))
        {
            ISearchManager2 manager = new CSearchManager() as ISearchManager2 ?? throw new NotSupportedException();
            manager.DeleteCatalog(Catalog);
        }
    }
}
