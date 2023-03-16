// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class ExceptionParam : ParamWrapper<(Exception Exception, bool IsCustom)>
{
    public ExceptionParam(Exception exception) : this(exception, false, null) { }
    public ExceptionParam(Exception exception, bool isCustom) : this(exception, isCustom, null) { }
    public ExceptionParam(Exception exception, string? displayText) : this(exception, false, displayText) { }
    public ExceptionParam(Exception exception, bool isCustom, string? displayText)
        : base((exception, isCustom), exception == null
            ? null
            : $"{exception.GetType().Name}(0x{exception.HResult:X8}{(isCustom ? ",custom" : "")}){(
                displayText == null ? "" : $" [{displayText}]")}")
    { }
}
