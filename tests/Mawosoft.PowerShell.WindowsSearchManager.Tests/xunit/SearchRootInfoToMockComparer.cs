// Copyright (c) 2023-2024 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchRootInfoToMockComparer : IEqualityComparer, IEqualityComparer<object>
{
    public static readonly SearchRootInfoToMockComparer Instance = new();

    private SearchRootInfoToMockComparer() { }

    public new bool Equals(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        SearchRootInfo? info;
        MockSearchRoot? mock;
        if ((info = x as SearchRootInfo) is null)
        {
            if ((info = y as SearchRootInfo) is null) return x.Equals(y);
            mock = x as MockSearchRoot;
        }
        else
        {
            mock = y as MockSearchRoot;
        }
        if (mock is null) return x.Equals(y);

        if (info.Schedule is not null) return false;
        if (mock.RootURL != info.Path) return false;
        if (mock.IsHierarchical != 0 != info.IsHierarchical) return false;
        if (mock.ProvidesNotifications != 0 != info.ProvidesNotifications) return false;
        if (mock.UseNotificationsOnly != 0 != info.UseNotificationsOnly) return false;
        if (mock.EnumerationDepth != info.EnumerationDepth) return false;
        if (mock.HostDepth != info.HostDepth) return false;
        if (mock.FollowDirectories != 0 != info.FollowDirectories) return false;
        if (mock.AuthenticationType != info.AuthenticationType) return false;
        if (info.User is not null) return false;
        if (info.Password is not null) return false;
        return true;
    }

    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "Only used for Asser.Equal().")]
    [ExcludeFromCodeCoverage]
    public int GetHashCode(object obj) => throw new NotImplementedException();
}
