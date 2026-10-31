using MediatR;

namespace Todo.Commands;

/// <summary>Asks to add an open item with <paramref name="Title"/> to the list <paramref name="ListId"/>.</summary>
public sealed record AddItemCommand(string ListId, string Title) : IRequest<AddItemResult>;

/// <summary>The added item.</summary>
public sealed record AddItemResult(ItemView Item);
