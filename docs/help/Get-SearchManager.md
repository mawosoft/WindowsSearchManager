---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://mawosoft.github.io/WindowsSearchManager/reference/Get-SearchManager.html
schema: 2.0.0
---

# Get-SearchManager

## SYNOPSIS

Gets global settings and version information for Windows Search.

## SYNTAX

```
Get-SearchManager [<CommonParameters>]
```

## DESCRIPTION

The `Get-SearchManager` cmdlet gets the global settings and version information for Windows Search.

> [!NOTE]
> You must run this cmdlet from an elevated PowerShell session. Start PowerShell by using the **Run as administrator** option.

## EXAMPLES

### Example 1: Get global Windows Search settings

```powershell
Get-SearchManager
```

```output
Version          : 10.0.19041.3324
MajorVersion     : 10
MinorVersion     : 0
UserAgent        : Mozilla/4.0 (compatible; MSIE 6.0; Windows NT; MS Search 4.0 Robot)
ProxyAccess      : PROXY_ACCESS_PRECONFIG
ProxyName        :
ProxyPortNumber  : 0
ProxyBypassLocal : False
ProxyBypassList  :
```

## PARAMETERS

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

You can't pipe objects to this cmdlet.

## OUTPUTS

### Mawosoft.PowerShell.WindowsSearchManager.SearchManagerInfo

This cmdlet returns a **SearchManagerInfo** object.

## NOTES

## RELATED LINKS

[Set-SearchManager](Set-SearchManager.md)
