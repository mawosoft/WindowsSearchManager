// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchCatalogInfoTests
{
    [Fact]
    public void Ctor_Defaults()
    {
        SearchCatalogInfo info = new();
        Assert.Equal(11, info.GetType().GetProperties().Length);
        Assert.Null(info.Catalog);
        Assert.Equal(0u, info.ConnectTimeout);
        Assert.Equal(0u, info.DataTimeout);
        Assert.False(info.DiacriticSensitivity);
        Assert.Equal(_CatalogStatus.CATALOG_STATUS_IDLE, info.Status);
        Assert.Equal(_CatalogPausedReason.CATALOG_PAUSED_REASON_NONE, info.PausedReason);
        Assert.Equal(0, info.ItemCount);
        Assert.Equal(0, info.ItemsToIndexCount);
        Assert.Equal(0, info.NotificationQueueCount);
        Assert.Equal(0, info.HighPriorityQueueCount);
        Assert.Null(info.PathBeingIndexed);
    }

    [Fact]
    public void Ctor_NullArgument_Throws()
    {
        Assert.Throws<ArgumentNullException>("searchCatalog", () => new SearchCatalogInfo(null!));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Ctor_ISearchCatalogManager_Succeeds(bool adminMode)
    {
        MockCatalogManager mock = new()
        {
            AdminMode = adminMode
        };
        SearchCatalogInfo info = new(mock);
        Assert.Equal(mock, info, SearchCatalogInfoToMockComparer.Instance);
    }

    [Fact]
    public void Clone_Succeeds()
    {
        MockCatalogManager mock = new();
        SearchCatalogInfo info = new(mock);
        SearchCatalogInfo clone = (SearchCatalogInfo)info.Clone();
        Assert.Equal(info, clone, ShallowFieldComparer.Instance);
    }
}
