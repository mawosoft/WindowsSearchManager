---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://mawosoft.github.io/WindowsSearchManager/reference/Get-SearchCatalog.html
schema: 2.0.0
---

# Get-SearchCatalog

## SYNOPSIS

Gets settings and status of all search catalogs or for a specified one.

## SYNTAX

```
Get-SearchCatalog [[-Catalog] <String>] [<CommonParameters>]
```

## DESCRIPTION

The `Get-SearchCatalog` cmdlet gets settings and status of all search catalogs or for a specified one.

## EXAMPLES

### Example 1: Get settings for all search catalogs

```powershell
Get-SearchCatalog
```

```output
Catalog                : SystemIndex
ConnectTimeout         : 0
DataTimeout            : 0
DiacriticSensitivity   : False
Status                 : CATALOG_STATUS_PROCESSING_NOTIFICATIONS
PausedReason           : CATALOG_PAUSED_REASON_NONE
ItemCount              : 23930
ItemsToIndexCount      : 0
NotificationQueueCount : 47
HighPriorityQueueCount : 0
PathBeingIndexed       : file:C:/Users/Bob/Documents/foo.txt
```

While it is possible to create, manage, and query multiple catalogs, Windows Search currently uses only one catalog, which is named **SystemIndex**.

### Example 2: Get settings for a specific catalog

```powershell
Get-SearchCatalog -Catalog SystemIndex
```

## PARAMETERS

### -Catalog

Specifies the name of the catalog to return information about. If omitted, settings and status of all search catalogs are returned.

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

You can't pipe objects to this cmdlet.

## OUTPUTS

### Mawosoft.PowerShell.WindowsSearchManager.SearchCatalogInfo

This cmdlet returns one or more **SearchCatalogInfo** objects.

## NOTES

## RELATED LINKS

[New-SearchCatalog](New-SearchCatalog.md)

[Remove-SearchCatalog](Remove-SearchCatalog.md)

[Reset-SearchCatalog](Reset-SearchCatalog.md)

[Set-SearchCatalog](Set-SearchCatalog.md)

[Update-SearchCatalog](Update-SearchCatalog.md)
