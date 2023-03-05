// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class DefaultSearchManagerFactoryTests
{
    [Fact]
    public void Instance_NotNull()
    {
        Assert.NotNull(DefaultSearchManagerFactory.Instance);
    }

    [SkippableFact(SkipCondition.WSearchDisabled)]
    public void CreateSearchManager_WSearchEnabled_Succeeds()
    {
        Assert.NotNull(DefaultSearchManagerFactory.Instance.CreateSearchManager());
    }

    [SkippableFact(SkipCondition.WSearchEnabled)]
    public void CreateSearchManager_WSearchDisabled_Throws()
    {
        COMException ex = Assert.Throws<COMException>(DefaultSearchManagerFactory.Instance.CreateSearchManager);
        Assert.Equal(unchecked((int)0x80070422), ex.HResult); // Service disabled
        Assert.Contains("7D096C5F-AC08-4F1F-BEB7-5C22C517CE39", ex.Message);
    }

    [Fact]
    public void CreateSearchRegistryProvider_Succeeds()
    {
        ISearchRegistryProvider srp = DefaultSearchManagerFactory.Instance.CreateSearchRegistryProvider();
        if (RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework", StringComparison.OrdinalIgnoreCase))
        {
            Assert.IsType<SearchRegistryProviderPSDesktop>(srp);
        }
        else
        {
            Assert.IsType<SearchRegistryProviderPSCore>(srp);
        }

    }
}
