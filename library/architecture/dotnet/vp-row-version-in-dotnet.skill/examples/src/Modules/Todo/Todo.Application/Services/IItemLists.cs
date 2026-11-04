using Todo.Domain.Entities;

namespace Todo.Application.Services;

/// <summary>
/// The item lists as the handlers see them; <see cref="ItemListsService"/> keeps
/// them in PostgreSQL. Every item goes up with its id and its version.
/// </summary>
public interface IItemLists
{
    /// <summary>Keeps item in the list; answers it with its id and its first version.</summary>
    Task<KeptItem> AddAsync(string listId, Item item, CancellationToken cancellationToken);

    Task<IReadOnlyList<KeptItem>> ItemsAsync(string listId, CancellationToken cancellationToken);

    /// <summary>One item of the list, with its version as it is now.</summary>
    Task<KeptItem> ItemAsync(string listId, long itemId, CancellationToken cancellationToken);

    /// <summary>Writes item back under the version it carries; answers it with its new version.</summary>
    Task<KeptItem> SaveAsync(string listId, KeptItem item, CancellationToken cancellationToken);
}
