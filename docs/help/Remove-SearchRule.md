---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://mawosoft.github.io/WindowsSearchManager/reference/Remove-SearchRule.html
schema: 2.0.0
---

# Remove-SearchRule

## SYNOPSIS

Removes the specified search rules from a search catalog.

## SYNTAX

```
Remove-SearchRule [-Path] <String[]> [[-RuleSet] <SearchRuleSet>] [-Catalog <String>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION

The `Remove-SearchRule` cmdlet removes the specified search rules from a search catalog.

## EXAMPLES

### Example 1: Remove a search rule

```powershell
Remove-SearchRule -Path file:///C:\Users\Bob\Documents\FooData\Common\ -RuleSet User
```

This command removes the specified user rule from the working rule set of the default Windows Search catalog.

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

### -Path

Specifies the search rule to be removed from a catalog. The specified path must match an existing search rule exactly, including the protocol prefix.

```yaml
Type: System.String[]
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -RuleSet

Specifies the rule set the rule belongs to. The acceptable values for this parameter are:

- `User` - Removes a user rule from the working rule set. If the user rule is a duplicate of or overrides a default rule, the default rule remains in the working rule set.
- `Default` - Removes a default rule from both the working rule set and the default rule set.

```yaml
Type: Mawosoft.PowerShell.WindowsSearchManager.SearchRuleInfo+SearchRuleSet
Parameter Sets: (All)
Aliases:
Accepted values: User, Default

Required: False
Position: 1
Default value: User
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

To learn more about search rules, see [Managing Scope Rules](https://learn.microsoft.com/windows/win32/search/-search-3x-wds-extidx-csm-scoperules) in Microsoft's Windows Search documentation.

## RELATED LINKS

[Add-SearchRule](Add-SearchRule.md)

[Get-SearchRule](Get-SearchRule.md)

[Reset-SearchRule](Reset-SearchRule.md)

[Test-SearchRule](Test-SearchRule.md)
