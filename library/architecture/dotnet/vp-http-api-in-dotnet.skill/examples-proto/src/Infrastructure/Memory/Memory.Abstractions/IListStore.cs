namespace Memory.Abstractions;

/// <summary>One stored item of a list.</summary>
public sealed record ListRow(string Title, bool Done);

/// <summary>
/// A unique key already taken - the technical outcome a database driver
/// reports for an insert that breaks a unique index.
/// </summary>
public sealed class DuplicateKeyException(string message) : Exception(message);

/// <summary>
/// Rows kept by list id, each title unique in its list - in the technique's
/// terms (insert, select, rows). The service over it speaks the application's
/// language; the DB variation point brings a store over a real database.
/// </summary>
public interface IListStore
{
    /// <summary>Appends <paramref name="row"/> to the list, creating it on its first row; a title already in the list throws <see cref="DuplicateKeyException"/>.</summary>
    Task InsertAsync(string listId, ListRow row, CancellationToken cancellationToken);

    /// <summary>The rows of the list in insertion order, or null when the list has no row.</summary>
    Task<IReadOnlyList<ListRow>?> SelectAsync(string listId, CancellationToken cancellationToken);
}
