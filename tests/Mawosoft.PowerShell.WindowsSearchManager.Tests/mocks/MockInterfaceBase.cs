// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

// Base class for mock implementations of SearchAPI interfaces.
public abstract class MockInterfaceBase
{
    // Info about a call to an interface method. Properties are methods starting with 'get_' or 'set_'.
    public class CallInfo
    {
        public string MethodName { get; }
        public IReadOnlyList<object?> Parameters { get; }
        public bool IsReadOnly { get; }
        public CallInfo(string methodName, object?[] parameters, bool isReadOnly)
        {
            MethodName = methodName;
            Parameters = parameters;
            IsReadOnly = isReadOnly;
        }
        public override string ToString() => $"{MethodName}({string.Join(",", Parameters)})";
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
    // ExceptionsToThrow and it is possible to force a null value to be returned. The Interface method should call GetChildInterface, to
    // obtain the value or trigger an exception.
    [SuppressMessage("Naming", "CA1721:Property names should not match get methods", Justification = "Internal properties used for mock setup")]
    internal object? ChildInterface { get; set; }

    // List of exceptions the mock should throw for corresponding methods.
    internal List<ExceptionInfo> ExceptionsToThrow { get; } = new List<ExceptionInfo>();

    // List of recorded calls to interface methods.
    internal List<CallInfo> RecordedCallInfos { get; } = new List<CallInfo>();

    // Strings should be sufficient for everything except AddRoot(CSearchRoot).
    internal List<string> RecordedCalls => RecordedCallInfos.ConvertAll(c => c.ToString());

    // Has any recorded calls?
    internal bool HasRecordings => RecordedCallInfos.Count > 0;

    // Has recorded writes, i.e. methods that change something.
    internal bool HasWriteRecordings => RecordedCallInfos.Find(c => !c.IsReadOnly) is not null;

    // Disable recording (and exception throwing). Mostly for debugging purposes to avoid recording debugger access to variables.
    internal bool RecordingDisabled { get; set; }

    // Shortcut for adding an exception to throw.
    internal void AddException(string methodRegex, Exception exception) => ExceptionsToThrow.Add(new ExceptionInfo(methodRegex, exception));

    // To be called from each interface method (and property getters/setters) whose calls should be recorded and/or which should throw an exception.
    // The method name itself is taken from the stack frame, but parameter values need to be passed. If the parameter value is not a string or value type,
    // consider passing a string representing the current value instead.
    // Note:
    // This is trickier than expected. The method name on the stackframe can be wrong with tail call optimization. The attribute [CallerMemberName]
    // would work in such a situation, but a) doesn't distinguish between property getters and setters, and b) doesn't work with 'params' parameters.
    // We could probably live with b) and drop 'params', but losing the getter/setter info is a no-go.
    // Solution: If calling Record() is the last/only call, follow it with a call to TailCall();

    protected void RecordRead(params object?[] parameters) => Record(parameters, isReadOnly: true);
    protected void RecordWrite(params object?[] parameters) => Record(parameters, isReadOnly: false);
    private void Record(object?[] parameters, bool isReadOnly)
    {
        if (RecordingDisabled) return;
        parameters ??= new object?[] { null };
        StackFrame frame = new(2); // 0 = Record, 1 = RecordRead/Write, 2 = caller
        string methodName = frame.GetMethod()?.Name ?? string.Empty;
        RecordedCallInfos.Add(new CallInfo(methodName, parameters, isReadOnly));
        ExceptionInfo? info = ExceptionsToThrow.Find(e => Regex.IsMatch(methodName, e.MethodRegex));
        if (info is not null)
        {
            throw info.Exception;
        }
        if (!AdminMode && AdminMethodRegex is not null && Regex.IsMatch(methodName, AdminMethodRegex))
        {
            throw new UnauthorizedAccessException();
        }
    }

    // Call to avoid misleading stack frame for Record() due to tail call optimization.
    // Seems to also work w/o the NoInlining option, resulting in 'call Record' followed by 'nop', but
    // just to be on the save side...
    [MethodImpl(MethodImplOptions.NoInlining)]
    protected static void TailCall() { }

    [SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "Triggers exception.")]
    protected object? GetChildInterface()
    {
        if (ChildInterface is Exception ex) throw ex;
        return ChildInterface;
    }
}
