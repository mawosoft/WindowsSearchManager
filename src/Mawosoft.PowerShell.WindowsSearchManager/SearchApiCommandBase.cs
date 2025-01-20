// Copyright (c) Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Base class for all SearchAPI-related commands.
/// </summary>
public abstract class SearchApiCommandBase : PSCmdlet
{
    /// <summary>
    /// The name of the default Windows Search catalog
    /// </summary>
    public const string DefaultCatalogName = "SystemIndex";

    // Internal for unit testing
    internal static ISearchManagerFactory SearchManagerFactory { get; set; } = DefaultSearchManagerFactory.Instance;

    // Ensure returned COM object is not null.
    [SuppressMessage("Usage", "CA2201:Do not raise reserved exception types", Justification = "Asserting returned COM object.")]
    protected static T EnsureNotNull<T>(T value) where T : class
        => value ?? throw new COMException(string.Format(SR.Culture, SR.ReturnedCOMObjectIsNull, typeof(T).Name), 0);

    // Various helpers to get instances of the SearchAPI management main interfaces.
    // On failure, all of these either throw an Exception or an ErrorRecord (via ThrowTerminatingError),
    // that are considered the 'final word' and should *not* be catched by the caller.

    protected ISearchManager CreateSearchManager()
    {
        try
        {
            return EnsureNotNull(SearchManagerFactory.CreateSearchManager());
        }
        catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
        {
            ThrowTerminatingError(rec);
            throw; // Unreachable
        }
    }

    protected ISearchManager2 GetSearchManager2() => GetSearchManager2(CreateSearchManager());

    protected ISearchManager2 GetSearchManager2(ISearchManager searchManager)
        => (searchManager ?? throw new ArgumentNullException(nameof(searchManager))) as ISearchManager2
           ?? throw new InvalidCastException(string.Format(SR.Culture, SR.InterfaceNotAvailable, nameof(ISearchManager2)));

    protected ISearchCatalogManager GetSearchCatalogManager(string catalogName)
        => GetSearchCatalogManager(CreateSearchManager(), catalogName);

    protected ISearchCatalogManager GetSearchCatalogManager(ISearchManager searchManager, string catalogName)
    {
        if (searchManager is null) throw new ArgumentNullException(nameof(searchManager));
        if (string.IsNullOrWhiteSpace(catalogName)) throw new ArgumentException(null, nameof(catalogName));
        try
        {
            return EnsureNotNull(searchManager.GetCatalog(catalogName));
        }
        catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
        {
            ThrowTerminatingError(rec);
            throw; // Unreachable
        }
    }

    protected ISearchCrawlScopeManager GetCrawlScopeManager(string catalogName)
        => GetCrawlScopeManager(GetSearchCatalogManager(catalogName));

    protected ISearchCrawlScopeManager GetCrawlScopeManager(ISearchCatalogManager catalogManager)
    {
        if (catalogManager is null) throw new ArgumentNullException(nameof(catalogManager));
        try
        {
            return EnsureNotNull(catalogManager.GetCrawlScopeManager());
        }
        catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
        {
            ThrowTerminatingError(rec);
            throw; // Unreachable
        }
    }

    // Helper for commands that modify the scope manager.
    protected void SaveCrawlScopeManager(ISearchCrawlScopeManager? scopeManager)
    {
        if (scopeManager is null) return;
        try
        {
            scopeManager.SaveAll();
        }
        catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
        {
            ThrowTerminatingError(rec);
        }
    }
}
