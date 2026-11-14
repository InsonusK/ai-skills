using MediatR;
using Todo.Commands;
using Todo.Domain.Errors;

namespace Todo.Application.Commands;

/// <summary>Handles <see cref="ListItemsCommand"/>: a list nobody added to is not found.</summary>
public sealed class ListItemsHandler(KeptLists lists) : IRequestHandler<ListItemsCommand, ListItemsResult>
{
    public Task<ListItemsResult> Handle(ListItemsCommand command, CancellationToken cancellationToken)
    {
        var items = lists.Items(command.ListId) ?? throw TodoErrors.NoList(command.ListId);
        return Task.FromResult(new ListItemsResult(items.Select(item => new ItemView(item.Title, item.Done)).ToList()));
    }
}
