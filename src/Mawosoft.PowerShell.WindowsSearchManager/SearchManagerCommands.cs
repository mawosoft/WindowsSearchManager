// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Returns an object with the global SearchManager settings. Requires admin rights.
/// </summary>
[Cmdlet(VerbsCommon.Get, Nouns.SearchManager)]
[OutputType(typeof(SearchManagerInfo))]
public sealed class GetSearchManagerCommand : SearchApiCommandBase
{
    protected override void EndProcessing()
    {
        ISearchManager manager = CreateSearchManager();
        try
        {
            WriteObject(new SearchManagerInfo(manager));
        }
        catch (UnauthorizedAccessException ex)
        {
            ThrowTerminatingError(
                new ErrorRecord(ex, string.Empty, ErrorCategory.PermissionDenied, null)
                {
                    ErrorDetails = new(SR.AdminRequiredForOperation)
                });
            throw; // Unreachable

        }
        catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
        {
            ThrowTerminatingError(rec);
            throw; // Unreachable
        }
    }
}

/// <summary>
/// Applies global SearchManager settings. Requires admin rights.
/// </summary>
[Cmdlet(VerbsCommon.Set, Nouns.SearchManager, ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
public sealed class SetSearchManagerCommand : SearchApiCommandBase
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
    /// Gets or sets the proxy bypass list. An empty array or string is allowed if there is nothing to bypass.
    /// </summary>
    [Parameter]
    [ValidateNotNull()]
    public string[]? ProxyBypassList { get; set; }

    protected override void EndProcessing()
    {
        ISearchManager manager = CreateSearchManager();
        SearchManagerInfo info;
        try
        {
            info = new(manager);
        }
        catch (UnauthorizedAccessException ex)
        {
            ThrowTerminatingError(
                new ErrorRecord(ex, string.Empty, ErrorCategory.PermissionDenied, null)
                {
                    ErrorDetails = new(SR.AdminRequiredForOperation)
                });
            throw; // Unreachable

        }
        catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
        {
            ThrowTerminatingError(rec);
            throw; // Unreachable
        }
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
            info.ProxyBypassList = string.Join(",", ProxyBypassList);
            setProxy = true;
        }

        string target = string.Empty;
        if (UserAgent != null)
        {
            setUserAgent = true;
            target += string.Format(SR.SetUserAgent, UserAgent);
        }
        if (setProxy)
        {
            target += string.Format(SR.SetProxy, info.ProxyAccess, info.ProxyName, info.ProxyPortNumber, info.ProxyBypassLocal, info.ProxyBypassList);
        }

        if ((setProxy || setUserAgent) && ShouldProcess(target))
        {
            try
            {
                if (setProxy)
                {
                    manager.SetProxy(info.ProxyAccess,
                                     info.ProxyBypassLocal ? 1 : 0,
                                     info.ProxyPortNumber,
                                     info.ProxyName,
                                     info.ProxyBypassList);
                }
                if (setUserAgent)
                {
                    manager.UserAgent = UserAgent;
                }
            }
            catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
            {
                ThrowTerminatingError(rec);
                throw; // Unreachable
            }
        }
    }
}
