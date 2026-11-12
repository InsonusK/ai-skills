using Postgres.Abstractions;
using Postgres.Abstractions.Rows;
using Postgres.Abstractions.Specifications;
using Todo.Domain.Entities;
using Todo.Domain.Errors;

namespace Todo.Application.Services;

/// <summary>
/// The service over the rows of the lists: it speaks the application's language
/// and turns the repository's technical outcomes into the error kinds - a
/// duplicate key is a conflict, no rows is not found, a database that does not
/// answer is unavailable. The selector stops here: entities go up.
/// </summary>
public sealed class ItemListsService(IRepository<ItemRow> rows) : IItemLists
{
    public async Task AddAsync(string listId, Item item, CancellationToken cancellationToken)
    {
        try
        {
            await rows.InsertAsync(new ItemRow { ListId = listId, Title = item.Title, Done = item.Done }, cancellationToken);
        }
        catch (DuplicateKeyException duplicate)
        {
            throw TodoErrors.ItemAlreadyInList(listId, item.Title, duplicate);
        }
        catch (StoreUnavailableException unavailable)
        {
            throw TodoErrors.ListsUnavailable(unavailable);
        }
    }

    public async Task<IReadOnlyList<Item>> ItemsAsync(string listId, CancellationToken cancellationToken)
    {
        IReadOnlyList<ItemRow> items;
        try
        {
            items = await rows.SelectBy(new ItemsOfList(listId)).OrderBy(row => row.Id).AllAsync(cancellationToken);
        }
        catch (StoreUnavailableException unavailable)
        {
            throw TodoErrors.ListsUnavailable(unavailable);
        }
        if (items.Count == 0)
        {
            throw TodoErrors.NoList(listId);
        }
        return items.Select(row =>
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
