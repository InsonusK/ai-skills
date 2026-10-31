using Microsoft.EntityFrameworkCore;
using Postgres.Abstractions;
using Postgres.Rows;

namespace Postgres.Stores;

/// <summary>
/// Keeps the item lists in PostgreSQL. A store saves at once: a command that
/// must keep several writes together gets a transaction around them from the
/// unit of work variation point, with no change here. A row that failed to save
/// leaves the context, or the next save of the command would send it again.
/// </summary>
public sealed class PostgresListStore(AppDbContext context) : IListStore
{
    public async Task InsertAsync(string listId, ListRow row, CancellationToken cancellationToken)
    {
        var item = new ItemRow { ListId = listId, Title = row.Title, Done = row.Done };
        context.Set<ItemRow>().Add(item);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception error) when (StoreFailures.IsDuplicateKey(error))
        {
            context.Entry(item).State = EntityState.Detached;
            throw new DuplicateKeyException($"duplicate key: list \"{listId}\", title \"{row.Title}\"", error);
        }
        catch (Exception error) when (StoreFailures.IsUnavailable(error))
        {
            context.Entry(item).State = EntityState.Detached;
            throw new StoreUnavailableException($"insert into list \"{listId}\": the database does not answer", error);
        }
    }

    public async Task<IReadOnlyList<ListRow>?> SelectAsync(string listId, CancellationToken cancellationToken)
    {
        try
        {
            var rows = await context.Set<ItemRow>()
                .AsNoTracking()
                .Where(item => item.ListId == listId)
                .OrderBy(item => item.Id)
                .Select(item => new ListRow(item.Title, item.Done))
                .ToListAsync(cancellationToken);
            return rows.Count == 0 ? null : rows;
        }
        catch (Exception error) when (StoreFailures.IsUnavailable(error))
        {
            throw new StoreUnavailableException($"select of list \"{listId}\": the database does not answer", error);
        }
    }
}
