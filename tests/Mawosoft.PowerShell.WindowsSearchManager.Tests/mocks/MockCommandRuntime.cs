// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

// NotImplementedException: the Mock is missing a implementation for this member.
// Internal members are used to setup mock behavior.
// See also https://github.com/PowerShell/PowerShell/blob/master/src/System.Management.Automation/engine/DefaultCommandRuntime.cs
[SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "Partial interface implementation.")]
public class MockCommandRuntime : ICommandRuntime
{
    internal List<ErrorRecord> Errors { get; } = new();

#if !NETFRAMEWORK
    [DoesNotReturn]
#endif
    public void ThrowTerminatingError(ErrorRecord errorRecord)
    {
        if (errorRecord is null) throw new ArgumentNullException(nameof(errorRecord));
        Errors.Add(errorRecord);
        throw errorRecord.Exception ?? new InvalidOperationException(errorRecord.ToString());
    }

    [ExcludeFromCodeCoverage] public PSTransactionContext CurrentPSTransaction => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public PSHost Host => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public bool ShouldContinue(string? query, string? caption) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public bool ShouldContinue(string? query, string? caption, ref bool yesToAll, ref bool noToAll) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public bool ShouldProcess(string? target) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public bool ShouldProcess(string? target, string? action) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public bool ShouldProcess(string? verboseDescription, string? verboseWarning, string? caption) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public bool ShouldProcess(string? verboseDescription, string? verboseWarning, string? caption, out ShouldProcessReason shouldProcessReason) => throw new NotImplementedException();

    [ExcludeFromCodeCoverage] public bool TransactionAvailable() => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public void WriteCommandDetail(string text) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public void WriteDebug(string text) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public void WriteError(ErrorRecord errorRecord) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public void WriteObject(object? sendToPipeline) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public void WriteObject(object? sendToPipeline, bool enumerateCollection) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public void WriteProgress(long sourceId, ProgressRecord progressRecord) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public void WriteProgress(ProgressRecord progressRecord) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public void WriteVerbose(string text) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public void WriteWarning(string text) => throw new NotImplementedException();
}
