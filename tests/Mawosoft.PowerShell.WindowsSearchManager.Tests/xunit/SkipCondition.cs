// Copyright (c) 2023 Matthias Wolf, Mawosoft.

using System.ServiceProcess;

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

internal static class SkipCondition
{
    public const string WSearchEnabled = "WSearchEnabled";
    public const string WSearchDisabled = "WSearchDisabled";
    public const string IsCIandWSearchDisabled = "IsCIandWSearchDisabled";

    private static readonly bool s_isCI;

    static SkipCondition()
    {
        foreach (string env in new[] { "CI", "TF_BUILD", "GITHUB_ACTIONS", "APPVEYOR" })
        {
            if (Environment.GetEnvironmentVariable(env) != null)
            {
                s_isCI = true;
                break;
            }
        }
    }

    public static string? Evaluate(params string[] skipconditions)
    {
        if (skipconditions == null)
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
                _ => throw new ArgumentException(null, nameof(skipcondition)),
            };
            if (skip)
            {
                return nameof(SkipCondition) + ": " + skipcondition;
            }
        }
        return null;
    }

#if !NETFRAMEWORK
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
#endif
    private static bool IsWSearchEnabled()
    {
        using ServiceController sc = new("WSearch");
        return sc.StartType != ServiceStartMode.Disabled;
    }
}
