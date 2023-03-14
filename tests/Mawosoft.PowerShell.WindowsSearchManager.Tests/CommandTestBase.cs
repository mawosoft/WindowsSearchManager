// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

[Collection(nameof(NoParallelTests))]
public class CommandTestBase
{
    protected static readonly System.Management.Automation.PowerShell PowerShell;

    // We already have SearchApiCommandBase.SearchManagerFactory as a static variable and therefore
    // disabled parallel tests. So we can use a static instance of PowerShell as well.
    //
    // This provides a limited InitialSessionState sufficient for testing w/o requiring the full PowerShell SDK.
    // However, user interaction is not possible, and a confirmation prompt will result HostException, which in
    // turn will contain the full prompt message.
    // '-WhatIf' works in so far as ShouldProcess() returns false, but the message is not available.
    //
    // The cmdlet can be invoked via AddScript(), but the PS parser has a few quirks. It is recommended to always
    // add an extra space at the end of the script. For example, "Set-SearchManager -UserAgent" with the actual
    // value omitted and no extra space will throw a NullReferenceException instead of a param validation error.
    static CommandTestBase()
    {
        InitialSessionState iss = InitialSessionState.Create();
        iss.ThreadOptions = PSThreadOptions.UseCurrentThread;
        iss.LanguageMode = PSLanguageMode.FullLanguage;
        foreach (Type t in typeof(SearchApiCommandBase).Assembly.GetTypes())
        {
            CmdletAttribute? a = t.GetCustomAttribute<CmdletAttribute>();
            if (a != null)
            {
                iss.Commands.Add(new SessionStateCmdletEntry($"{a.VerbName}-{a.NounName}", t, null));
            }
        }
        PowerShell = System.Management.Automation.PowerShell.Create(iss);
        // This will create all the built-in variables like ConfirmPreference.
        // See PowerShell.Runspace.ExecutionContext.TopLevelSessionState.CurrentScope.Variables
        // in the debugger (all non-public below Runspace).
        // Alternatively, required variables could be added to the InitialSessionState...
        //     iss.Variables.Add(new SessionStateVariableEntry("ConfirmPreference", ConfirmImpact.High, ""));
        // ...or explicitly set in the Runspace.
        //     PowerShell.Runspace.SessionStateProxy.SetVariable("ConfirmPreference", ConfirmImpact.High);
        PowerShell.Runspace.ResetRunspaceState();
    }

    protected readonly MockInterfaceChain InterfaceChain;

    public CommandTestBase()
    {
        InterfaceChain = new MockInterfaceChain();
        SearchApiCommandBase.SearchManagerFactory = InterfaceChain.Factory;
        PowerShell.Streams.ClearStreams();
        PowerShell.Commands.Clear();
        // TODO? PowerShell.Runspace.SessionStateProxy.SetVariable("ConfirmPreference", ConfirmImpact.High);
    }

    protected void AssertShouldProcess(Type commandType, ConfirmImpact confirmImpact)
    {
        CmdletAttribute a = commandType.GetCustomAttribute<CmdletAttribute>()!;
        Assert.NotNull(a);
        Assert.True(a.SupportsShouldProcess, "Expected SupportsShouldProcess == true");
        Assert.Equal(confirmImpact, a.ConfirmImpact);
    }

    protected ErrorRecord AssertSingleErrorRecord(Exception exception, bool shouldHaveCustomDetails)
    {
        Assert.True(PowerShell.HadErrors);
        ErrorRecord errorRecord = Assert.Single(PowerShell.Streams.Error);
        Assert.Same(exception, errorRecord.Exception);
        if (shouldHaveCustomDetails)
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

    protected ErrorRecord AssertParameterValidation(string script)
    {
        PowerShell.AddScript(script);
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.Empty(results);
        Assert.True(PowerShell.HadErrors);
        ErrorRecord errorRecord = Assert.Single(PowerShell.Streams.Error);
        Assert.IsAssignableFrom<ParameterBindingException>(errorRecord.Exception);
        return errorRecord;
    }

    protected class Exception_TheoryData : TheoryData<ExceptionParam, bool>
    {
        public Exception_TheoryData()
        {
            Add(new ExceptionParam(new Exception()), false);
            Add(new ExceptionParam(new COMException()), false);
            Add(new ExceptionParam(new COMException(null, unchecked((int)0x80042103))), true);
        }
    }
}
