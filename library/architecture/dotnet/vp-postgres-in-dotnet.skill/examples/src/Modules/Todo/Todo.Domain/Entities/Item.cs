using System.Globalization;
using Todo.Domain.Errors;

namespace Todo.Domain.Entities;

/// <summary>
/// One to-do item. Its state changes only through its methods, which guard the
/// item's own rules. A stored item is a child of its <see cref="ItemList"/>,
/// which adds it.
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

    /// <summary>The store's key of the item, 0 until its list is saved.</summary>
    public long Id { get; private set; }

    public string Title { get; }

    public bool Done { get; private set; }

    /// <summary>Marks the item done; an item is completed once.</summary>
    public void Complete()
    {
        if (Done)
        {
            throw TodoErrors.AlreadyDone();
        }
        Done = true;
    }
}
