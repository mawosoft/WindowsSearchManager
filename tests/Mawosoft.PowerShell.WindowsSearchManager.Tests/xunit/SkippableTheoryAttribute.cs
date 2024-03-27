// Copyright (c) 2023-2024 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public sealed class SkippableTheoryAttribute : TheoryAttribute
{
    [SuppressMessage("Design", "CA1019:Define accessors for attribute arguments", Justification = "Arguments become Skip property.")]
    public SkippableTheoryAttribute(params string[] skipconditions)
    {
        string? s = SkipCondition.Evaluate(skipconditions);
        // Don't overwrite user-set Skip with null.
        if (s is not null) Skip = s;
    }
}
