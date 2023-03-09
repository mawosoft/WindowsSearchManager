// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

// NotSupportedException: the original COM class does not support this member.
public class MockSearchRoot : ISearchRoot
{
    public virtual string Schedule { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public virtual string RootURL { get; set; } = string.Empty;
    public virtual int IsHierarchical { get; set; } = 1;
    public virtual int ProvidesNotifications { get; set; } = 1;
    public virtual int UseNotificationsOnly { get; set; }
    public virtual uint EnumerationDepth { get; set; } = uint.MaxValue;
    public virtual uint HostDepth { get; set; }
    public virtual int FollowDirectories { get; set; } = 1;
    public virtual _AUTH_TYPE AuthenticationType { get; set; }
    public virtual string User { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public virtual string Password { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
}
