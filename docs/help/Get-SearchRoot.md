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
{{ Fill in the Description }}

## EXAMPLES

### Example 1
```powershell
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

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
{{ Fill PathOnly Description }}

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

## RELATED LINKS

[Add-SearchRoot](Add-SearchRoot.md)

[Remove-SearchRoot](Remove-SearchRoot.md)
