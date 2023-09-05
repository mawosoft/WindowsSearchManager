// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Returns a collection of objects representing the search roots of a catalog.
/// </summary>
[Cmdlet(VerbsCommon.Get, Nouns.SearchRoot, ConfirmImpact = ConfirmImpact.None)]
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
        }
    }
}
