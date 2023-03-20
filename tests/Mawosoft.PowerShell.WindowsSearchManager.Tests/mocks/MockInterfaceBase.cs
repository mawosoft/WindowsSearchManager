﻿// Copyright (c) 2023 Matthias Wolf, Mawosoft.

using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

// Base class for mock implementations of SearchAPI interfaces.
public abstract class MockInterfaceBase
{
    // Info about a call to an interface method. Properties are methods starting with 'get_' or 'set_'.
    public class CallInfo
    {
        public string MethodName { get; }
        public object?[] Parameters { get; }
        public CallInfo(string methodName, object?[] parameters)
        {
            MethodName = methodName;
            Parameters = parameters;
        }
        public override string? ToString() => $"{MethodName}({string.Join(",", Parameters)})";
    }

    // Info about an exception a method matching the regex string should throw.
    public class ExceptionInfo
    {
        public string MethodRegex { get; }
        public Exception Exception { get; }
        public ExceptionInfo(string methodRegex, Exception exception)
        {
            MethodRegex = methodRegex;
            Exception = exception;
        }
    }

    // Indicates if the user is supposed to have admin rights or not.
    internal bool AdminMode { get; set; } = true;

    // Regex string defining the interface methods that require admin rights.
    internal string? AdminMethodRegex { get; set; }

    // Either the default instance of the child interface, null, or an exception to throw if the caller requests a child interface instance.
    // This means the interface method returning the child interface instance (e.g. ISearchManager.GetCatalog) doesn't need to be added to
    // ExceptionsToThrow and it is possible to force a null value to be returned. The Interface methodt should call GetChildInterface, to
    // obtain the value or trigger an exception.
    internal object? ChildInterface { get; set; }

    // List of exceptions the mock should throw for corresponding methods.
    internal List<ExceptionInfo> ExceptionsToThrow { get; } = new List<ExceptionInfo>();

    // List of recorded calls to interface methods.
    internal List<CallInfo> RecordedCalls { get; } = new List<CallInfo>();

    // Disable recording (and exception throwing). Mostly for debugging purposes to avoid recording debugger access to variables.
    internal bool RecordingDisabled { get; set; }

    // Shortcut for adding an exception to throw.
    internal void AddException(string methodRegex, Exception exception) => ExceptionsToThrow.Add(new ExceptionInfo(methodRegex, exception));

    // To be called from each interface method (and property getters/setters) whose calls should be recorded and/or which should throw an exception.
    // The method name itself is taken from the stack frame, but parameter values need to be passed. If the parameter value is not a string or value type,
    // consider passing a string representing the current value instead.
    protected void Record(params object?[] parameters)
    {
        if (RecordingDisabled) return;
        parameters ??= new object?[] { null };
        StackFrame frame = new(1);
        string methodName = frame.GetMethod()?.Name ?? string.Empty;
        RecordedCalls.Add(new CallInfo(methodName, parameters));
        ExceptionInfo? info = ExceptionsToThrow.Find(e => Regex.IsMatch(methodName, e.MethodRegex));
        if (info != null)
        {
            throw info.Exception;
        }
        if (!AdminMode && AdminMethodRegex != null && Regex.IsMatch(methodName, AdminMethodRegex))
        {
            throw new UnauthorizedAccessException();
        }
    }

    protected object? GetChildInterface()
    {
        if (ChildInterface is Exception ex) throw ex;
        return ChildInterface;
    }
}
