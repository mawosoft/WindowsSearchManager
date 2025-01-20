// Copyright (c) Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

// NotSupportedException: the original COM class does not support this member.
//
// Internal members are used to setup mock behavior.
public class MockSearchScopeRule : ISearchScopeRule, CSearchScopeRule
{
    public virtual string PatternOrURL { get; internal set; } = string.Empty;
    public virtual int IsIncluded { get; internal set; }
    public virtual int IsDefault { get; internal set; }
    [ExcludeFromCodeCoverage] public virtual uint FollowFlags => throw new NotSupportedException();
}
