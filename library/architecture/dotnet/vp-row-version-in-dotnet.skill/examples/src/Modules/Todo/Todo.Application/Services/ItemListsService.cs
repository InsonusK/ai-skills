using System.Globalization;
using Postgres.Abstractions;
using Postgres.Abstractions.Rows;
using Postgres.Abstractions.Specifications;
using Todo.Domain.Entities;
using Todo.Domain.Errors;

namespace Todo.Application.Services;

/// <summary>
/// The service over the rows of the lists: it speaks the application's language
/// and turns the repository's technical outcomes into the error kinds - a
/// duplicate key is a conflict, no rows is not found, a row changed since the
/// caller read it is a concurrent change, a database that does not answer is
/// unavailable. The selector stops here: entities go up, each with its id and
/// its version as text.
/// </summary>
public sealed class ItemListsService(IRepository<ItemRow> rows) : IItemLists
{
    public async Task<KeptItem> AddAsync(string listId, Item item, CancellationToken cancellationToken)
    {
        var row = new ItemRow { ListId = listId, Title = item.Title, Done = item.Done };
        try
        {
            await rows.InsertAsync(row, cancellationToken);
        }
        catch (DuplicateKeyException duplicate)
        {
            throw TodoErrors.ItemAlreadyInList(listId, item.Title, duplicate);
        }
        catch (StoreUnavailableException unavailable)
        {
            throw TodoErrors.ListsUnavailable(unavailable);
        }
        return Kept(row);
    }

    public async Task<IReadOnlyList<KeptItem>> ItemsAsync(string listId, CancellationToken cancellationToken)
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
        return items.Select(Kept).ToList();
    }

    public async Task<KeptItem> ItemAsync(string listId, long itemId, CancellationToken cancellationToken)
    {
        ItemRow? row;
        try
        {
            row = await rows.SelectBy(new ItemOfList(listId, itemId)).SingleAsync(cancellationToken);
        }
        catch (StoreUnavailableException unavailable)
        {
            throw TodoErrors.ListsUnavailable(unavailable);
        }
        return row is null ? throw TodoErrors.NoItem(listId, itemId) : Kept(row);
    }

    public async Task<KeptItem> SaveAsync(string listId, KeptItem item, CancellationToken cancellationToken)
    {
        // A version that is no number is none this service gave: it matches no
        // row, as a version another caller moved does not.
        if (!uint.TryParse(item.Version, NumberStyles.None, CultureInfo.InvariantCulture, out var version))
        {
            throw TodoErrors.ItemChanged(listId, item.Id);
        }
        var row = new ItemRow { Id = item.Id, ListId = listId, Title = item.Item.Title, Done = item.Item.Done, Version = version };
        try
        {
            await rows.UpdateAsync(row, cancellationToken);
        }
        catch (RowChangedException changed)
        {
            throw TodoErrors.ItemChanged(listId, item.Id, changed);
        }
        catch (DuplicateKeyException duplicate)
        {
            throw TodoErrors.ItemAlreadyInList(listId, item.Item.Title, duplicate);
        }
        catch (StoreUnavailableException unavailable)
        {
            throw TodoErrors.ListsUnavailable(unavailable);
        }
        return Kept(row);
    }

    // The entity of a row, its rules checked again, with its id and its version.
    private static KeptItem Kept(ItemRow row)
    {
        var item = new Item(row.Title);
        if (row.Done)
        {
            item.Complete();
        }
        return new KeptItem(row.Id, row.Version.ToString(CultureInfo.InvariantCulture), item);
    }
}
