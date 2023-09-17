// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Contains the detailed result of testing a path against the search rules of a search catalog.
/// </summary>
public sealed class TestSearchRuleInfo : ICloneable
{

    /// <summary>Gets or sets the path that has been tested.</summary>
    /// <value>The path that has been tested.</value>
    public string? Path { get; set; }

    /// <summary>Gets or sets a value indicating whether the path is included in the index.</summary>
    /// <value><c>true</c> if the path is included, <c>false</c> otherwise.</value>
    public bool IsIncluded { get; set; }

    /// <summary>Gets or sets a value indicating why the path is included or excluded.</summary>
    /// <value>One of the enumeration values indicating the reason for the inclusion or exclusion.</value>
    public CLUSION_REASON Reason { get; set; }

    /// <summary>Gets or sets a value indicating whether the path has a child scope.</summary>
    /// <value><c>true</c> if the path has a child scope, <c>false</c> otherwise.</value>
    public bool HasChildScope { get; set; }

    /// <summary>Gets or sets a value indicating whether the path has a parent scope.</summary>
    /// <value><c>true</c> if the path has a parent scope, <c>false</c> otherwise.</value>
    public bool HasParentScope { get; set; }

    /// <summary>Gets or sets the parent scope version ID.</summary>
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
