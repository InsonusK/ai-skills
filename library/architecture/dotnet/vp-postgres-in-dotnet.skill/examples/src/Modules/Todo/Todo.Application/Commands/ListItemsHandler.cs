using MediatR;
using Shared.Persistence;
using Todo.Commands;
using Todo.Domain.Entities;
using Todo.Domain.Errors;
using Todo.Domain.Specifications;

namespace Todo.Application.Commands;

/// <summary>Handles <see cref="ListItemsCommand"/>: a list nobody added to is not found.</summary>
public sealed class ListItemsHandler(IReadRepository<ItemList> lists) : IRequestHandler<ListItemsCommand, ListItemsResult>
{
    public async Task<ListItemsResult> Handle(ListItemsCommand command, CancellationToken cancellationToken)
    {
        var list = await lists.SingleOrDefaultAsync(new ListById(command.ListId), cancellationToken)
            ?? throw TodoErrors.NoList(command.ListId);
        return new ListItemsResult(list.Items.Select(item => new ItemView(item.Title, item.Done)).ToList());
    }
}
