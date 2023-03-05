// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SkippableTheoryAttribute : TheoryAttribute
{
    public SkippableTheoryAttribute(params string[] skipconditions)
    {
        string? s = SkipCondition.Evaluate(skipconditions);
        // Don't overwrite user-set Skip with null.
        if (s != null) Skip = s;
    }
}
