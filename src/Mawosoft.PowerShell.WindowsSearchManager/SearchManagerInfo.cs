// Copyright (c) 2023-2024 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Contains the global settings and version information for Windows Search.
/// </summary>
public sealed class SearchManagerInfo : ICloneable
{
    /// <value>
    /// The complete version number of the indexer. For Windows 10 and newer, this will typically
    /// match the OS version number.
    /// </value>
    public string? Version { get; }

    /// <value>The major version number of the indexer.</value>
    public uint MajorVersion { get; }

    /// <value>The minor version number of the indexer.</value>
    public uint MinorVersion { get; }

    /// <value>The user agent string that a user agent passes to website and services to identify itself.</value>
    public string? UserAgent { get; set; }

    /// <value>One of the enumeration values that indicates if and how a proxy server is used.</value>
    public _PROXY_ACCESS ProxyAccess { get; set; }

    /// <value>The name of the proxy server.</value>
    public string? ProxyName { get; set; }

    /// <value>The port number of the proxy server.</value>
    public uint ProxyPortNumber { get; set; }

    /// <value><c>true</c> if the proxy should be bypassed for local domains, <c>false</c> otherwise.</value>
    public bool ProxyBypassLocal { get; set; }

    /// <value>
    /// A comma-separated list of items that are considered local by the indexer and are not
    /// to be accessed through a proxy server.
    /// </value>
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
