// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

// NotSupportedException: the original COM class does not support this member.
//
// Internal members are used to setup mock behavior.
internal class MockSearchScopeRule : ISearchScopeRule
{
    public virtual string PatternOrURL { get; internal set; } = string.Empty;
    public virtual int IsIncluded { get; internal set; }
    public virtual int IsDefault { get; internal set; }
    public virtual uint FollowFlags { get => throw new NotSupportedException(); }
}
