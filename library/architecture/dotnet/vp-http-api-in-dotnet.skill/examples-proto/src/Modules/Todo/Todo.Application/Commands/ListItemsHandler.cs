using MediatR;
using Todo.Application.Services;
using Todo.Commands;

namespace Todo.Application.Commands;

/// <summary>Handles <see cref="ListItems"/>: an unknown list is the service's not found.</summary>
public sealed class ListItemsHandler(IItemLists lists) : IRequestHandler<ListItems, ListItemsResult>
{
    public async Task<ListItemsResult> Handle(ListItems command, CancellationToken cancellationToken)
    {
        var items = await lists.ItemsAsync(command.ListId, cancellationToken);
        return new ListItemsResult(items.Select(item => new ItemView(item.Title, item.Done)).ToList());
    }
}
