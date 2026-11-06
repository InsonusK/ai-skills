using Todo.Domain.Entities;

namespace Todo.Application.Commands;

/// <summary>
/// The lists of this example, kept by the process while it runs and lost when
/// it stops: the HTTP variation point adds a way to call the application, not
/// a store, and AddItem and ListItems need somewhere to keep what they show.
/// One instance for the process, registered by the module; a lock keeps two
/// commands from changing one list at once. The DB variation point replaces it
/// with the repository of the lists.
/// </summary>
public sealed class KeptLists
{
    private readonly Dictionary<string, ItemList> _lists = [];
    private readonly Lock _changing = new();

    /// <summary>Runs <paramref name="change"/> on the list, started on its first item, while no other command changes a list.</summary>
    public void Change(string listId, Action<ItemList> change)
    {
        lock (_changing)
        {
            var list = _lists.TryGetValue(listId, out var kept) ? kept : new ItemList(listId);
            change(list);
            _lists[listId] = list;
        }
    }

    /// <summary>The titles and states of the list's items, or null when nobody added to it.</summary>
    public IReadOnlyList<(string Title, bool Done)>? Items(string listId)
    {
        lock (_changing)
        {
            return _lists.TryGetValue(listId, out var list) ? list.Items.Select(item => (item.Title, item.Done)).ToList() : null;
        }
    }
}
