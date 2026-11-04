using MediatR;
using Todo.Application.Services;
using Todo.Commands;
using Todo.Domain.Entities;

namespace Todo.Application.Commands;

/// <summary>
/// Handles <see cref="AddItemCommand"/>: an invalid title is the entity's invalid
/// error, a title already in the list the service's conflict.
/// </summary>
public sealed class AddItemHandler(IItemLists lists) : IRequestHandler<AddItemCommand, AddItemResult>
{
    public async Task<AddItemResult> Handle(AddItemCommand command, CancellationToken cancellationToken)
    {
        var item = new Item(command.Title);
        var kept = await lists.AddAsync(command.ListId, item, cancellationToken);
        // The added item answers its id and its version: the caller's first
        // change of it gives that version back.
        return new AddItemResult(CompleteItemHandler.View(kept));
    }
}
