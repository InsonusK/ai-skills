using MediatR;

namespace Todo.Commands;

/// <summary>Asks for the items of the list <paramref name="ListId"/>.</summary>
public sealed record ListItems(string ListId) : IRequest<ListItemsResult>;

/// <summary>The items of the list, in the order they were added.</summary>
public sealed record ListItemsResult(IReadOnlyList<ItemView> Items);
