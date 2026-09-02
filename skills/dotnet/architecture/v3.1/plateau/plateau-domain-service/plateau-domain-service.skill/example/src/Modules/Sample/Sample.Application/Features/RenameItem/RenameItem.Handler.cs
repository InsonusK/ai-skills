using Ardalis.Result;
using MediatR;
using Sample.Application.Specifications;
using Sample.Domain.Entities;
using Sample.Domain.ValueObjects;
using Sample.Interfaces.Commands;
using Shared.Repositories;

namespace Sample.Application.Features.RenameItem;

// Concurrency is already checked by ConcurrencyBehavior before this runs.
public sealed class RenameItemHandler(IRepository<TodoItem> repository)
    : IRequestHandler<RenameItemCommand, Result>
{
    public async Task<Result> Handle(RenameItemCommand request, CancellationToken ct)
    {
        var item = await repository.FirstOrDefaultAsync(new TodoItemByIdSpec(request.ItemId), ct);
        if (item is null)
            return Result.NotFound();

        item.Rename(new ItemTitle(request.NewTitle.Value));  // domain guard: throws if completed
        item.RecordUpdatedByUser(request.ActionTimeStamp);
        await repository.UpdateAsync(item, ct);

        return Result.Success();
    }
}
