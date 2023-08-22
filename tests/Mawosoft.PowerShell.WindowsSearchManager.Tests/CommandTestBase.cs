// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

[Collection(nameof(NoParallelTests))]
public class CommandTestBase
{
    protected static readonly System.Management.Automation.PowerShell PowerShell;
    private protected static readonly List<(Type Type, string Name)> AllCommands;
    private protected static readonly List<(Type Type, string Name)> CommandsSupportingShouldProcess;

    // We already have SearchApiCommandBase.SearchManagerFactory as a static variable and therefore
    // disabled parallel tests. So we can use a static instance of PowerShell as well.
    //
    // This provides a limited InitialSessionState sufficient for testing w/o requiring the full PowerShell SDK.
    // However, user interaction is not possible, and a confirmation prompt will result HostException, which in
    // turn will contain the full prompt message.
    // '-WhatIf' works in so far as ShouldProcess() returns false, but the message is not available.
    [SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "Multiple fields from same source data.")]
    static CommandTestBase()
    {
        AllCommands = new();
        CommandsSupportingShouldProcess = new();
        InitialSessionState iss = InitialSessionState.Create();
        iss.ThreadOptions = PSThreadOptions.UseCurrentThread;
        iss.LanguageMode = PSLanguageMode.FullLanguage;
        foreach (Type t in typeof(SearchApiCommandBase).Assembly.GetTypes())
        {
            CmdletAttribute? a = t.GetCustomAttribute<CmdletAttribute>();
            if (a is not null)
            {
                string name = $"{a.VerbName}-{a.NounName}";
                AllCommands.Add((t, name));
                if (a.SupportsShouldProcess) CommandsSupportingShouldProcess.Add((t, name));
                iss.Commands.Add(new SessionStateCmdletEntry(name, t, null));
            }
        }

        Assert.Distinct(AllCommands.ConvertAll(vt => vt.Name));

        PowerShell = System.Management.Automation.PowerShell.Create(iss);
        // This will create all the built-in variables like ConfirmPreference.
        // See PowerShell.Runspace.ExecutionContext.TopLevelSessionState.CurrentScope.Variables
        // in the debugger (all non-public below Runspace).
        // Alternatively, required variables could be added to the InitialSessionState...
        //     iss.Variables.Add(new SessionStateVariableEntry("ConfirmPreference", ConfirmImpact.High, ""));
        // ...or explicitly set in the Runspace.
        //     PowerShell.Runspace.SessionStateProxy.SetVariable("ConfirmPreference", ConfirmImpact.High);
        PowerShell.Runspace.ResetRunspaceState();
        // Avoid accidental issues with confirmation. We use -WhatIf for ShouldProcess() testing and assert
        // the ConfirmImpact property of the CmdletAttribute for selected commands.
        PowerShell.Runspace.SessionStateProxy.SetVariable("ConfirmPreference", ConfirmImpact.None);
        // 'Continue' is default, but make it explicit.
        PowerShell.Runspace.SessionStateProxy.SetVariable("ErrorActionPreference", ActionPreference.Continue);
    }

    protected readonly MockInterfaceChain InterfaceChain;

    public CommandTestBase()
    {
        InterfaceChain = new MockInterfaceChain();
        SearchApiCommandBase.SearchManagerFactory = InterfaceChain.Factory;
        PowerShell.Streams.ClearStreams();
        PowerShell.Commands.Clear();
    }

    protected static IEnumerable<object[]> CombineTestDataParameters(
        IEnumerable firstParameters,
        IEnumerable secondParameters)
    {
        foreach (object? first in firstParameters)
        {
            foreach (object? second in secondParameters)
            {
                yield return new[] { first, second };
            }
        }
    }

    protected static ErrorRecord AssertSingleErrorRecord(ExceptionParam exceptionParam)
    {
        Assert.True(PowerShell.HadErrors);
        ErrorRecord errorRecord = Assert.Single(PowerShell.Streams.Error);
        Assert.Same(exceptionParam.Exception, errorRecord.Exception);
        if (exceptionParam.IsSearchApi)
        {
            Assert.NotNull(errorRecord.ErrorDetails);
            Assert.NotEqual(errorRecord.Exception.Message, errorRecord.ErrorDetails.Message);
        }
        else
        {
            Assert.Null(errorRecord.ErrorDetails);
        }
        return errorRecord;
    }

    protected static ErrorRecord AssertUnauthorizedAccess()
    {
        Assert.True(PowerShell.HadErrors);
        ErrorRecord errorRecord = Assert.Single(PowerShell.Streams.Error);
        Assert.IsType<UnauthorizedAccessException>(errorRecord.Exception);
        Assert.Equal(ErrorCategory.PermissionDenied, errorRecord.CategoryInfo.Category);
        Assert.NotEqual(errorRecord.Exception.Message, errorRecord.ErrorDetails.Message);
        return errorRecord;
    }

    protected ErrorRecord AssertParameterValidation(string script, string? parameterName = null)
    {
        Collection<PSObject> results = InvokeScript(script);
        Assert.Empty(results);
        Assert.True(PowerShell.HadErrors);
        ErrorRecord errorRecord = Assert.Single(PowerShell.Streams.Error);
        ParameterBindingException exception = Assert.IsAssignableFrom<ParameterBindingException>(errorRecord.Exception);
        if (parameterName is not null) Assert.Equal(parameterName, exception.ParameterName);
        return errorRecord;
    }

    protected Collection<PSObject> InvokeScript(string script) => InvokeScript(script, null);
    protected Collection<PSObject> InvokeScript(string script, IEnumerable? input)
    {
        try
        {

            if (!script.EndsWith(" ", StringComparison.Ordinal))
            {
                // The PS parser has a few quirks. It is recommended to always add an extra space
                // at the end of the script. For example, "Set-SearchManager -UserAgent" with the
                // actual value omitted and no extra space will throw a NullReferenceException
                // instead of a param validation error.
                script += ' ';
            }

            Assert.Empty(PowerShell.Commands.Commands);
            PowerShell.AddScript(script);
            // Enabled by default, but make it explicit.
            InterfaceChain.EnableRecording(true);
            Assert.False(InterfaceChain.HasRecordings());
            return PowerShell.Invoke(input);
        }
        finally
        {
            InterfaceChain.EnableRecording(false);
        }
    }

    protected class Exception_TheoryData : TheoryData<ExceptionParam>
    {
        public Exception_TheoryData()
        {
            Add(new ExceptionParam(new Exception()));
            Add(new ExceptionParam(new COMException()));
            Add(new ExceptionParam(new COMException(null, ExceptionParam.MSS_E_CATALOGNOTFOUND)));
        }
    }
}
