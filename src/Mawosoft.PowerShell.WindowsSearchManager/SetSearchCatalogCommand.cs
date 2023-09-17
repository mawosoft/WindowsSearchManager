// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Changes settings for a search catalog.
/// </summary>
[Cmdlet(VerbsCommon.Set, Nouns.SearchCatalog, ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
public sealed class SetSearchCatalogCommand : SearchApiCommandBase
{
    [Parameter]
    public uint ConnectTimeout { get; set; }

    [Parameter]
    public uint DataTimeout { get; set; }

    [Parameter]
    public SwitchParameter DiacriticSensitivity { get; set; }

    [Parameter]
    [ValidateNotNullOrEmpty()]
    public string Catalog { get; set; } = DefaultCatalogName;

    protected override void EndProcessing()
    {
        if (ShouldProcess(Catalog))
        {
            ISearchCatalogManager catalog = GetSearchCatalogManager(Catalog);
            try
            {
                if (MyInvocation.BoundParameters.ContainsKey(nameof(ConnectTimeout)))
                {
                    catalog.ConnectTimeout = ConnectTimeout;
                }
                if (MyInvocation.BoundParameters.ContainsKey(nameof(DataTimeout)))
                {
                    catalog.DataTimeout = DataTimeout;
                }
                if (MyInvocation.BoundParameters.ContainsKey(nameof(DiacriticSensitivity)))
                {
                    catalog.DiacriticSensitivity = DiacriticSensitivity ? 1 : 0;
                }
            }
            catch (COMException ex) when (SearchApiErrorHelper.TryWrapCOMException(ex, out ErrorRecord rec))
            {
                ThrowTerminatingError(rec);
            }
        }
    }
}
