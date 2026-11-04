using MediatR;

namespace Todo.Commands;

/// <summary>
/// Asks to mark the item ItemId of the list ListId done. Version is the item's
/// version the caller read: an item changed since is not completed (skill
/// vp-row-version).
/// </summary>
public sealed record CompleteItemCommand(string ListId, long ItemId, string Version) : IRequest<CompleteItemResult>;

/// <summary>The completed item, with its new version.</summary>
public sealed record CompleteItemResult(ItemView Item);
