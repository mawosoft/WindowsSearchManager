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
{{ Fill in the Description }}

## EXAMPLES

### Example 1
```powershell
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -All
{{ Fill All Description }}

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
{{ Fill Catalog Description }}

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
{{ Fill Path Description }}

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
{{ Fill RootPath Description }}

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

## OUTPUTS

### None

## NOTES

## RELATED LINKS

[Get-SearchCatalog](Get-SearchCatalog.md)

[New-SearchCatalog](New-SearchCatalog.md)

[Remove-SearchCatalog](Remove-SearchCatalog.md)

[Reset-SearchCatalog](Reset-SearchCatalog.md)

[Set-SearchCatalog](Set-SearchCatalog.md)
