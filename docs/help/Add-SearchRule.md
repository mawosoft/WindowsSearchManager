---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://mawosoft.github.io/WindowsSearchManager/reference/Add-SearchRule.html
schema: 2.0.0
---

# Add-SearchRule

## SYNOPSIS

Adds search rules to a search catalog.

## SYNTAX

### PathParameterSet (Default)
```
Add-SearchRule [-Path] <String[]> [-RuleType] <SearchRuleType> [[-RuleSet] <SearchRuleSet>] [-OverrideChildren]
 [-Catalog <String>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### InputParameterSet
```
Add-SearchRule -InputObject <SearchRuleInfo[]> [-OverrideChildren] [-Catalog <String>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION

The `Add-SearchRule` cmdlet adds search rules to a search catalog.

## EXAMPLES

### Example 1: Add search rules

```powershell
Add-SearchRule -Path file:///C:\Users\Bob\Documents\FooData\ -RuleType Exclude -RuleSet User
Add-SearchRule -Path file:///C:\Users\Bob\Documents\FooData\Common\ -RuleType Include -RuleSet User
```

The first command adds a user exclusion rule to the default Windows Search catalog. The `FooData` directory and all its subdirectories are excluded from the catalog.

The second command amends the exclusion by making an exception for one subdirectory. The subdirectory `Common` is included in the catalog, all other subdirectories of `FooData` are still excluded.

If the specified path is recognizable as a file system path, you can omit the `file:///` protocol prefix.


### Example 2: Override child rules

```powershell
Add-SearchRule -Path file:///C:\Users\Bob\Documents\FooData\ -RuleType Exclude -RuleSet User -OverrideChildren
```

This command adds a user exclusion rule to the default Windows Search catalog. The `FooData` directory and all its subdirectories are excluded from the catalog, any existing child rules are removed.

Without the **OverrideChildren** parameter, child rules, like the one for the subdirectory `Common` in Example 1, would remain active.

### Example 3: Use wildcards in exclusion rules

```powershell
Add-SearchRule -Path file:///C:\Users\*\Documents\PowerShell\ -RuleType Exclude -RuleSet Default
```

This command adds a default exclusion rule to the default Windows Search catalog. The `PowerShell` subdirectory in the `Documents` directory of every user is excluded from the catalog.


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

Specifies **SearchRuleInfo** objects containing search rules with their properties to be added to a search catalog.

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

Use this parameter to remove any child rules when adding a user search rule.

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

### -Path

Specifies the URL or path of the search rule to be added to a search catalog. For exclusion rules, the path can contain the wildcard character `*`.

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

Specifies the rule set the rule belongs to. The acceptable values for this parameter are:

- `User` - Adds a user rule to the working rule set.
- `Default` - Adds a default rule to both the working rule set and the default rule set.

```yaml
Type: Mawosoft.PowerShell.WindowsSearchManager.SearchRuleInfo+SearchRuleSet
Parameter Sets: PathParameterSet
Aliases:
Accepted values: User, Default

Required: False
Position: 2
Default value: User
Accept pipeline input: False
Accept wildcard characters: False
```

### -RuleType

Specifies the type of the rule. The acceptable values for this parameter are:

- `Exclude` - The rule specifies an exclusion.
- `Include` - The rule specifies an inclusion.

```yaml
Type: Mawosoft.PowerShell.WindowsSearchManager.SearchRuleInfo+SearchRuleType
Parameter Sets: PathParameterSet
Aliases:
Accepted values: Exclude, Include

Required: True
Position: 1
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

### System.String[]

You can pipe paths to this cmdlet.

### Mawosoft.PowerShell.WindowsSearchManager.SearchRuleInfo[]

You can pipe **SearchRuleInfo** objects to this cmdlet.

## OUTPUTS

### None

This cmdlet returns no output.

## NOTES

To learn more about search rules, see [Managing Scope Rules](https://learn.microsoft.com/windows/win32/search/-search-3x-wds-extidx-csm-scoperules) in Microsoft's Windows Search documentation.

## RELATED LINKS

[Get-SearchRule](Get-SearchRule.md)

[Remove-SearchRule](Remove-SearchRule.md)

[Reset-SearchRule](Reset-SearchRule.md)

[Test-SearchRule](Test-SearchRule.md)
