// Copyright (c) 2023 Matthias Wolf, Mawosoft.

using System.ServiceProcess;

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

internal static class SkipCondition
{
    // TODO should we introduce Not operator? Could be bool or prefix string "!"
    public const string IsNetFramework = "IsNetFramework";
    public const string IsNotNetFramework = "IsNotNetFramework";
    public const string WSearchEnabled = "WSearchEnabled";
    public const string WSearchDisabled = "WSearchDisabled";

    private static readonly bool s_isNetFramework =
        RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework", StringComparison.OrdinalIgnoreCase);

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
                IsNetFramework => s_isNetFramework,
                IsNotNetFramework => !s_isNetFramework,
                WSearchEnabled => IsWSearchEnabled(),
                WSearchDisabled => !IsWSearchEnabled(),
                _ => throw new ArgumentException(null, nameof(skipcondition)),
            };
            if (skip)
            {
                return nameof(SkipCondition) + ": " + skipcondition;
            }
        }
        return null;
    }

    private static bool IsWSearchEnabled()
    {
        using ServiceController sc = new("WSearch");
        return sc.StartType != ServiceStartMode.Disabled;
    }
}
