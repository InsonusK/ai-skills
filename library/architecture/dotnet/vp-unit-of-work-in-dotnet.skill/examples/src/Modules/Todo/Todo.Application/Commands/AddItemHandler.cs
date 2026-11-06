using MediatR;
using Shared.Persistence;
using Todo.Commands;
using Todo.Domain.Entities;
using Todo.Domain.Specifications;

namespace Todo.Application.Commands;

/// <summary>
/// Handles <see cref="AddItemCommand"/>: loads the list, or starts it on its
/// first item, lets the list add the item, and saves the list with its items.
/// An invalid title is the entity's invalid error, a title already in the list
/// the list's conflict.
/// </summary>
public sealed class AddItemHandler(IRepository<ItemList> lists) : IRequestHandler<AddItemCommand, AddItemResult>
{
    public async Task<AddItemResult> Handle(AddItemCommand command, CancellationToken cancellationToken)
    {
        var item = new Item(command.Title);
        var list = await lists.SingleOrDefaultAsync(new ListById(command.ListId), cancellationToken);
        if (list is null)
        {
            list = new ItemList(command.ListId);
            list.Add(item);
            await lists.AddAsync(list, cancellationToken);
        }
        else
        {
            list.Add(item);
            await lists.UpdateAsync(list, cancellationToken);
        }
        return new AddItemResult(new ItemView(item.Title, item.Done));
    }
}
