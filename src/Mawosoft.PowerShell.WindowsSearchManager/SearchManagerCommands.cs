// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Returns an object with the global SearchManager settings. Requires admin rights.
/// </summary>
[Cmdlet(VerbsCommon.Get, Nouns.SearchManager)]
[OutputType(typeof(SearchManagerInfo))]
public sealed class GetSearchManagerCommand : Cmdlet
{
    protected override void EndProcessing()
    {
        // TODO Should we catch 'access denied'?
        SearchManagerInfo info = new(new CSearchManager());
        WriteObject(info);
    }
}

/// <summary>
/// Applies global SearchManager settings. Requires admin rights.
/// </summary>
[Cmdlet(VerbsCommon.Set, Nouns.SearchManager, ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
public sealed class SetSearchManagerCommand : PSCmdlet
{
    [Parameter]
    [ValidateNotNullOrEmpty()]
    public string? UserAgent { get; set; }

    [Parameter]
    public _PROXY_ACCESS ProxyAccess { get; set; }

    /// <summary>
    /// Gets or sets the proxy name.
    /// An empty string is allowed to clear the proxy info if ProxyAccess != PROXY_ACCESS_PROXY.
    /// </summary>
    [Parameter]
    [ValidateNotNull()]
    public string? ProxyName { get; set; }

    /// <summary>
    /// Gets or sets the proxy port number.
    /// Valid numbers start with 1, but 0 is allowed for clearing the proxy info if ProxyAccess != PROXY_ACCESS_PROXY.
    /// </summary>
    [Parameter]
    [ValidateRange(0, ushort.MaxValue)]
    public uint ProxyPortNumber { get; set; }

    [Parameter]
    public SwitchParameter ProxyBypassLocal { get; set; }

    /// <summary>
    /// Gets or sets the proxy bypass list. An empty string is allowed if there is nothing to bypass.
    /// </summary>
    [Parameter]
    [ValidateNotNull()]
    public string? ProxyBypassList { get; set; }

    protected override void EndProcessing()
    {
        CSearchManager manager = new();
        SearchManagerInfo info = new(manager);
        bool setProxy = false;
        bool setUserAgent = false;
        if (MyInvocation.BoundParameters.ContainsKey(nameof(ProxyAccess)))
        {
            info.ProxyAccess = ProxyAccess;
            setProxy = true;
        }
        if (ProxyName != null)
        {
            info.ProxyName = ProxyName;
            setProxy = true;
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(ProxyPortNumber)))
        {
            info.ProxyPortNumber = ProxyPortNumber;
            setProxy = true;
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(ProxyBypassLocal)))
        {
            info.ProxyBypassLocal = ProxyBypassLocal;
            setProxy = true;
        }
        if (ProxyBypassList != null)
        {
            info.ProxyBypassList = ProxyBypassList;
            setProxy = true;
        }

        string description = string.Empty;
        if (!string.IsNullOrEmpty(UserAgent))
        {
            setUserAgent = true;
            if (description.Length > 0) { description += Environment.NewLine; }
            // TODO stringres
            description += $"Setting UserAgent={UserAgent}";
        }
        if (setProxy)
        {
            if (description.Length > 0) { description += Environment.NewLine; }
            // TODO stringres
            description += $"Setting Proxy: Access={info.ProxyAccess}, Name={info.ProxyName}, PortNumber={info.ProxyPortNumber}, BypassLocal={info.ProxyBypassLocal}, BypassList={info.ProxyBypassList}";
        }

        if ((setProxy || setUserAgent) && ShouldProcess(description))
        {
            if (setProxy)
            {
                // TODO Invalid proxy settings will result in COMException with HRESULT: 0x80040D31.
                // See C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\um\WindowsSearchErrors.h
                // See also C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\shared\winerror.h
                manager.SetProxy(info.ProxyAccess, info.ProxyBypassLocal ? 1 : 0, info.ProxyPortNumber, info.ProxyName, info.ProxyBypassList);
            }
            if (setUserAgent)
            {
                manager.UserAgent = UserAgent;
            }
        }
    }
}
