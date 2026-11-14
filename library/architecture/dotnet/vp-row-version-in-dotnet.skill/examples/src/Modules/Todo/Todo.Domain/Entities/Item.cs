using System.Globalization;
using Todo.Domain.Errors;

namespace Todo.Domain.Entities;

/// <summary>
/// One to-do item, a row of the table list_items (Configurations/ItemConfiguration).
/// Its state changes only through its methods, which guard the item's own rules.
/// A title is unique in its list: the table's unique index holds it.
/// Its rows change, so the item carries the row version (skill vp-row-version):
/// EF Core compares it on every save.
/// </summary>
public sealed class Item
{
    /// <summary>The longest item title, in characters.</summary>
    public const int MaxTitleLength = 100;

    /// <summary>Creates an open item; the title is trimmed and must be 1..<see cref="MaxTitleLength"/> characters.</summary>
    public Item(string title)
    {
        title = title.Trim();
        if (title.Length == 0)
        {
            throw TodoErrors.EmptyTitle();
        }
        // Characters as a reader counts them, not UTF-16 units.
        if (new StringInfo(title).LengthInTextElements > MaxTitleLength)
        {
            throw TodoErrors.TitleTooLong();
        }
        Title = title;
    }

    /// <summary>An item of the list <paramref name="listId"/>, as AddItem keeps it.</summary>
    public static Item InList(string listId, string title) => new(title) { ListId = listId };

    /// <summary>The store's key of the item, 0 until it is saved.</summary>
    public long Id { get; private set; }

    /// <summary>The list the item belongs to; empty for an item of no list (a planned one).</summary>
    public string ListId { get; private set; } = "";

    public string Title { get; }

    public bool Done { get; private set; }

    /// <summary>
    /// The row version the engine keeps (PostgreSQL's xmin, configured
    /// <c>IsRowVersion()</c>): EF Core compares it in the WHERE of every update
    /// and delete and reads the new one back. Opaque text in every contract.
    /// </summary>
    public uint Version { get; private set; }

    /// <summary>
    /// Marks the item done; an item is completed once.
    /// The change is saved under the version the caller read
    /// (<see cref="ChangeUnder"/>), so an item changed since is not completed.
    /// </summary>
    public void Complete()
    {
        if (Done)
        {
            throw TodoErrors.AlreadyDone();
        }
        Done = true;
    }

    /// <summary>
    /// Sets the version the caller read, under which the change that follows is
    /// saved: EF Core compares this value, not the one the store holds now. A
    /// version that is no number matches no row, so it is a concurrent change
    /// like any other moved version.
    /// </summary>
    public void ChangeUnder(string version) => Version = uint.TryParse(version, out var read) ? read : 0;
}
