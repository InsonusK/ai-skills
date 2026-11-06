using MediatR;
using Todo.Commands;
using Todo.Domain.Entities;

namespace Todo.Application.Commands;

/// <summary>
/// Handles <see cref="AddItemCommand"/>: lets the list add the item. An invalid
/// title is the entity's invalid error, a title already in the list the list's
/// conflict.
/// </summary>
public sealed class AddItemHandler(KeptLists lists) : IRequestHandler<AddItemCommand, AddItemResult>
{
    public Task<AddItemResult> Handle(AddItemCommand command, CancellationToken cancellationToken)
    {
        var item = new Item(command.Title);
        lists.Change(command.ListId, list => list.Add(item));
        return Task.FromResult(new AddItemResult(new ItemView(item.Title, item.Done)));
    }
}
