---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://mawosoft.github.io/WindowsSearchManager/reference/Set-SearchCatalog.html
schema: 2.0.0
---

# Set-SearchCatalog

## SYNOPSIS

Changes settings for a search catalog.

## SYNTAX

```
Set-SearchCatalog [-ConnectTimeout <UInt32>] [-DataTimeout <UInt32>] [-DiacriticSensitivity]
 [-Catalog <String>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

The `Set-SearchCatalog` cmdlet changes settings for a search catalog.

## EXAMPLES

### Example 1: Enable diacritic sensitivity
```powershell
Set-SearchCatalog -DiacriticSensitivity
```

This command enables diacritic sensitivity for the default Windows Search catalog.

### Example 2: Disable diacritic sensitivity
```powershell
Set-SearchCatalog -DiacriticSensitivity:$false
```

This command disables diacritic sensitivity for the default Windows Search catalog.

## PARAMETERS

### -Catalog

Specifies the name of the catalog this cmdlet operates on. If omitted, this is the default Windows Search catalog, named **SystemIndex**.

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: SystemIndex
Accept pipeline input: False
Accept wildcard characters: False
```

### -ConnectTimeout

Specifies the time, in seconds, that the indexer should wait for a connection response from a server or data store.

> [!NOTE]
> This parameter has currently no effect in Windows Search.

```yaml
Type: System.UInt32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DataTimeout

Specifies the time, in seconds, that the indexer should wait for a data transaction.

> [!NOTE]
> This parameter has currently no effect in Windows Search.

```yaml
Type: System.UInt32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DiacriticSensitivity

Enables diacritic sensitivity. When enabled, the catalog treats words like `resume` and `resumé` as different. When disabled, the catalog treats them as if they were the same word.

To disable diacritic sensitivity, specify the parameter as follows: `-DiacriticSensitivity:$false`.

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

Changing diacritic sensitivity automatically causes the catalog to be reindexed.

## RELATED LINKS

[Get-SearchCatalog](Get-SearchCatalog.md)

[New-SearchCatalog](New-SearchCatalog.md)

[Remove-SearchCatalog](Remove-SearchCatalog.md)

[Reset-SearchCatalog](Reset-SearchCatalog.md)

[Update-SearchCatalog](Update-SearchCatalog.md)
