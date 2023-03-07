// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// POCO for CSearchRoot
/// Properties declared as internal are not implemented by CSearchRoot.
/// </summary>
public sealed class SearchRootInfo : ICloneable
{
    internal string? Schedule { get; set; }
    public string? Path { get; set; }
    public bool IsHierarchical { get; set; }
    public bool ProvidesNotifications { get; set; }
    public bool UseNotificationsOnly { get; set; }
    public uint EnumerationDepth { get; set; }
    public uint HostDepth { get; set; }
    public bool FollowDirectories { get; set; }
    public _AUTH_TYPE AuthenticationType { get; set; }
    internal string? User { get; set; }
    internal string? Password { get; set; }

    public SearchRootInfo()
    {
        IsHierarchical = true;
        ProvidesNotifications = true;
        EnumerationDepth = uint.MaxValue;
        FollowDirectories = true;
    }

    internal SearchRootInfo(ISearchRoot searchRoot)
    {
        if (searchRoot == null) throw new ArgumentNullException(nameof(searchRoot));

        Path = searchRoot.RootURL;
        IsHierarchical = searchRoot.IsHierarchical != 0;
        ProvidesNotifications = searchRoot.ProvidesNotifications != 0;
        UseNotificationsOnly = searchRoot.UseNotificationsOnly != 0;
        EnumerationDepth = searchRoot.EnumerationDepth;
        HostDepth = searchRoot.HostDepth;
        FollowDirectories = searchRoot.FollowDirectories != 0;
        AuthenticationType = searchRoot.AuthenticationType;
    }

    internal CSearchRoot ToCSearchRoot()
    {
        CSearchRoot root = new()
        {
            RootURL = Path,
            IsHierarchical = IsHierarchical ? 1 : 0,
            ProvidesNotifications = ProvidesNotifications ? 1 : 0,
            UseNotificationsOnly = UseNotificationsOnly ? 1 : 0,
            EnumerationDepth = EnumerationDepth,
            HostDepth = HostDepth,
            FollowDirectories = FollowDirectories ? 1 : 0,
            AuthenticationType = AuthenticationType
        };
        return root;
    }

    public object Clone() => MemberwiseClone();
}
