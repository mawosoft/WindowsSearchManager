---
external help file: Mawosoft.PowerShell.WindowsSearchManager.dll-Help.xml
Module Name: WindowsSearchManager
online version: https://mawosoft.github.io/WindowsSearchManager/reference/Set-SearchManager.html
schema: 2.0.0
---

# Set-SearchManager

## SYNOPSIS

Changes global settings for Windows Search.

## SYNTAX

```
Set-SearchManager [-UserAgent <String>] [-ProxyAccess <_PROXY_ACCESS>] [-ProxyName <String>]
 [-ProxyPortNumber <UInt32>] [-ProxyBypassLocal] [-ProxyBypassList <String[]>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION

The `Set-SearchManager` cmdlet changes the global settings of Windows Search across all search catalogs.

> [!NOTE]
> You must run this cmdlet from an elevated PowerShell session. Start PowerShell by using the **Run as administrator** option.

## EXAMPLES

### Example 1: Configure a custom proxy server

```powershell
Set-SearchManager -ProxyAccess PROXY_ACCESS_PROXY -ProxyName proxy.foo.org -ProxyPortNumber 8080

```

This command tells Windows Search to use `proxy.foo.org:8080` as the proxy server.

### Example 2: Configure local addresses to bypass the proxy.

```powershell
Set-SearchManager -ProxyBypassLocal -ProxyBypassList localhost, 127.0.0.1, *.foo.org
```

This command tells Windows Search to bypass the proxy for local addresses and provides a list of those local addresses.

### Example 3: Use Windows settings

```powershell
Set-SearchManager -ProxyAccess PROXY_ACCESS_PRECONFIG
```

This command tells Windows Search to use the settings as configured in Windows Network & Internet Settings.

## PARAMETERS

### -ProxyAccess

Specifies if and how a proxy server is used. The acceptable values for this parameter are:

- `PROXY_ACCESS_PRECONFIG` - Use the Windows Network & Internet Settings.
- `PROXY_ACCESS_DIRECT` - Don't use a proxy.
- `PROXY_ACCESS_PROXY` - Use a proxy as specified by the other **Proxy...** parameters.

```yaml
Type: SearchAPI._PROXY_ACCESS
Parameter Sets: (All)
Aliases:
Accepted values: PROXY_ACCESS_PRECONFIG, PROXY_ACCESS_DIRECT, PROXY_ACCESS_PROXY

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProxyBypassList

Specifies, as a string array, the local addresses for which the proxy server should not be used. Use this parameter in conjunction with the **ProxyBypassLocal** parameter.

```yaml
Type: System.String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProxyBypassLocal

Use this parameter to bypass the proxy server for local addresses. Use the **ProxyBypassList** parameter to specifiy a list of local addresses.

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

### -ProxyName

Specifies the name of the proxy server.

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProxyPortNumber

Specifies the port number of the proxy server.

```yaml
Type: System.UInt32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UserAgent

Specifies the user agent string.

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
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

### None

You can't pipe objects to this cmdlet.

## OUTPUTS

### None

This cmdlet returns no output.

## NOTES

Microsoft's [Windows Search documentation](https://learn.microsoft.com/windows/win32/search/-search-3x-wds-mngidx-searchmanager) states that the settings described above are implemented, but reserved for future use.

## RELATED LINKS

[Get-SearchManager](Get-SearchManager.md)
