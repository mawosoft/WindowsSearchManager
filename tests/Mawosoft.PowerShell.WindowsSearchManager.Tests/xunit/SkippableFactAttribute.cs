// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SkippableFactAttribute : FactAttribute
{
    public SkippableFactAttribute(params string[] skipconditions)
    {
        string? s = SkipCondition.Evaluate(skipconditions);
        // Don't overwrite user-set Skip with null.
        if (s is not null) Skip = s;
    }
}
