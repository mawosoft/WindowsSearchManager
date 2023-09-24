---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://mawosoft.github.io/WindowsSearchManager/reference/Update-SearchCatalog.html
schema: 2.0.0
---

# Update-SearchCatalog

## SYNOPSIS

Reindexes a search catalog either completely or partially.

## SYNTAX

### AllParameterSet (Default)
```
Update-SearchCatalog [-All] [-Catalog <String>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### RootParameterSet
```
Update-SearchCatalog -RootPath <String[]> [-Catalog <String>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### PathParameterSet
```
Update-SearchCatalog [-Path] <String[]> [-Catalog <String>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

The `Update-SearchCatalog` reindexes a search catalog either completely or partially.

## EXAMPLES

### Example 1: Reindex an entire catalog

```powershell
Update-SearchCatalog -All
```

This command reindexes the entire default Windows Search catalog.

### Example 2: Reindex a search root.

```powershell
Update-SearchCatalog -RootPath file:///C:\
```

This command reindexes the search root drive `C:` in the default Windows Search catalog.

For file system search roots, you can omit the `file:///` protocol prefix.

### Example 3: Reindex matching paths.

```powershell
Update-SearchCatalog -Path file:///C:\Users\*\Documents\*.docx
```

This command reindexes all Microsoft Word files in and below the `Documents` folder of all users in the default Windows Search catalog.

If the specified path is recognizable as a file system path, you can omit the `file:///` protocol prefix.

## PARAMETERS

### -All

Use this parameter to reindex an entire search catalog.

> [!NOTE]
> To use the **All** parameter, you must run this cmdlet from an elevated PowerShell session. Start PowerShell by using the **Run as administrator** option.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: AllParameterSet
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

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

### -Path

Use this parameter to specify an URL or path pattern to reindex all matching items.

```yaml
Type: System.String[]
Parameter Sets: PathParameterSet
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: True
```

### -RootPath

Use this parameter to specify a search root to be reindexed.

```yaml
Type: System.String[]
Parameter Sets: RootParameterSet
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
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

### System.String[]

You can pipe paths to this cmdlet.

## OUTPUTS

### None

This cmdlet returns no output.

## NOTES

The difference between `Reset-SearchCatalog` and `Update-SearchCatalog -All` is that the former removes any old information from the index before reindexing while the latter doesn't.

## RELATED LINKS

[Get-SearchCatalog](Get-SearchCatalog.md)

[New-SearchCatalog](New-SearchCatalog.md)

[Remove-SearchCatalog](Remove-SearchCatalog.md)

[Reset-SearchCatalog](Reset-SearchCatalog.md)

[Set-SearchCatalog](Set-SearchCatalog.md)
