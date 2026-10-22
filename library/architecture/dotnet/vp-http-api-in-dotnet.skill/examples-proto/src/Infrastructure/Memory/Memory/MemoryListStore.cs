using Memory.Abstractions;

namespace Memory;

/// <summary>
/// Keeps the item lists in the process memory. The DB variation point
/// replaces it with a store over a real database; the service above it keeps
/// its methods, and no command changes.
/// </summary>
public sealed class MemoryListStore : IListStore
{
    private readonly Dictionary<string, List<ListRow>> _rows = [];
    private readonly Lock _writing = new();

    public Task InsertAsync(string listId, ListRow row, CancellationToken cancellationToken)
    {
        lock (_writing)
        {
            if (!_rows.TryGetValue(listId, out var rows))
            {
                _rows[listId] = rows = [];
            }
            if (rows.Any(existing => existing.Title == row.Title))
            {
                throw new DuplicateKeyException($"duplicate key: list \"{listId}\", title \"{row.Title}\"");
            }
            rows.Add(row);
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ListRow>?> SelectAsync(string listId, CancellationToken cancellationToken)
    {
        lock (_writing)
        {
            return Task.FromResult<IReadOnlyList<ListRow>?>(_rows.TryGetValue(listId, out var rows) ? [.. rows] : null);
        }
    }
}
