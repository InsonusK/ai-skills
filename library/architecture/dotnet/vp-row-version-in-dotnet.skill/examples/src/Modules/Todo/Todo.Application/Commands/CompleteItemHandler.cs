using System.Globalization;
using MediatR;
using Shared.Persistence;
using Todo.Application.Specifications;
using Todo.Commands;
using Todo.Domain.Entities;
using Todo.Domain.Errors;

namespace Todo.Application.Commands;

/// <summary>
/// Handles <see cref="CompleteItemCommand"/>: reads the item untracked
/// (Specifications/ItemsOfList says why), completes it by the entity's rule,
/// and saves it under the version the caller sent — never under the one just
/// read — so a change made since the caller's read is a concurrent change,
/// never overwritten. EF Core compares the version in the write itself; a
/// moved row is answered by the store failures middleware.
/// </summary>
public sealed class CompleteItemHandler(IRepository<Item> items) : IRequestHandler<CompleteItemCommand, CompleteItemResult>
{
    public async Task<CompleteItemResult> Handle(CompleteItemCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.Version))
        {
            throw TodoErrors.NoVersion();
        }
        var item = await items.FirstOrDefaultAsync(new ItemOfListById(command.ListId, command.ItemId), cancellationToken)
            ?? throw TodoErrors.NoItem(command.ListId, command.ItemId);
        item.Complete();
        item.ChangeUnder(command.Version);
        await items.UpdateAsync(item, cancellationToken);
        return new CompleteItemResult(new ItemView(item.Title, item.Done, item.Id, item.Version.ToString(CultureInfo.InvariantCulture)));
    }
}
