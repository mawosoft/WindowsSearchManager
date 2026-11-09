# Module manifest for module 'WindowsSearchManager'

@{

    RootModule           = 'Mawosoft.PowerShell.WindowsSearchManager.dll'
    ModuleVersion        = '@ModuleVersion@'
    CompatiblePSEditions = @('Desktop', 'Core')
    GUID                 = 'b7f8bc03-e1a2-4294-aed5-02e1bfc15cd8'
    Author               = 'Matthias Wolf'
    CompanyName          = 'mawosoft'
    Copyright            = 'Copyright (c) 2023-2026 Matthias Wolf, Mawosoft. All rights reserved.'
    Description          = 'Powershell module for managing Windows Search. With WindowsSearchManager you can:
- Manage global Windows Search settings across catalogs
- Manage individual search catalogs (content indexes)
- Manage search roots (content stores)
- Manage search rules (what is and what isn''t indexed)'
    PowerShellVersion    = '5.1'
    FunctionsToExport    = @()
    CmdletsToExport      = @(
        'Get-SearchManager'
        'Set-SearchManager'
        'Get-SearchCatalog'
        'New-SearchCatalog'
        'Remove-SearchCatalog'
        'Reset-SearchCatalog'
        'Set-SearchCatalog'
        'Update-SearchCatalog'
        'Add-SearchRoot'
        'Get-SearchRoot'
        'Remove-SearchRoot'
        'Add-SearchRule'
        'Get-SearchRule'
        'Remove-SearchRule'
        'Reset-SearchRule'
        'Test-SearchRule'
    )
    VariablesToExport    = @()
    AliasesToExport      = @()
    DscResourcesToExport = @()
    PrivateData          = @{
        PSData = @{
            Tags                     = @('PSEdition_Desktop', 'PSEdition_Core', 'Windows', 'Search', 'WindowsSearchManager', 'SearchManager')
            LicenseUri               = 'https://github.com/mawosoft/WindowsSearchManager/blob/master/LICENSE'
            ProjectUri               = 'https://github.com/mawosoft/WindowsSearchManager'
            # ReleaseNotes = ''
            Prerelease               = '@Prerelease@'
            RequireLicenseAcceptance = $false
        }
    }
}
