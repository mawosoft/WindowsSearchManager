---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://mawosoft.github.io/WindowsSearchManager/reference/Test-SearchRule.html
schema: 2.0.0
---

# Test-SearchRule

## SYNOPSIS

Tests specified paths against the search rules of a search catalog.

## SYNTAX

### IncludedParameterSet (Default)
```
Test-SearchRule [-Path] <String[]> [-IsIncluded] [-Catalog <String>] [<CommonParameters>]
```

### ChildScopeParameterSet
```
Test-SearchRule [-Path] <String[]> -HasChildScope [-Catalog <String>] [<CommonParameters>]
```

### ParentScopeParameterSet
```
Test-SearchRule [-Path] <String[]> -HasParentScope [-Catalog <String>] [<CommonParameters>]
```

### DetailedParameterSet
```
Test-SearchRule [-Path] <String[]> -Detailed [-Catalog <String>] [<CommonParameters>]
```

## DESCRIPTION

The `Test-SearchRule` cmdlet tests specified paths against the search rules of a search catalog.

## EXAMPLES

### Example 1: Test if a path is included

```powershell
Test-SearchRule -Path file:///C:\Users\Bob\Drafts\
```

```output
True
```

This command checks if the specified path is included in the default Windows Search catalog.

For file system paths, you can omit the `file:///` protocol prefix.


### Example 2: Test a path and get detailed results

```powershell
Test-SearchRule -Path C:\Users\Bob\Drafts\ -Detailed
```

```output
Path                  : C:\Users\Bob\Drafts
IsIncluded            : True
Reason                : CLUSIONREASON_DEFAULT
HasChildScope         : True
HasParentScope        : False
ParentScopeVersiondId : 1
```

This command gets details about the inclusion or exclusion of the specified path.

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

### -Detailed

Use this parameter to return detailed results.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: DetailedParameterSet
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HasChildScope

Use this parameter to check if the specified path has child search rules.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: ChildScopeParameterSet
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HasParentScope

Use this parameter to check if the specified path has parent search rules.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: ParentScopeParameterSet
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IsIncluded

Use this parameter to check if the specified path is included in the search catalog.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: IncludedParameterSet
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path

Specifies the URL or path to be tested.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String[]

You can pipe paths to this cmdlet.

## OUTPUTS

### System.Boolean

When you use the **IsIncluded**, **HasChildScope**, or **HasParentScope** parameters, the cmdlet returns a **Boolean** value for each tested path.

### Mawosoft.PowerShell.WindowsSearchManager.TestSearchRuleInfo

When you use the **Detailed** parameter, the cmdlet returns an **TestSearchRuleInfo** object for each tested path.

## NOTES

To learn more about search rules, see [Managing Scope Rules](https://learn.microsoft.com/windows/win32/search/-search-3x-wds-extidx-csm-scoperules) in Microsoft's Windows Search documentation.

## RELATED LINKS

[Add-SearchRule](Add-SearchRule.md)

[Get-SearchRule](Get-SearchRule.md)

[Remove-SearchRule](Remove-SearchRule.md)

[Reset-SearchRule](Reset-SearchRule.md)
