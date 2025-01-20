// Copyright (c) Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

internal static class SkipCondition
{
    public const string WSearchEnabled = "WSearchEnabled";
    public const string WSearchDisabled = "WSearchDisabled";
    public const string IsCIandWSearchDisabled = "IsCIandWSearchDisabled";

    private static readonly bool s_isCI = InitCI();

    private static bool InitCI()
    {
        foreach (string env in new[] { "CI", "TF_BUILD", "GITHUB_ACTIONS", "APPVEYOR" })
        {
            if (Environment.GetEnvironmentVariable(env) is not null)
            {
                return true;
            }
        }
        return false;
    }

    public static string? Evaluate(params string[] skipconditions)
    {
        if (skipconditions is null)
        {
            throw new ArgumentNullException(nameof(skipconditions));
        }
        foreach (string skipcondition in skipconditions)
        {
            bool skip = skipcondition switch
            {
                WSearchEnabled => IsWSearchEnabled(),
                WSearchDisabled => !IsWSearchEnabled(),
                IsCIandWSearchDisabled => s_isCI && !IsWSearchEnabled(),
                _ => throw new ArgumentException(null, nameof(skipconditions)),
            };
            if (skip)
            {
                return nameof(SkipCondition) + ": " + skipcondition;
            }
        }
        return null;
    }

#if !NETFRAMEWORK
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
#endif
    private static bool IsWSearchEnabled()
    {
        using System.ServiceProcess.ServiceController sc = new("WSearch");
        return sc.StartType != System.ServiceProcess.ServiceStartMode.Disabled;
    }
}
