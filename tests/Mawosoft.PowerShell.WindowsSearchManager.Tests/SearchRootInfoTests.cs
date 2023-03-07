﻿// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchRootInfoTests
{
    [Fact]
    public void ctor_Defaults()
    {
        SearchRootInfo info = new();
        Assert.Null(info.Schedule);
        Assert.Null(info.Path);
        Assert.True(info.IsHierarchical);
        Assert.True(info.ProvidesNotifications);
        Assert.False(info.UseNotificationsOnly);
        Assert.Equal(uint.MaxValue, info.EnumerationDepth);
        Assert.Equal(0u, info.HostDepth);
        Assert.True(info.FollowDirectories);
        Assert.Equal(_AUTH_TYPE.eAUTH_TYPE_ANONYMOUS, info.AuthenticationType);
        Assert.Null(info.User);
        Assert.Null(info.Password);
    }

    [Fact]
    public void ctor_NullArguments_Throws()
    {
        Assert.Throws<ArgumentNullException>("searchRoot", () => new SearchRootInfo(null!));
    }

    [Fact]
    public void ctor_ISearchRoot_Succeeds()
    {
        MockSearchRoot mock = new()
        {
            RootURL = @"fooprotocol:///{bar-class}",
            IsHierarchical = 0,
            UseNotificationsOnly = 1,
            EnumerationDepth = 333,
            HostDepth = 222,
            AuthenticationType = _AUTH_TYPE.eAUTH_TYPE_BASIC
        };
        SearchRootInfo info = new(mock);
        Assert.Null(info.Schedule);
        Assert.Equal(mock.RootURL, info.Path);
        Assert.Equal(mock.IsHierarchical != 0, info.IsHierarchical);
        Assert.Equal(mock.ProvidesNotifications != 0, info.ProvidesNotifications);
        Assert.Equal(mock.UseNotificationsOnly != 0, info.UseNotificationsOnly);
        Assert.Equal(mock.EnumerationDepth, info.EnumerationDepth);
        Assert.Equal(mock.HostDepth, info.HostDepth);
        Assert.Equal(mock.FollowDirectories != 0, info.FollowDirectories);
        Assert.Equal(mock.AuthenticationType, info.AuthenticationType);
        Assert.Null(info.User);
        Assert.Null(info.Password);
    }

    [Fact]
    public void ToCSearchRoot_Succeeds()
    {
        SearchRootInfo info = new()
        {
            Schedule = "schedule",
            Path = @"file:///C:\",
            EnumerationDepth = 111,
            HostDepth = 222,
            AuthenticationType = _AUTH_TYPE.eAUTH_TYPE_NTLM,
            User = "user",
            Password = "password"
        };
        ISearchRoot root = info.ToCSearchRoot();
        Assert.Equal(info.Path, root.RootURL);
        Assert.Equal(info.IsHierarchical, root.IsHierarchical != 0);
        Assert.Equal(info.ProvidesNotifications, root.ProvidesNotifications != 0);
        Assert.Equal(info.UseNotificationsOnly, root.UseNotificationsOnly != 0);
        Assert.Equal(info.EnumerationDepth, root.EnumerationDepth);
        Assert.Equal(info.HostDepth, root.HostDepth);
        Assert.Equal(info.FollowDirectories, root.FollowDirectories != 0);
        Assert.Equal(info.AuthenticationType, root.AuthenticationType);
    }

    [Fact]
    public void Clone_Succeeds()
    {
        SearchRootInfo info = new()
        {
            Schedule = "schedule",
            Path = @"fooprotocol:///{bar-class}",
            UseNotificationsOnly = true,
            EnumerationDepth = 333,
            HostDepth = 222,
            AuthenticationType = _AUTH_TYPE.eAUTH_TYPE_BASIC,
            User = "buzz",
            Password = "pass"
        };
        SearchRootInfo clone = (SearchRootInfo)info.Clone();
        Assert.Equal(info, clone, ShallowFieldComparer.Instance);
    }
}
