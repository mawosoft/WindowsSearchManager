# Module WindowsSearchManager

The WindowsSearchManager module contains cmdlets for managing Windows Search.

## Installation

You can install WindowsSearchManager from the PowerShell Gallery.

```powershell
Install-Module -Name WindowsSearchManager
```

## Cmdlet groups

### SearchManager

Cmdlets for managing global Windows Search settings across catalogs.

[!include[](includes/index-SearchManager.md)]

### SearchCatalog

Cmdlets for managing *search catalogs*. Search catalogs are content indexes.

[!include[](includes/index-SearchCatalog.md)]

### SearchRoot

Cmdlets for managing *search roots*. Search roots are the content stores indexed in a catalog.

[!include[](includes/index-SearchRoot.md)]

### SearchRule

Cmdlets for managing *search rules*. Search rules define which items in a content store are indexed.

[!include[](includes/index-SearchRule.md)]

## See also

[Microsoft's Windows Search documentation](https://learn.microsoft.com/windows/win32/search/windows-search)
