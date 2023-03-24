// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchManagerInfoToMockComparer : IEqualityComparer, IEqualityComparer<object>
{
    public static readonly SearchManagerInfoToMockComparer Instance = new();

    private SearchManagerInfoToMockComparer() { }

    [SuppressMessage("Style", "IDE0019:Use pattern matching", Justification = "Does not work for this use case.")]
    public new bool Equals(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;
        SearchManagerInfo? info;
        MockSearchManager? mock;
        if ((info = x as SearchManagerInfo) == null)
        {
            if ((info = y as SearchManagerInfo) == null) return x.Equals(y);
            mock = x as MockSearchManager;
        }
        else
        {
            mock = y as MockSearchManager;
        }
        if (mock == null) return x.Equals(y);

        if (mock.IndexerVersionStrInternal != info.Version) return false;
        if (mock.VersionInternal.Major != info.MajorVersion) return false;
        if (mock.VersionInternal.Minor != info.MinorVersion) return false;
        if (mock.UserAgentInternal != info.UserAgent) return false;
        if (mock.UseProxyInternal != info.ProxyAccess) return false;
        if (mock.ProxyNameInternal != info.ProxyName) return false;
        if (mock.PortNumberInternal != info.ProxyPortNumber) return false;
        if (mock.LocalByPassInternal != 0 != info.ProxyBypassLocal) return false;
        if (mock.ByPassListInternal != info.ProxyBypassList) return false;
        return true;
    }

    public int GetHashCode(object obj) => obj?.GetHashCode() ?? 0;
}
