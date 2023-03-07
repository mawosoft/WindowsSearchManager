// Copyright (c) 2023 Matthias Wolf, Mawosoft.

using System.Reflection;

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

internal class ShallowFieldComparer : IEqualityComparer, IEqualityComparer<object>
{
    public static ShallowFieldComparer Instance = new();

    private ShallowFieldComparer() { }

    public new bool Equals(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;
        Type t = x.GetType();
        if (t != y.GetType()) return false;
        FieldInfo[] fields = t.GetFields(BindingFlags.Public
                                         | BindingFlags.NonPublic
                                         | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            object? fx = field.GetValue(x);
            object? fy = field.GetValue(y);
            if (fx != null)
            {
                if (fy == null) return false;
                if (!fx.Equals(fy)) return false;
            }
            else if (fy != null)
            {
                return false;
            }
        }
        return true;
    }

    public int GetHashCode(object obj) => obj?.GetHashCode() ?? 0;
}
