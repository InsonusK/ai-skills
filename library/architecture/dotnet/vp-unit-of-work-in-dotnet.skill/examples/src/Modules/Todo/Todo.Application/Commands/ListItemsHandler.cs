using MediatR;
using Shared.Persistence;
using Todo.Application.Specifications;
using Todo.Commands;
using Todo.Domain.Entities;
using Todo.Domain.Errors;

namespace Todo.Application.Commands;

/// <summary>Handles <see cref="ListItemsCommand"/>: a list nobody added to is not found.</summary>
public sealed class ListItemsHandler(IReadRepository<Item> items) : IRequestHandler<ListItemsCommand, ListItemsResult>
{
    public async Task<ListItemsResult> Handle(ListItemsCommand command, CancellationToken cancellationToken)
    {
        var listed = await items.ListAsync(new ItemsOfList(command.ListId), cancellationToken);
        if (listed.Count == 0)
        {
            throw TodoErrors.NoList(command.ListId);
        }
        return new ListItemsResult(listed.Select(item => new ItemView(item.Title, item.Done)).ToList());
    }
}
