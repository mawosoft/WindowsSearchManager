// Copyright (c) 2023 Matthias Wolf, Mawosoft.

using System.IO;

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Returns a collection of objects representing the search roots of a catalog.
/// </summary>
[Cmdlet(VerbsCommon.Get, Nouns.SearchRoot)]
[OutputType(typeof(SearchRootInfo), typeof(string))]
public sealed class GetSearchRootCommand : DefaultCatalogCommandBase
{
    [Parameter]
    public SwitchParameter PathOnly { get; set; }

    protected override void EndProcessing()
    {
        CSearchManager manager = new();
        ISearchCatalogManager catalog = manager.GetCatalog(Catalog);
        ISearchCrawlScopeManager scope = catalog.GetCrawlScopeManager();
        IEnumSearchRoots roots = scope.EnumerateRoots();
        for (; ; )
        {
            uint fetched = 0;
            roots.Next(1, out CSearchRoot root, ref fetched);
            if (fetched != 1 || root == null) break;
            if (PathOnly)
            {
                WriteObject(root.RootURL);
            }
            else
            {
                SearchRootInfo info = new(root);
                WriteObject(info);
            }
        }
    }
}

/// <summary>
/// Adds one or more search roots to a catalog.
/// </summary>
/// <remarks>
/// It is not necessary to add search roots for the <c>file:</c> protocol.
/// If needed, they are automatically created when a search rule is added.
/// </remarks>
[Cmdlet(VerbsCommon.Add, Nouns.SearchRoot, DefaultParameterSetName = PathParameterSet,
        ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
public sealed class AddSearchRootCommand : DefaultCatalogCommandBase
{
    private const string PathParameterSet = "PathParameterSet";
    private const string InputParameterSet = "InputParameterSet";

    [Parameter(Mandatory = true, ParameterSetName = PathParameterSet, Position = 0, ValueFromPipeline = true)]
    [ValidateNotNullOrEmpty()]
    public string[]? Path { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = InputParameterSet, ValueFromPipeline = true)]
    public SearchRootInfo[]? InputObject { get; set; }

    private ISearchCrawlScopeManager? _scopeManager;
    private ISearchCrawlScopeManager ScopeManager { get => _scopeManager ??= new CSearchManager().GetCatalog(Catalog).GetCrawlScopeManager(); }

    protected override void ProcessRecord()
    {
        if (ParameterSetName == InputParameterSet)
        {
            if (InputObject?.Length > 0)
            {
                foreach (SearchRootInfo info in InputObject)
                {
                    // TODO stringres?
                    string target = $"{Catalog} {nameof(SearchRootInfo.Path)}={info.Path}";
                    if (ShouldProcess(target))
                    {
                        ScopeManager.AddRoot(info.ToCSearchRoot());
                    }
                }
            }
        }
        else if (Path?.Length > 0)
        {
            foreach (string path in Path)
            {
                SearchRootInfo info = new()
                {
                    Path = path
                };
                // TODO stringres?
                string target = $"{Catalog} {nameof(SearchRootInfo.Path)}={info.Path}";
                if (ShouldProcess(target))
                {
                    ScopeManager.AddRoot(info.ToCSearchRoot());
                }
            }
        }
    }

    protected override void EndProcessing()
    {
        // Only !=null if something has been added/removed.
        _scopeManager?.SaveAll();
    }
}

/// <summary>
/// Remove one or more search roots from a catalog.
/// </summary>
[Cmdlet(VerbsCommon.Remove, Nouns.SearchRoot, ConfirmImpact = ConfirmImpact.High, SupportsShouldProcess = true)]
public sealed class RemoveSearchRootCommand : DefaultCatalogCommandBase
{
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty()]
    public string[]? Path { get; set; }

    private ISearchCrawlScopeManager? _scopeManager;
    private ISearchCrawlScopeManager ScopeManager { get => _scopeManager ??= new CSearchManager().GetCatalog(Catalog).GetCrawlScopeManager(); }

    protected override void ProcessRecord()
    {
        if (Path?.Length > 0)
        {
            foreach (string path in Path)
            {
                // TODO stringres?
                string target = $"{Catalog} {nameof(Path)}={path}";
                if (ShouldProcess(target))
                {
                    ScopeManager.RemoveRoot(path);
                }
            }
        }
    }

    protected override void EndProcessing()
    {
        // Only !=null if something has been added/removed.
        _scopeManager?.SaveAll();
    }
}
