// Copyright (c) 2023-2024 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public static class TestDataEnumerableExtensions
{
    public static IEnumerable<object?[]> AsRows<T>(this IEnumerable<T> @this) => @this.Select(o => new object?[] { o });
    public static IEnumerable<object?[]> AsRows<T>(this IEnumerable<T[]> @this) => @this.Select(a => a.Cast<object?>().ToArray());

    public static IEnumerable<object?[]> CrossJoin(this IEnumerable<object?[]> first, IEnumerable<object?[]> second)
        => first.SelectMany(o1 => second.Select(o2 => o1.Concat(o2).ToArray()));

    public static IEnumerable<object?[]> CrossJoin<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second)
        => CrossJoin(first.AsRows(), second.AsRows());

    public static IEnumerable<object?[]> CrossJoin<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2[]> second)
        => CrossJoin(first.AsRows(), second.AsRows());

    public static IEnumerable<object?[]> CrossJoin<T1, T2>(this IEnumerable<T1[]> first, IEnumerable<T2> second)
        => CrossJoin(first.AsRows(), second.AsRows());

    public static IEnumerable<object?[]> CrossJoin<T1, T2>(this IEnumerable<T1[]> first, IEnumerable<T2[]> second)
        => CrossJoin(first.AsRows(), second.AsRows());
}
