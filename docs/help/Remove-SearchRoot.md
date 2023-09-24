---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://mawosoft.github.io/WindowsSearchManager/reference/Remove-SearchRoot.html
schema: 2.0.0
---

# Remove-SearchRoot

## SYNOPSIS

Removes the specified search roots from a search catalog.

## SYNTAX

```
Remove-SearchRoot [-Path] <String[]> [-Catalog <String>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

The `Remove-SearchRoot` cmdlet removes the specified search roots from a search catalog.

> [!CAUTION]
> Removing a search root also removes all search rules associated with it.

## EXAMPLES

### Example 1: Remove a search root

```powershell
Remove-SearchRoot -Path file:///D:\
```

```output
Confirm
Are you sure you want to perform this action?
Performing the operation "Remove-SearchRoot" on target "SystemIndex Path=file:///D:\".
[Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "Y"):
```

This command removes the search root drive `D:` from the default Windows Search catalog after prompting the user for confirmation.

### Example 2: Remove a search root without confirmation

```powershell
Remove-SearchRoot -Path file:///D:\ -Confirm:$false
```

This command removes the search root drive `D:` from the default Windows Search catalog without prompting for confirmation.

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

Specifies the search root to be removed from a catalog. The specified path must match an existing search root exactly, including the protocol prefix.

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

To learn more about search roots, see [Managing Search Roots](https://learn.microsoft.com/windows/win32/search/-search-3x-wds-extidx-csm-searchroots) in Microsoft's Windows Search documentation.

## RELATED LINKS

[Add-SearchRoot](Add-SearchRoot.md)

[Get-SearchRoot](Get-SearchRoot.md)
