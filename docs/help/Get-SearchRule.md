---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://mawosoft.github.io/WindowsSearchManager/reference/Get-SearchRule.html
schema: 2.0.0
---

# Get-SearchRule

## SYNOPSIS

Gets the search rules in effect for a search catalog.

## SYNTAX

```
Get-SearchRule [[-Catalog] <String>] [<CommonParameters>]
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
{{ Fill Catalog Description }}

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

You can't pipe objects to this cmdlet.

## OUTPUTS

### Mawosoft.PowerShell.WindowsSearchManager.SearchRuleInfo

This cmdlet returns **SearchRuleInfo** objects.

## NOTES

## RELATED LINKS

[Add-SearchRule](Add-SearchRule.md)

[Remove-SearchRule](Remove-SearchRule.md)

[Reset-SearchRule](Reset-SearchRule.md)

[Test-SearchRule](Test-SearchRule.md)
