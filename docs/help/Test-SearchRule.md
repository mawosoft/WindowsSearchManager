---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://github.com/mawosoft/WinSearchManager/blob/master/docs/help/Test-SearchRule.md
schema: 2.0.0
---

# Test-SearchRule

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

### IncludedParameterSet (Default)
```
Test-SearchRule [-Path] <String[]> [-IsIncluded] [-Catalog <String>] [<CommonParameters>]
```

### ChildScopeParameterSet
```
Test-SearchRule [-Path] <String[]> [-HasChildScope] [-Catalog <String>] [<CommonParameters>]
```

### ParentScopeParameterSet
```
Test-SearchRule [-Path] <String[]> [-HasParentScope] [-Catalog <String>] [<CommonParameters>]
```

### DetailedParameterSet
```
Test-SearchRule [-Path] <String[]> [-Detailed] [-Catalog <String>] [<CommonParameters>]
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
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Detailed
{{ Fill Detailed Description }}

```yaml
Type: SwitchParameter
Parameter Sets: DetailedParameterSet
Aliases:

Required: True
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -HasChildScope
{{ Fill HasChildScope Description }}

```yaml
Type: SwitchParameter
Parameter Sets: ChildScopeParameterSet
Aliases:

Required: True
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -HasParentScope
{{ Fill HasParentScope Description }}

```yaml
Type: SwitchParameter
Parameter Sets: ParentScopeParameterSet
Aliases:

Required: True
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -IsIncluded
{{ Fill IsIncluded Description }}

```yaml
Type: SwitchParameter
Parameter Sets: IncludedParameterSet
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
Type: String[]
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String[]
## OUTPUTS

### System.Boolean
### Mawosoft.PowerShell.WindowsSearchManager.TestSearchRuleInfo
## NOTES

## RELATED LINKS
