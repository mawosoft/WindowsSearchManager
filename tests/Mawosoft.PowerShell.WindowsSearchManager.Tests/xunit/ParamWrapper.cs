// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class ParamWrapper<T> : IDisposable
{
    public T Value;
    public string? DisplayText;
    public override string? ToString()
        => DisplayText ?? Value?.ToString() ?? "null";
    public ParamWrapper(T value, string? displayText)
    {
        Value = value;
        DisplayText = displayText;
    }

    // Sadly, implicit casting doesn't do any good with Xunit because it uses the parameter type as defined
    // on the test method for display, not the type of the argument (parameter source).
    // public static implicit operator T(ParamWrapper<T> @this) => @this != null ? @this.Value : default!;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "Just forwarding")]
    public void Dispose() => (Value as IDisposable)?.Dispose();
}

public class ExceptionParam : ParamWrapper<Exception>
{
    public ExceptionParam(Exception value) : this(value, null) { }
    public ExceptionParam(Exception value, string? displayText)
        : base(value, value == null ? null : $"{value.GetType().Name}(0x{value.HResult:X8}){(displayText == null ? "" : $" [{displayText}]")}") { }
}
