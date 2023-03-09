// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchCatalogInfoTests
{
    [Fact]
    public void ctor_Defaults()
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
    public void ctor_NullArgument_Throws()
    {
        Assert.Throws<ArgumentNullException>("searchCatalog", () => new SearchCatalogInfo(null!));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ctor_ISearchCatalogManager_Succeeds(bool noAdmin)
    {
        MockCatalogManager mock = new()
        {
            NoAdmin = noAdmin
        };
        SearchCatalogInfo info = new(mock);
        Assert.Equal(mock.Name, info.Catalog);
        Assert.Equal(mock.ConnectTimeout, info.ConnectTimeout);
        Assert.Equal(mock.DataTimeout, info.DataTimeout);
        Assert.Equal(mock.DiacriticSensitivity != 0, info.DiacriticSensitivity);
        Assert.Equal(mock.Status, info.Status);
        Assert.Equal(mock.PausedReason, info.PausedReason);
        Assert.Equal(mock.NumberOfItemsInternal, info.ItemCount);
        Assert.Equal(mock.NumberOfItemsToIndexInternal.Items, info.ItemsToIndexCount);
        Assert.Equal(mock.NumberOfItemsToIndexInternal.Notifications, info.NotificationQueueCount);
        Assert.Equal(mock.NumberOfItemsToIndexInternal.HighPrio, info.HighPriorityQueueCount);
        if (noAdmin)
        {
            Assert.StartsWith("N/A", info.PathBeingIndexed);
        }
        else
        {
            Assert.Equal(mock.URLBeingIndexedInternal, info.PathBeingIndexed);
        }
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
