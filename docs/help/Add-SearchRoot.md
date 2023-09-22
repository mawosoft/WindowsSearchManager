---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://mawosoft.github.io/WindowsSearchManager/reference/Add-SearchRoot.html
schema: 2.0.0
---

# Add-SearchRoot

## SYNOPSIS

Adds search roots to a search catalog.

## SYNTAX

### PathParameterSet (Default)
```
Add-SearchRoot [-Path] <String[]> [-Catalog <String>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### InputParameterSet
```
Add-SearchRoot -InputObject <SearchRootInfo[]> [-Catalog <String>] [-WhatIf] [-Confirm] [<CommonParameters>]
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
Position: Named
Default value: SystemIndex
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
{{ Fill InputObject Description }}

```yaml
Type: Mawosoft.PowerShell.WindowsSearchManager.SearchRootInfo[]
Parameter Sets: InputParameterSet
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
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
Accept pipeline input: True (ByValue)
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

### Mawosoft.PowerShell.WindowsSearchManager.SearchRootInfo[]

You can pipe **SearchRootInfo** objects to this cmdlet.

## OUTPUTS

### None

This cmdlet returns no output.

## NOTES

## RELATED LINKS

[Get-SearchRoot](Get-SearchRoot.md)

[Remove-SearchRoot](Remove-SearchRoot.md)
