// Copyright (c) 2023 Matthias Wolf, Mawosoft.

using System.Diagnostics.CodeAnalysis;
using System.Management.Automation.Host;

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

// NotImplementedException: the Mock is missing a implementation for this member.
// Internal members are used to setup mock behavior.
// See also https://github.com/PowerShell/PowerShell/blob/master/src/System.Management.Automation/engine/DefaultCommandRuntime.cs
public class MockCommandRuntime : ICommandRuntime
{
    internal List<object?> Outputs { get; } = new();
    internal List<ErrorRecord> Errors { get; } = new();

#if !NETFRAMEWORK
    [DoesNotReturn]
#endif
    public void ThrowTerminatingError(ErrorRecord errorRecord)
    {
        Assert.NotNull(errorRecord);
        Errors.Add(errorRecord);
        throw errorRecord.Exception ?? new InvalidOperationException(errorRecord.ToString());
    }

    public void WriteError(ErrorRecord errorRecord)
    {
        Assert.NotNull(errorRecord);
        Errors.Add(errorRecord);
    }

    public void WriteObject(object? sendToPipeline) => Outputs.Add(sendToPipeline);

    public PSTransactionContext CurrentPSTransaction => throw new NotImplementedException();
    public PSHost Host => throw new NotImplementedException();
    public bool ShouldContinue(string? query, string? caption) => throw new NotImplementedException();
    public bool ShouldContinue(string? query, string? caption, ref bool yesToAll, ref bool noToAll) => throw new NotImplementedException();
    public bool ShouldProcess(string? target) => throw new NotImplementedException();
    public bool ShouldProcess(string? target, string? action) => throw new NotImplementedException();
    public bool ShouldProcess(string? verboseDescription, string? verboseWarning, string? caption) => throw new NotImplementedException();
    public bool ShouldProcess(string? verboseDescription, string? verboseWarning, string? caption, out ShouldProcessReason shouldProcessReason) => throw new NotImplementedException();

    public bool TransactionAvailable() => throw new NotImplementedException();
    public void WriteCommandDetail(string text) => throw new NotImplementedException();
    public void WriteDebug(string text) => throw new NotImplementedException();
    public void WriteObject(object? sendToPipeline, bool enumerateCollection) => throw new NotImplementedException();
    public void WriteProgress(long sourceId, ProgressRecord progressRecord) => throw new NotImplementedException();
    public void WriteProgress(ProgressRecord progressRecord) => throw new NotImplementedException();
    public void WriteVerbose(string text) => throw new NotImplementedException();
    public void WriteWarning(string text) => throw new NotImplementedException();
}
