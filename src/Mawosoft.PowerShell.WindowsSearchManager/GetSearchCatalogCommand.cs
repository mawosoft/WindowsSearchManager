// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Gets settings and status of all search catalogs or for a specified one.
/// </summary>
[Cmdlet(VerbsCommon.Get, Nouns.SearchCatalog, ConfirmImpact = ConfirmImpact.None)]
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
