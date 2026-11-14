using System.Globalization;
using MediatR;
using Shared.Persistence;
using Todo.Application.Specifications;
using Todo.Commands;
using Todo.Domain.Entities;
using Todo.Domain.Errors;

namespace Todo.Application.Commands;

/// <summary>
/// Handles <see cref="AddItemCommand"/>: an invalid title is the entity's invalid
/// error, a title already in the list a conflict. The check before the write
/// gives the rule's message; under a race the unique index refuses the second
/// write, and the store failures middleware answers it as a conflict.
/// </summary>
public sealed class AddItemHandler(IRepository<Item> items) : IRequestHandler<AddItemCommand, AddItemResult>
{
    public async Task<AddItemResult> Handle(AddItemCommand command, CancellationToken cancellationToken)
    {
        var item = Item.InList(command.ListId, command.Title);
        if (await items.AnyAsync(new ItemOfListTitled(command.ListId, item.Title), cancellationToken))
        {
            throw TodoErrors.ItemAlreadyInList(command.ListId, item.Title);
        }
        await items.AddAsync(item, cancellationToken);
        return new AddItemResult(new ItemView(item.Title, item.Done, item.Id, item.Version.ToString(CultureInfo.InvariantCulture)));
    }
}
