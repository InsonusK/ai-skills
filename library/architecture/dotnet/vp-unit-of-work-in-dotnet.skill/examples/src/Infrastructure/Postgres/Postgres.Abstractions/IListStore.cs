namespace Postgres.Abstractions;

/// <summary>One stored item of a list.</summary>
public sealed record ListRow(string Title, bool Done);

/// <summary>
/// Rows kept by list id, each title unique in its list - in the technique's
/// terms (insert, select, rows). The service over it speaks the application's
/// language. Every method throws <see cref="StoreUnavailableException"/> when
/// the database does not answer.
/// </summary>
public interface IListStore
{
    /// <summary>Appends <paramref name="row"/> to the list, creating it on its first row; a title already in the list throws <see cref="DuplicateKeyException"/>.</summary>
    Task InsertAsync(string listId, ListRow row, CancellationToken cancellationToken);

    /// <summary>The rows of the list in insertion order, or null when the list has no row.</summary>
    Task<IReadOnlyList<ListRow>?> SelectAsync(string listId, CancellationToken cancellationToken);
}
