using Memory.Abstractions;
using Todo.Domain.Entities;
using Todo.Domain.Errors;

namespace Todo.Application.Services;

/// <summary>
/// The service over the list store: it speaks the application's language and
/// turns the store's technical outcomes into the error kinds - a duplicate key
/// is a conflict, no rows is not found.
/// </summary>
public sealed class ItemListsService(IListStore store) : IItemLists
{
    public async Task AddAsync(string listId, Item item, CancellationToken cancellationToken)
    {
        try
        {
            await store.InsertAsync(listId, new ListRow(item.Title, item.Done), cancellationToken);
        }
        catch (DuplicateKeyException duplicate)
        {
            throw TodoErrors.ItemAlreadyInList(listId, item.Title, duplicate);
        }
    }

    public async Task<IReadOnlyList<Item>> ItemsAsync(string listId, CancellationToken cancellationToken)
    {
        var rows = await store.SelectAsync(listId, cancellationToken) ?? throw TodoErrors.NoList(listId);
        return rows.Select(row =>
        {
            var item = new Item(row.Title);
            if (row.Done)
            {
                item.Complete();
            }
            return item;
        }).ToList();
    }
}
