// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Adds search roots to a search catalog.
/// </summary>
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

    [Parameter]
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
                infos[i] = new() { Path = Path[i] };
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
        }
    }

    protected override void EndProcessing() => SaveCrawlScopeManager(_scopeManager);
}
