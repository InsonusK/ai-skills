using MediatR;

namespace Todo.Commands;

/// <summary>
/// Asks to mark the item <paramref name="ItemId"/> of the list
/// <paramref name="ListId"/> done, under the <paramref name="Version"/> the
/// caller read: an item changed since is not completed (skill vp-row-version).
/// </summary>
public sealed record CompleteItemCommand(string ListId, long ItemId, string Version) : IRequest<CompleteItemResult>;

/// <summary>The completed item, with its new version.</summary>
public sealed record CompleteItemResult(ItemView Item);
