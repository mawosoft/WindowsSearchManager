---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://mawosoft.github.io/WindowsSearchManager/reference/New-SearchCatalog.html
schema: 2.0.0
---

# New-SearchCatalog

## SYNOPSIS

Creates a new search catalog.

## SYNTAX

```
New-SearchCatalog [-Catalog] <String> [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

The `New-SearchCatalog` cmdlet creates a new search catalog with the specified name.

## EXAMPLES

### Example 1: Create a new catalog

```powershell
New-SearchCatalog -Catalog Sample01
```

This command creates a new search catalog named `Sample01`.

## PARAMETERS

### -Catalog

Specifies the name of the catalog to create.

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases: wi

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

### None

This cmdlet returns no output.

## NOTES

While it is possible to create, manage, and query multiple catalogs, Windows Search currently uses only one catalog, which is named **SystemIndex**.

## RELATED LINKS

[Get-SearchCatalog](Get-SearchCatalog.md)

[Remove-SearchCatalog](Remove-SearchCatalog.md)

[Reset-SearchCatalog](Reset-SearchCatalog.md)

[Set-SearchCatalog](Set-SearchCatalog.md)

[Update-SearchCatalog](Update-SearchCatalog.md)
