using Todo.Domain.Entities;
using Todo.Domain.Errors;

namespace Todo.Application.Commands;

/// <summary>
/// The lists of this example, kept by the process while it runs and lost when
/// it stops: the HTTP variation point adds a way to call the application, not
/// a store, and AddItem and ListItems need somewhere to keep what they show.
/// A title is unique in its list. One instance for the process, registered by
/// the module; a lock keeps two commands from changing one list at once. The
/// DB variation point replaces it with the repository of the items.
/// </summary>
public sealed class KeptLists
{
    private readonly Dictionary<string, List<Item>> _items = [];
    private readonly Lock _changing = new();

    /// <summary>Keeps <paramref name="item"/> at the end of the list, started on its first item; a title already in the list is a conflict.</summary>
    public void Add(string listId, Item item)
    {
        lock (_changing)
        {
            if (!_items.TryGetValue(listId, out var kept))
            {
                _items[listId] = kept = [];
            }
            if (kept.Any(existing => existing.Title == item.Title))
            {
                throw TodoErrors.ItemAlreadyInList(listId, item.Title);
            }
            kept.Add(item);
        }
    }

    /// <summary>The titles and states of the list's items, or null when nobody added to it.</summary>
    public IReadOnlyList<(string Title, bool Done)>? Items(string listId)
    {
        lock (_changing)
        {
            return _items.TryGetValue(listId, out var kept) ? kept.Select(item => (item.Title, item.Done)).ToList() : null;
        }
    }
}
