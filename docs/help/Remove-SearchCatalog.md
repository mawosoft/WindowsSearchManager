---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://mawosoft.github.io/WindowsSearchManager/reference/Remove-SearchCatalog.html
schema: 2.0.0
---

# Remove-SearchCatalog

## SYNOPSIS

Deletes the specified search catalog.

## SYNTAX

```
Remove-SearchCatalog [-Catalog] <String> [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

The `Remove-SearchCatalog` cmdlet deletes the catalog with the specified name.

## EXAMPLES

### Example 1: Delete a catalog

```powershell
Remove-SearchCatalog -Catalog Sample01
```

```output
Confirm
Are you sure you want to perform this action?
Performing the operation "Remove-SearchCatalog" on target "Sample01".
[Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "Y"):
```

This command deletes the search catalog named `Sample01` after prompting the user for confirmation.

### Example 2: Delete a catalog without confirmation

```powershell
Remove-SearchCatalog -Catalog Sample01 -Confirm:$false
```

This command deletes the search catalog named `Sample01` without prompting for confirmation.

## PARAMETERS

### -Catalog

Specifies the name of the catalog to delete.

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

## RELATED LINKS

[Get-SearchCatalog](Get-SearchCatalog.md)

[New-SearchCatalog](New-SearchCatalog.md)

[Reset-SearchCatalog](Reset-SearchCatalog.md)

[Set-SearchCatalog](Set-SearchCatalog.md)

[Update-SearchCatalog](Update-SearchCatalog.md)
