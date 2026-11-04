namespace Todo.Domain.Entities;

/// <summary>
/// An item kept in a list: its id, the version of it the caller read, which the
/// caller gives back with a change, and the item itself. The version is opaque
/// (skill vp-row-version): it is carried and given back, never read or computed
/// with.
/// </summary>
public sealed record KeptItem(long Id, string Version, Item Item)
{
    /// <summary>The version the item is saved under: the caller's, set by the command.</summary>
    public string Version { get; set; } = Version;
}
