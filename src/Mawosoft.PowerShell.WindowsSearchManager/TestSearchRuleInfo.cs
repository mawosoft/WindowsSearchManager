// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// POCO for result of Test-SearchRule -Detailed
/// </summary>
public sealed class TestSearchRuleInfo : ICloneable
{
    public string? Path { get; set; }
    public bool IsIncluded { get; set; }
    public CLUSION_REASON Reason { get; set; }
    public bool HasChildScope { get; set; }
    public bool HasParentScope { get; set; }
    public int ParentScopeVersiondId { get; set; }

    public TestSearchRuleInfo() { }

    public object Clone() => MemberwiseClone();
}
