// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class CommonCommandTests : CommandTestBase
{

    [Fact]
    public void CommonTests_TheoryData_AllCommands_Complete()
    {
        HashSet<string?> expectedCommandNames = new(AllCommands.ConvertAll(vt => vt.Name));
        MethodInfo m = ((Action<string, ConfirmImpact>)ConfirmImpact_Matches_SupportsShouldProcess).GetMethodInfo();
        IEnumerable<string?> actualCommandNames = m.GetCustomAttributes<DataAttribute>()
            .Select(d => d.GetData(m).First().First() as string);
        IEnumerable<string?> missing = expectedCommandNames.Except(actualCommandNames);
        Assert.Empty(missing);
    }

    [Fact]
    public void CommonTests_TheoryData_CommandsSupportingShouldProcess_Complete()
    {
        HashSet<string?> expectedCommandNames = new(CommandsSupportingShouldProcess.ConvertAll(vt => vt.Name));
        MethodInfo m = ((Delegate)WhatIf_Succeeds).GetMethodInfo();
        IEnumerable<string?> actualCommandNames = m.GetCustomAttributes<DataAttribute>()
            .Select(d => (d.GetData(m).First().First() as string)?.Split(' ').First())
            .Distinct();
        IEnumerable<string?> missing = expectedCommandNames.Except(actualCommandNames);
        Assert.Empty(missing);
        IEnumerable<string?> unexpected = actualCommandNames.Except(expectedCommandNames);
        Assert.Empty(unexpected);
    }

    [Theory]
    [InlineData("Get-SearchManager", ConfirmImpact.None)]
    [InlineData("Set-SearchManager", ConfirmImpact.Medium)]
    [InlineData("Get-SearchCatalog", ConfirmImpact.None)]
    [InlineData("New-SearchCatalog", ConfirmImpact.Low)]
    [InlineData("Remove-SearchCatalog", ConfirmImpact.High)]
    [InlineData("Reset-SearchCatalog", ConfirmImpact.Medium)]
    [InlineData("Set-SearchCatalog", ConfirmImpact.Medium)]
    [InlineData("Update-SearchCatalog", ConfirmImpact.Medium)]
    [InlineData("Add-SearchRoot", ConfirmImpact.Medium)]
    [InlineData("Get-SearchRoot", ConfirmImpact.None)]
    [InlineData("Remove-SearchRoot", ConfirmImpact.High)]
    [InlineData("Add-SearchRule", ConfirmImpact.Medium)]
    [InlineData("Get-SearchRule", ConfirmImpact.None)]
    [InlineData("Remove-SearchRule", ConfirmImpact.Medium)]
    [InlineData("Reset-SearchRule", ConfirmImpact.High)]
    [InlineData("Test-SearchRule", ConfirmImpact.None)]
    public void ConfirmImpact_Matches_SupportsShouldProcess(string commandName, ConfirmImpact confirmImpact)
    {
        Type commandType = AllCommands.Find(vt => vt.Name == commandName).Type;
        Assert.NotNull(commandType);
        CmdletAttribute a = commandType.GetCustomAttribute<CmdletAttribute>()!;
        Assert.NotNull(a);
        Assert.Equal(confirmImpact, a.ConfirmImpact);
        Assert.Equal(confirmImpact != ConfirmImpact.None, a.SupportsShouldProcess);
    }

    [Theory]
    [InlineData("Set-SearchManager -UserAgent foo-agent ")]
    [InlineData("Set-SearchManager -ProxyAccess PROXY_ACCESS_DIRECT ")]
    [InlineData("Set-SearchManager -UserAgent foo-agent -ProxyAccess PROXY_ACCESS_DIRECT ")]
    [InlineData("Set-SearchManager -ProxyAccess PROXY_ACCESS_PROXY -ProxyName bar.com -ProxyPortNumber 0x8080 -ProxyBypassLocal -ProxyBypassList buzz.com,baz.org ")]
    [InlineData("New-SearchCatalog newcat ")]
    [InlineData("Remove-SearchCatalog oldcat ")]
    [InlineData("Reset-SearchCatalog ")]
    [InlineData("Set-SearchCatalog -ConnectTimeout 100 ")]
    [InlineData("Set-SearchCatalog -DataTimeout 100 ")]
    [InlineData("Set-SearchCatalog -DiacriticSensitivity ")]
    [InlineData("Set-SearchCatalog -ConnectTimeout 100 -DataTimeout 100 -DiacriticSensitivity ")]
    [InlineData("Update-SearchCatalog ")]
    [InlineData("Update-SearchCatalog -RootPath x:\\ ")]
    [InlineData("Update-SearchCatalog -Path x:\\foo ")]
    [InlineData("Add-SearchRoot x:\\ ")]
    [InlineData("Add-SearchRoot -InputObject @{ Path = 'x:\\' } ")]
    [InlineData("Remove-SearchRoot x:\\ ")]
    [InlineData("Add-SearchRule x:\\foo -RuleType Exclude ")]
    [InlineData("Add-SearchRule -InputObject @{ Path = 'x:\\foo'; RuleType = 'Exclude' } ")]
    [InlineData("Remove-SearchRule x:\\foo ")]
    [InlineData("Remove-SearchRule x:\\foo Default ")]
    [InlineData("Reset-SearchRule ")]
    public void WhatIf_Succeeds(string command)
    {
        Collection<PSObject> results = InvokeScript(command + " -WhatIf ");
        Assert.False(InterfaceChain.HasWriteRecordings());
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
    }
}
