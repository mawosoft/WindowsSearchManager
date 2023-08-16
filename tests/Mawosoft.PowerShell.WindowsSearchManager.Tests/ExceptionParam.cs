// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Not needed.")]
public struct ExceptionParam
{
    public const int MSS_E_CATALOGNOTFOUND = unchecked((int)0x80042103);
    public const int OLEDB_BINDER_CUSTOM_ERROR = unchecked((int)0x80042500);
    public const int GTHR_E_SINGLE_THREADED_EMBEDDING = unchecked((int)0x80040DA5);

    internal static readonly List<int> s_searchApiHResults = new() { MSS_E_CATALOGNOTFOUND, OLEDB_BINDER_CUSTOM_ERROR, GTHR_E_SINGLE_THREADED_EMBEDDING };

    public Exception Exception { get; }
    public ExceptionParam(Exception exception) => Exception = exception;
    public readonly bool IsSearchApi => Exception is COMException && s_searchApiHResults.Contains(Exception.HResult);
    public override readonly string? ToString() => Exception is null
        ? null
        : $"{Exception.GetType().Name}(0x{Exception.HResult:X8}{(IsSearchApi ? ",SearchApi" : "")})";
}
