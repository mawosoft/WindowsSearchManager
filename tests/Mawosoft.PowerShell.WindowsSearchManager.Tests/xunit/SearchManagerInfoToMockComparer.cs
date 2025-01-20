// Copyright (c) Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchManagerInfoToMockComparer : IEqualityComparer, IEqualityComparer<object>
{
    public static readonly SearchManagerInfoToMockComparer Instance = new();

    private SearchManagerInfoToMockComparer() { }

    public new bool Equals(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        SearchManagerInfo? info;
        MockSearchManager? mock;
        if ((info = x as SearchManagerInfo) is null)
        {
            if ((info = y as SearchManagerInfo) is null) return x.Equals(y);
            mock = x as MockSearchManager;
        }
        else
        {
            mock = y as MockSearchManager;
        }
        if (mock is null) return x.Equals(y);

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

    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "Only used for Asser.Equal().")]
    [ExcludeFromCodeCoverage]
    public int GetHashCode(object obj) => throw new NotImplementedException();
}
