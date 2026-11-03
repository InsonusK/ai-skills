using MediatR;

namespace Todo.Commands;

/// <summary>Asks to add every one of <paramref name="Titles"/> to the list <paramref name="ListId"/>, or none.</summary>
public sealed record ImportItemsCommand(string ListId, IReadOnlyList<string> Titles) : IRequest<ImportItemsResult>;

/// <summary>The added items, in the order of the titles.</summary>
public sealed record ImportItemsResult(IReadOnlyList<ItemView> Items);
