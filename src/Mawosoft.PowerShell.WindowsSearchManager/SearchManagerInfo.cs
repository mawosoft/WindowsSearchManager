// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Contains the global settings and version information for Windows Search.
/// </summary>
public sealed class SearchManagerInfo : ICloneable
{
    /// <summary>Gets the complete version number of the indexer.</summary>
    /// <value>The complete version number of the indexer.</value>
    public string? Version { get; }

    /// <summary>Gets the major version number of the indexer.</summary>
    /// <value>The major version number of the indexer.</value>
    public uint MajorVersion { get; }

    /// <summary>Gets the minor version number of the indexer.</summary>
    /// <value>The minor version number of the indexer.</value>
    public uint MinorVersion { get; }

    /// <summary>Gets or sets the user agent string.</summary>
    /// <value>The user agent string.</value>
    public string? UserAgent { get; set; }

    /// <summary>Gets or sets if and how a proxy server is to be used.</summary>
    /// <value>One of the enumeration values that identifies the proxy use.</value>
    public _PROXY_ACCESS ProxyAccess { get; set; }

    /// <summary>Gets or sets the proxy name.</summary>
    /// <value>The proxy name.</value>
    public string? ProxyName { get; set; }

    /// <summary>Gets or sets the proxy port number.</summary>
    /// <value>The proxy port number.</value>
    public uint ProxyPortNumber { get; set; }

    /// <summary>Gets or sets whether the proxy should be bypassed for local domains.</summary>
    /// <value><c>true</c> if the proxy is bypassed for local domains, <c>false</c> otherwise.</value>
    public bool ProxyBypassLocal { get; set; }

    /// <summary>Gets or sets a comma-separated list of items that are considered local by the indexer.</summary>
    /// <value>A comma-separated list of items that are considered local by the indexer.</value>
    public string? ProxyBypassList { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchManagerInfo"/> class.
    /// </summary>
    public SearchManagerInfo() { }

    internal SearchManagerInfo(ISearchManager searchManager)
    {
        if (searchManager is null) throw new ArgumentNullException(nameof(searchManager));

        searchManager.GetIndexerVersionStr(out string version);
        Version = version;
        searchManager.GetIndexerVersion(out uint major, out uint minor);
        MajorVersion = major;
        MinorVersion = minor;
        UserAgent = searchManager.UserAgent;
        ProxyAccess = searchManager.UseProxy;
        ProxyName = searchManager.ProxyName;
        ProxyPortNumber = searchManager.PortNumber;
        ProxyBypassLocal = searchManager.LocalBypass != 0;
        ProxyBypassList = searchManager.BypassList;
    }

    /// <summary>
    /// Creates a shallow copy of the <see cref="SearchManagerInfo"/> instance.
    /// </summary>
    /// <returns>A shallow copy of the <see cref="SearchManagerInfo"/> instance.</returns>
    public object Clone() => MemberwiseClone();
}
