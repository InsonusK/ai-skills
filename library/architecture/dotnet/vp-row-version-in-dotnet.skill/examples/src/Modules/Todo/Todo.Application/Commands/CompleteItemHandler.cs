using MediatR;
using Todo.Application.Services;
using Todo.Commands;
using Todo.Domain.Entities;
using Todo.Domain.Errors;

namespace Todo.Application.Commands;

/// <summary>
/// Handles <see cref="CompleteItemCommand"/>: reads the item, completes it by the
/// entity's rule, and saves it under the caller's version - not the one just
/// read - so a change made since the caller's read is a concurrent change, never
/// overwritten.
/// </summary>
public sealed class CompleteItemHandler(IItemLists lists) : IRequestHandler<CompleteItemCommand, CompleteItemResult>
{
    public async Task<CompleteItemResult> Handle(CompleteItemCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.Version))
        {
            throw TodoErrors.NoVersion();
        }
        var kept = await lists.ItemAsync(command.ListId, command.ItemId, cancellationToken);
        kept.Item.Complete();
        kept.Version = command.Version;
        var saved = await lists.SaveAsync(command.ListId, kept, cancellationToken);
        return new CompleteItemResult(View(saved));
    }

    /// <summary>The view of an item kept in a list: its fields, its id, its version.</summary>
    internal static ItemView View(KeptItem kept) => new(kept.Item.Title, kept.Item.Done, kept.Id, kept.Version);
}
