---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://mawosoft.github.io/WindowsSearchManager/reference/Add-SearchRule.html
schema: 2.0.0
---

# Add-SearchRule

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

### PathParameterSet (Default)
```
Add-SearchRule [-Path] <String[]> [[-RuleSet] <SearchRuleSet>] [-RuleType] <SearchRuleType> [-OverrideChildren]
 [[-Catalog] <String>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### InputParameterSet
```
Add-SearchRule -InputObject <SearchRuleInfo[]> [[-Catalog] <String>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
{{ Fill in the Description }}

## EXAMPLES

### Example 1
```
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
Position: 3
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
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
{{ Fill InputObject Description }}

```yaml
Type: Mawosoft.PowerShell.WindowsSearchManager.SearchRuleInfo[]
Parameter Sets: InputParameterSet
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -OverrideChildren
{{ Fill OverrideChildren Description }}

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: PathParameterSet
Aliases:

Required: False
Position: Named
Default value: False
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
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -RuleSet
{{ Fill RuleSet Description }}

```yaml
Type: Mawosoft.PowerShell.WindowsSearchManager.SearchRuleInfo+SearchRuleSet
Parameter Sets: PathParameterSet
Aliases:
Accepted values: User, Default

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RuleType
{{ Fill RuleType Description }}

```yaml
Type: Mawosoft.PowerShell.WindowsSearchManager.SearchRuleInfo+SearchRuleType
Parameter Sets: PathParameterSet
Aliases:
Accepted values: Exclude, Include

Required: True
Position: 2
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
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String[]
### Mawosoft.PowerShell.WindowsSearchManager.SearchRuleInfo[]
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
