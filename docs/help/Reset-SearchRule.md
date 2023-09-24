---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://mawosoft.github.io/WindowsSearchManager/reference/Reset-SearchRule.html
schema: 2.0.0
---

# Reset-SearchRule

## SYNOPSIS

Resets a catalog to the default search rules.

## SYNTAX

```
Reset-SearchRule [[-Catalog] <String>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

The `Reset-SearchRule` cmdlet removes all `User` rules from a catalog and restores all `Default` rules to the working rule set.

## EXAMPLES

### Example 1: Reset search rules

```powershell
Reset-SearchRule
```

```output
Confirm
Are you sure you want to perform this action?
Performing the operation "Reset-SearchRule" on target "SystemIndex".
[Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "Y"):
```

This command resets the search rules of the default Windows Search catalog after prompting the user for confirmation.

### Example 2: Reset search rules without confirmation

```powershell
Reset-SearchRule -Confirm:$false
```

This command resets the search rules of the default Windows Search catalog without prompting for confirmation.

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

To learn more about search rules, see [Managing Scope Rules](https://learn.microsoft.com/windows/win32/search/-search-3x-wds-extidx-csm-scoperules) in Microsoft's Windows Search documentation.

## RELATED LINKS

[Add-SearchRule](Add-SearchRule.md)

[Get-SearchRule](Get-SearchRule.md)

[Remove-SearchRule](Remove-SearchRule.md)

[Test-SearchRule](Test-SearchRule.md)
