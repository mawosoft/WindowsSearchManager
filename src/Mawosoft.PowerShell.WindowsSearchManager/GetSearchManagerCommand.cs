// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Gets global settings and version information for Windows Search.
/// </summary>
/// <remarks>
/// Requires admin rights.
/// </remarks>
[Cmdlet(VerbsCommon.Get, Nouns.SearchManager, ConfirmImpact = ConfirmImpact.None)]
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

        }
        catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
        {
            ThrowTerminatingError(rec);
        }
    }
}
