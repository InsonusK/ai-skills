using Todo.Domain.Entities;

namespace Todo.Application.Services;

/// <summary>
/// What the module's handlers need of the item lists, in the application's
/// language. Declared here, beside its consumers, for the composition root to
/// choose the implementation and for the handlers' scenarios to fake it; no
/// other module or entry point sees it (skill app-layers-in-dotnet).
/// </summary>
public interface IItemLists
{
    /// <summary>Keeps <paramref name="item"/> in the list; a title already in it is a conflict.</summary>
    Task AddAsync(string listId, Item item, CancellationToken cancellationToken);

    /// <summary>The items of the list in the order they were added; an unknown list is not found.</summary>
    Task<IReadOnlyList<Item>> ItemsAsync(string listId, CancellationToken cancellationToken);
}
