// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Returns a collection of objects representing the search roots of a catalog.
/// </summary>
[Cmdlet(VerbsCommon.Get, Nouns.SearchRoot)]
[OutputType(typeof(SearchRootInfo), typeof(string))]
public sealed class GetSearchRootCommand : SearchApiCommandBase
{
    [Parameter(Position = 0)]
    [ValidateNotNullOrEmpty()]
    public string Catalog { get; set; } = DefaultCatalogName;

    [Parameter]
    public SwitchParameter PathOnly { get; set; }

    protected override void EndProcessing()
    {
        ISearchCrawlScopeManager scope = GetCrawlScopeManager(Catalog);
        try
        {
            IEnumSearchRoots? roots = scope.EnumerateRoots();
            if (roots is null) return; // null -> none
            for (; ; )
            {
                uint fetched = 0;
                roots.Next(1, out CSearchRoot root, ref fetched);
                if (fetched != 1 || root is null) break;
                if (PathOnly)
                {
                    WriteObject(root.RootURL);
                }
                else
                {
                    WriteObject(new SearchRootInfo(root));
                }
            }
        }
        catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
        {
            ThrowTerminatingError(rec);
            throw; // Unreachable
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
public sealed class AddSearchRootCommand : SearchApiCommandBase
{
    private const string PathParameterSet = "PathParameterSet";
    private const string InputParameterSet = "InputParameterSet";

    [Parameter(Mandatory = true, ParameterSetName = PathParameterSet, Position = 0, ValueFromPipeline = true)]
    [ValidateNotNullOrEmpty()]
    public string[]? Path { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = InputParameterSet, ValueFromPipeline = true)]
    public SearchRootInfo[]? InputObject { get; set; }

    [Parameter(Position = 1)]
    [ValidateNotNullOrEmpty()]
    public string Catalog { get; set; } = DefaultCatalogName;

    private ISearchCrawlScopeManager? _scopeManager;
    private ISearchCrawlScopeManager ScopeManager => _scopeManager ??= GetCrawlScopeManager(Catalog);

    protected override void ProcessRecord()
    {
        SearchRootInfo[] infos;
        if (ParameterSetName == InputParameterSet)
        {
            if (!(InputObject?.Length > 0)) return;
            infos = InputObject;
        }
        else
        {
            if (!(Path?.Length > 0)) return;
            infos = new SearchRootInfo[Path.Length];
            for (int i = 0; i < Path.Length; i++)
            {
                infos[i].Path = Path[i];
            }
        }
        try
        {
            foreach (SearchRootInfo info in infos)
            {
                string target = $"{Catalog} {nameof(Path)}={info.Path}";
                if (ShouldProcess(target))
                {
                    ScopeManager.AddRoot(info.ToCSearchRoot());
                }
            }
        }
        catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
        {
            ThrowTerminatingError(rec);
            throw; // Unreachable
        }
    }

    protected override void EndProcessing() => SaveCrawlScopeManager(_scopeManager);
}

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
            throw; // Unreachable
        }
    }

    protected override void EndProcessing() => SaveCrawlScopeManager(_scopeManager);
}
