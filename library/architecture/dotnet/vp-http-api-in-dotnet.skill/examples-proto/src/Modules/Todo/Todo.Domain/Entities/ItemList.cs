using Todo.Domain.Errors;

namespace Todo.Domain.Entities;

/// <summary>
/// A to-do list and its items. A title is unique in its list, a rule over every
/// item, so the list guards it, and the items are added only through it. The
/// caller names the list; it exists from its first item on.
/// </summary>
public sealed class ItemList
{
    private readonly List<Item> _items = [];

    public ItemList(string id) => Id = id;

    /// <summary>The name the caller gave the list.</summary>
    public string Id { get; }

    /// <summary>The items in the order they were added.</summary>
    public IReadOnlyList<Item> Items => _items;

    /// <summary>Adds <paramref name="item"/> at the end; a title already in the list is a conflict.</summary>
    public void Add(Item item)
    {
        if (_items.Any(existing => existing.Title == item.Title))
        {
            throw TodoErrors.ItemAlreadyInList(Id, item.Title);
        }
        _items.Add(item);
    }
}
