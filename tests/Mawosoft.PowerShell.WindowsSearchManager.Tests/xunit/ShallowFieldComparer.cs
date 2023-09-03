// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class ShallowFieldComparer : IEqualityComparer, IEqualityComparer<object>
{
    public static readonly ShallowFieldComparer Instance = new();

    private ShallowFieldComparer() { }

    public new bool Equals(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        Type t = x.GetType();
        if (t != y.GetType()) return false;
        FieldInfo[] fields = t.GetFields(BindingFlags.Public
                                         | BindingFlags.NonPublic
                                         | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            object? fx = field.GetValue(x);
            object? fy = field.GetValue(y);
            if (fx is not null)
            {
                if (fy is null) return false;
                if (!fx.Equals(fy)) return false;
            }
            else if (fy is not null)
            {
                return false;
            }
        }
        return true;
    }

    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "Only used for Asser.Equal().")]
    [ExcludeFromCodeCoverage]
    public int GetHashCode(object obj) => throw new NotImplementedException();
}
