// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

public abstract class DefaultCatalogCommandBase : PSCmdlet
{
    [Parameter]
    [ValidateNotNullOrEmpty()]
    public string Catalog { get; set; } = "SystemIndex";
}
