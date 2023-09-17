// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Contains information about a search root in a search catalog.
/// </summary>
public sealed class SearchRootInfo : ICloneable
{
    internal string? Schedule { get; set; }

    /// <summary>Gets or sets the path of this search root.</summary>
    /// <value>The path of this search root.</value>
    public string? Path { get; set; }

    /// <summary>Gets or sets a value indicating whether this search root is hierarchical.</summary>
    /// <value><c>true</c> if the search root is hierarchical, <c>false</c> otherwise.</value>
    public bool IsHierarchical { get; set; }

    /// <summary>Gets or sets a value indicating whether the search root provides change notifications.</summary>
    /// <value><c>true</c> if the search root provides notifications, <c>false</c> otherwise.</value>
    public bool ProvidesNotifications { get; set; }

    /// <summary>Gets or sets a value indicating whether the search root uses notifications only.</summary>
    /// <value><c>true</c> if the search root uses notifications only, <c>false</c> otherwise.</value>
    public bool UseNotificationsOnly { get; set; }

    /// <summary>Gets or sets the enumeration depth of the search root.</summary>
    /// <value>The enumeration depth of the search root.</value>
    public uint EnumerationDepth { get; set; }

    /// <summary>Gets or sets the host depth of the search root.</summary>
    /// <value>The host depth of the search root.</value>
    public uint HostDepth { get; set; }

    /// <summary>Gets or sets a value indicating whether to follow subdirectories.</summary>
    /// <value><c>true</c> to follow subdirectories, <c>false</c> otherwise.</value>
    public bool FollowDirectories { get; set; }

    /// <summary>Gets or sets the authentication type needed to crawl this search root.</summary>
    /// <value>One of the enumeration values indicating the authentication type.</value>
    public _AUTH_TYPE AuthenticationType { get; set; }

    internal string? User { get; set; }
    internal string? Password { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchRootInfo"/> class.
    /// </summary>
    public SearchRootInfo()
    {
        IsHierarchical = true;
        ProvidesNotifications = true;
        EnumerationDepth = uint.MaxValue;
        FollowDirectories = true;
    }

    internal SearchRootInfo(ISearchRoot searchRoot)
    {
        if (searchRoot is null) throw new ArgumentNullException(nameof(searchRoot));

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

    /// <summary>
    /// Creates a shallow copy of the <see cref="SearchRootInfo"/> instance.
    /// </summary>
    /// <returns>A shallow copy of the <see cref="SearchRootInfo"/> instance.</returns>
    public object Clone() => MemberwiseClone();
}
