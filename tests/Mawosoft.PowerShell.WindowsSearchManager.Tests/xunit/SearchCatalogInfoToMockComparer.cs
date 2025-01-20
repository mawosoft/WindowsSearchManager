// Copyright (c) Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchCatalogInfoToMockComparer : IEqualityComparer, IEqualityComparer<object>
{
    public static readonly SearchCatalogInfoToMockComparer Instance = new();

    private SearchCatalogInfoToMockComparer() { }

    public new bool Equals(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        SearchCatalogInfo? info;
        MockCatalogManager? mock;
        if ((info = x as SearchCatalogInfo) is null)
        {
            if ((info = y as SearchCatalogInfo) is null) return x.Equals(y);
            mock = x as MockCatalogManager;
        }
        else
        {
            mock = y as MockCatalogManager;
        }
        if (mock is null) return x.Equals(y);

        if (mock.NameInternal != info.Catalog) return false;
        if (mock.ConnectTimeoutInternal != info.ConnectTimeout) return false;
        if (mock.DataTimeoutInternal != info.DataTimeout) return false;
        if (mock.DiacriticSensitivityInternal != 0 != info.DiacriticSensitivity) return false;
        if (mock.StatusInternal != info.Status) return false;
        if (mock.PausedReasonInternal != info.PausedReason) return false;
        if (mock.NumberOfItemsInternal != info.ItemCount) return false;
        if (mock.NumberOfItemsToIndexInternal.Items != info.ItemsToIndexCount) return false;
        if (mock.NumberOfItemsToIndexInternal.Notifications != info.NotificationQueueCount) return false;
        if (mock.NumberOfItemsToIndexInternal.HighPrio != info.HighPriorityQueueCount) return false;
        if (mock.AdminMode)
        {
            if (mock.URLBeingIndexedInternal != info.PathBeingIndexed) return false;
        }
        else
        {
            if (info.PathBeingIndexed is null
                || !info.PathBeingIndexed.StartsWith("N/A", StringComparison.Ordinal)) return false;
        }
        return true;
    }

    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "Only used for Asser.Equal().")]
    [ExcludeFromCodeCoverage]
    public int GetHashCode(object obj) => throw new NotImplementedException();
}
