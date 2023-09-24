---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://mawosoft.github.io/WindowsSearchManager/reference/Get-SearchRule.html
schema: 2.0.0
---

# Get-SearchRule

## SYNOPSIS

Gets the search rules in effect for a search catalog.

## SYNTAX

```
Get-SearchRule [[-Catalog] <String>] [<CommonParameters>]
```

## DESCRIPTION

The `Get-SearchRule` cmdlet gets the search rules in effect for a search catalog.

## EXAMPLES

### Example 1: Get search rules

```powershell
Get-SearchRule
```

```output
Path                                                         RuleType RuleSet         FollowFlags
----                                                         -------- -------         -----------
csc://{S-1-5-21-3419697060-3810377854-678604692-1001}/        Include Default FF_INDEXCOMPLEXURLS
file:///*\$RECYCLE.BIN\                                       Exclude Default FF_INDEXCOMPLEXURLS
file:///*\DfsrPrivate\                                        Exclude Default FF_INDEXCOMPLEXURLS
file:///*\System Volume Information\                          Exclude Default FF_INDEXCOMPLEXURLS
file:///C:\ProgramData\Microsoft\Windows\Start Menu\          Include Default FF_INDEXCOMPLEXURLS
file:///C:\Users\                                             Include Default FF_INDEXCOMPLEXURLS
file:///C:\Users\*\AppData\                                   Exclude Default FF_INDEXCOMPLEXURLS
file:///C:\Users\Bob\.*\                                      Exclude Default FF_INDEXCOMPLEXURLS
file:///C:\Users\Bob\Documents\FooData\                       Exclude    User FF_INDEXCOMPLEXURLS
file:///C:\Users\Bob\Documents\FooData\Common\                Include    User FF_INDEXCOMPLEXURLS
file:///C:\Windows.*\                                         Exclude Default FF_INDEXCOMPLEXURLS
file:///C:\Windows\*\temp\                                    Exclude Default FF_INDEXCOMPLEXURLS
iehistory://{S-1-5-21-3419697060-3810377854-678604692-1001}/  Include Default FF_INDEXCOMPLEXURLS
winrt://{S-1-5-21-3419697060-3810377854-678604692-1001}/      Include Default FF_INDEXCOMPLEXURLS
```

This command gets the search rules in effect for the default Windows Search catalog.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

You can't pipe objects to this cmdlet.

## OUTPUTS

### Mawosoft.PowerShell.WindowsSearchManager.SearchRuleInfo

This cmdlet returns **SearchRuleInfo** objects.

## NOTES

To learn more about search rules, see [Managing Scope Rules](https://learn.microsoft.com/windows/win32/search/-search-3x-wds-extidx-csm-scoperules) in Microsoft's Windows Search documentation.

## RELATED LINKS

[Add-SearchRule](Add-SearchRule.md)

[Remove-SearchRule](Remove-SearchRule.md)

[Reset-SearchRule](Reset-SearchRule.md)

[Test-SearchRule](Test-SearchRule.md)
