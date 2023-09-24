---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://mawosoft.github.io/WindowsSearchManager/reference/Get-SearchRoot.html
schema: 2.0.0
---

# Get-SearchRoot

## SYNOPSIS

Gets all search roots defined for a search catalog.

## SYNTAX

```
Get-SearchRoot [[-Catalog] <String>] [-PathOnly] [<CommonParameters>]
```

## DESCRIPTION

The `Get-SearchRoot` cmdlet gets all search roots defined for a search catalog.

## EXAMPLES

### Example 1: Get all search roots paths

```powershell
Get-SearchRoot -PathOnly
```

```output
csc://{S-1-5-21-3419697060-3810377854-678604692-1001}/
defaultroot://{S-1-5-21-3419697060-3810377854-678604692-1001}/
file:///C:\
iehistory://{S-1-5-21-3419697060-3810377854-678604692-1001}/
winrt://{S-1-5-21-3419697060-3810377854-678604692-1001}/
```

This command gets the search root paths for the default Windows Search catalog.

### Example 2: Get detailed infos for all search roots paths

```powershell
Get-SearchRoot
```

```output
Path                  : csc://{S-1-5-21-3419697060-3810377854-678604692-1001}/
IsHierarchical        : True
ProvidesNotifications : True
UseNotificationsOnly  : False
EnumerationDepth      : 4294967295
HostDepth             : 0
FollowDirectories     : True
AuthenticationType    : eAUTH_TYPE_ANONYMOUS

Path                  : defaultroot://{S-1-5-21-3419697060-3810377854-678604692-1001}/
IsHierarchical        : True
... output truncated ...
```

This command gets the search root details for the default Windows Search catalog.

## PARAMETERS

### -Catalog

Specifies the name of the catalog this cmdlet operates on. If omitted, this is the default Windows Search catalog, named **SystemIndex**.

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: SystemIndex
Accept pipeline input: False
Accept wildcard characters: False
```

### -PathOnly

Use this parameter to return only the paths of the search roots, not the details.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
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

### Mawosoft.PowerShell.WindowsSearchManager.SearchRootInfo

By default, this cmdlet returns **SearchRootInfo** objects.

### System.String

When you use the **PathOnly** parameter, the cmdlet returns the root paths.

## NOTES

To learn more about search roots, see [Managing Search Roots](https://learn.microsoft.com/windows/win32/search/-search-3x-wds-extidx-csm-searchroots) in Microsoft's Windows Search documentation.

## RELATED LINKS

[Add-SearchRoot](Add-SearchRoot.md)

[Remove-SearchRoot](Remove-SearchRoot.md)
