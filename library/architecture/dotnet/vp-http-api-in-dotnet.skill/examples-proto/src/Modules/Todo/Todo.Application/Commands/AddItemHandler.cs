using MediatR;
using Todo.Commands;
using Todo.Domain.Entities;

namespace Todo.Application.Commands;

/// <summary>
/// Handles <see cref="AddItemCommand"/>: keeps the item in its list. An invalid
/// title is the entity's invalid error, a title already in the list a conflict.
/// </summary>
public sealed class AddItemHandler(KeptLists lists) : IRequestHandler<AddItemCommand, AddItemResult>
{
    public Task<AddItemResult> Handle(AddItemCommand command, CancellationToken cancellationToken)
    {
        var item = new Item(command.Title);
        lists.Add(command.ListId, item);
        return Task.FromResult(new AddItemResult(new ItemView(item.Title, item.Done)));
    }
}
