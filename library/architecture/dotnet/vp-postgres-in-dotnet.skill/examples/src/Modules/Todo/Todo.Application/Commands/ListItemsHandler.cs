using MediatR;
using TodoExampleService.Shared.Persistence;
using TodoExampleService.Modules.Todo.Application.Specifications;
using TodoExampleService.Modules.Todo.Commands;
using TodoExampleService.Modules.Todo.Domain.Entities;
using TodoExampleService.Modules.Todo.Domain.Errors;

namespace TodoExampleService.Modules.Todo.Application.Commands;

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
