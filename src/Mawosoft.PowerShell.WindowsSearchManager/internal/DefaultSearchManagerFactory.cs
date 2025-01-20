// Copyright (c) Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Default implementation of <see cref="ISearchManagerFactory"/>.
/// </summary>
internal sealed class DefaultSearchManagerFactory : ISearchManagerFactory
{
    public static ISearchManagerFactory Instance = new DefaultSearchManagerFactory();

    private static readonly bool s_isNetFramework =
        RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework", StringComparison.Ordinal);

    private DefaultSearchManagerFactory() { }

    public ISearchManager CreateSearchManager() => new CSearchManager();
    public ISearchRegistryProvider CreateSearchRegistryProvider() => s_isNetFramework
        ? new SearchRegistryProviderPSDesktop()
        : new SearchRegistryProviderPSCore();
}
