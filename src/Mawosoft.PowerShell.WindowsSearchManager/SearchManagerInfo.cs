// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// POCO for CSearchManager
/// Everything except version info requires admin rights.
/// </summary>
public sealed class SearchManagerInfo : ICloneable
{
    public string? Version { get; }
    public uint MajorVersion { get; }
    public uint MinorVersion { get; }
    public string? UserAgent { get; set; }
    public _PROXY_ACCESS ProxyAccess { get; set; }
    public string? ProxyName { get; set; }
    public uint ProxyPortNumber { get; set; }
    public bool ProxyBypassLocal { get; set; }
    public string? ProxyBypassList { get; set; }

    public SearchManagerInfo() { }

    internal SearchManagerInfo(ISearchManager searchManager)
    {
        if (searchManager == null) throw new ArgumentNullException(nameof(searchManager));

        searchManager.GetIndexerVersionStr(out string version);
        Version = version;
        searchManager.GetIndexerVersion(out uint major, out uint minor);
        MajorVersion = major;
        MinorVersion = minor;
        UserAgent =  searchManager.UserAgent;
        ProxyAccess = searchManager.UseProxy;
        ProxyName = searchManager.ProxyName;
        ProxyPortNumber = searchManager.PortNumber;
        ProxyBypassLocal = searchManager.LocalBypass != 0;
        ProxyBypassList = searchManager.BypassList;
    }

    public object Clone() => MemberwiseClone();
}
