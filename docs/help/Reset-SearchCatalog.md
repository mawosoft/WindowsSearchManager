---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://mawosoft.github.io/WindowsSearchManager/reference/Reset-SearchCatalog.html
schema: 2.0.0
---

# Reset-SearchCatalog

## SYNOPSIS

Resets a search catalog by completely rebuilding its index database.

## SYNTAX

```
Reset-SearchCatalog [[-Catalog] <String>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

The `Reset-SearchCatalog` cmdlet resets a search catalog by completely rebuilding its index database.

> [!NOTE]
> You must run this cmdlet from an elevated PowerShell session. Start PowerShell by using the **Run as administrator** option.

## EXAMPLES

### Example 1: Reset the default catalog.

```powershell
Reset-SearchCatalog
```

This command resets the default Windows Search catalog, which is named **SystemIndex**.

### Example 2: Reset a custom catalog.

```powershell
Reset-SearchCatalog -Catalog Sample01
```

This command resets the search catalog named `Sample01`.

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

The difference between `Reset-SearchCatalog` and `Update-SearchCatalog -All` is that the former removes any old information from the index before reindexing while the latter doesn't.

## RELATED LINKS

[Get-SearchCatalog](Get-SearchCatalog.md)

[New-SearchCatalog](New-SearchCatalog.md)

[Remove-SearchCatalog](Remove-SearchCatalog.md)

[Set-SearchCatalog](Set-SearchCatalog.md)

[Update-SearchCatalog](Update-SearchCatalog.md)
