using MediatR;
using Todo.Application.Services;
using Todo.Commands;
using Todo.Domain.Entities;

namespace Todo.Application.Commands;

/// <summary>
/// Handles <see cref="AddItem"/>: an invalid title is the entity's invalid
/// error, a title already in the list the service's conflict.
/// </summary>
public sealed class AddItemHandler(IItemLists lists) : IRequestHandler<AddItem, AddItemResult>
{
    public async Task<AddItemResult> Handle(AddItem command, CancellationToken cancellationToken)
    {
        var item = new Item(command.Title);
        await lists.AddAsync(command.ListId, item, cancellationToken);
        return new AddItemResult(new ItemView(item.Title, item.Done));
    }
}
