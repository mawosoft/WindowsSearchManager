// Copyright (c) 2023 Matthias Wolf, Mawosoft.

using System.Reflection;

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Implementation of <see cref="ISearchRegistryProvider"/> for Windows PowerShell.
/// </summary>
/// <remarks>
/// In Windows PowerShell, the <see cref="Microsoft.Win32.Registry"/> type is already loaded via mscorlib.
/// Using the type directly via the extension package (as in <see cref="SearchRegistryProviderPSCore"/>)
/// would cause a TypeLoadException. Therefore we use reflection here.
/// </remarks>
internal class SearchRegistryProviderPSDesktop : SearchRegistryProviderBase, ISearchRegistryProvider
{
    // Reflection data for Registry encapsulated for lazy init.
    private class Win32Registry
    {
        public Type Registry;
        public Type RegistryKey;
        public object LocalMachine;
        public MethodInfo OpenSubKey;
        public MethodInfo GetSubKeyNames;
        // Parameter arrays for method invocation
        public string[] CatalogListWindowsCatalogs = new[] {
            SearchRegistryProviderBase.CatalogListWindowsCatalogs
        };

        public Win32Registry()
        {
            Registry = Type.GetType("Microsoft.Win32.Registry", throwOnError: true);
            FieldInfo fi = Registry.GetField(nameof(LocalMachine))
                           ?? throw new MissingFieldException(nameof(LocalMachine));
            LocalMachine = fi.GetValue(null)
                           ?? throw new ArgumentNullException(nameof(LocalMachine));
            RegistryKey = LocalMachine.GetType();
            OpenSubKey = RegistryKey.GetMethod(nameof(OpenSubKey), new[] { typeof(string) })
                         ?? throw new MissingMethodException(nameof(OpenSubKey));
            GetSubKeyNames = RegistryKey.GetMethod(nameof(GetSubKeyNames))
                             ?? throw new MissingMethodException(nameof(GetSubKeyNames));
        }
    }

    private static readonly Lazy<Win32Registry> s_registry = new();

    /// <inheritdoc/>
    public IReadOnlyList<string> GetCatalogNames()
    {
        Win32Registry r = s_registry.Value;
        object? subkey = r.OpenSubKey.Invoke(r.LocalMachine, r.CatalogListWindowsCatalogs);
        try
        {
            if (subkey is not null && r.GetSubKeyNames.Invoke(subkey, null) is IReadOnlyList<string> names)
            {
                return names;
            }
        }
        finally
        {
            (subkey as IDisposable)?.Dispose();
        }
        return Array.Empty<string>();
    }
}
