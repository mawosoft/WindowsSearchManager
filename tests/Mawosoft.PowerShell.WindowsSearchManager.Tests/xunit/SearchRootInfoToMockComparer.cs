// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchRootInfoToMockComparer : IEqualityComparer, IEqualityComparer<object>
{
    public static readonly SearchRootInfoToMockComparer Instance = new();

    private SearchRootInfoToMockComparer() { }

    [SuppressMessage("Style", "IDE0019:Use pattern matching", Justification = "Does not work for this use case.")]
    public new bool Equals(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;
        SearchRootInfo? info;
        MockSearchRoot? mock;
        if ((info = x as SearchRootInfo) == null)
        {
            if ((info = y as SearchRootInfo) == null) return x.Equals(y);
            mock = x as MockSearchRoot;
        }
        else
        {
            mock = y as MockSearchRoot;
        }
        if (mock == null) return x.Equals(y);

        if (info.Schedule != null) return false;
        if (mock.RootURL != info.Path) return false;
        if (mock.IsHierarchical != 0 != info.IsHierarchical) return false;
        if (mock.ProvidesNotifications != 0 != info.ProvidesNotifications) return false;
        if (mock.UseNotificationsOnly != 0 != info.UseNotificationsOnly) return false;
        if (mock.EnumerationDepth != info.EnumerationDepth) return false;
        if (mock.HostDepth != info.HostDepth) return false;
        if (mock.FollowDirectories != 0 != info.FollowDirectories) return false;
        if (mock.AuthenticationType != info.AuthenticationType) return false;
        if (info.User != null) return false;
        if (info.Password != null) return false;
        return true;
    }

    public int GetHashCode(object obj) => obj?.GetHashCode() ?? 0;
}
