// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Contains the detailed result of testing a path against the search rules of a search catalog.
/// </summary>
public sealed class TestSearchRuleInfo : ICloneable
{
    /// <value>The path that has been tested.</value>
    public string? Path { get; set; }

    /// <value><c>true</c> if the path is included in the index, <c>false</c> otherwise.</value>
    public bool IsIncluded { get; set; }

    /// <value>One of the enumeration values indicating the reason why a path is included or excluded.</value>
    public CLUSION_REASON Reason { get; set; }

    /// <value><c>true</c> if the path has a child scope, <c>false</c> otherwise.</value>
    public bool HasChildScope { get; set; }

    /// <value><c>true</c> if the path has a parent scope, <c>false</c> otherwise.</value>
    public bool HasParentScope { get; set; }

    /// <value>The parent scope version ID.</value>
    public int ParentScopeVersiondId { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestSearchRuleInfo"/> class.
    /// </summary>
    public TestSearchRuleInfo() { }

    /// <summary>
    /// Creates a shallow copy of the <see cref="TestSearchRuleInfo"/> instance.
    /// </summary>
    /// <returns>A shallow copy of the <see cref="TestSearchRuleInfo"/> instance.</returns>
    public object Clone() => MemberwiseClone();
}
