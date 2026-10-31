using MediatR;
using Todo.Commands;

namespace Todo.Application.Commands;

/// <summary>
/// Handles <see cref="ImportItemsCommand"/> by running one nested
/// <see cref="AddItemCommand"/> per title, one after another: each is logged and
/// checked as any other command, and all of them join the unit of work of this
/// one, so a title already in the list leaves none of the others.
/// </summary>
public sealed class ImportItemsHandler(ISender sender) : IRequestHandler<ImportItemsCommand, ImportItemsResult>
{
    public async Task<ImportItemsResult> Handle(ImportItemsCommand command, CancellationToken cancellationToken)
    {
        var items = new List<ItemView>();
        foreach (var title in command.Titles)
        {
            var added = await sender.Send(new AddItemCommand(command.ListId, title), cancellationToken);
            items.Add(added.Item);
        }
        return new ImportItemsResult(items);
    }
}
