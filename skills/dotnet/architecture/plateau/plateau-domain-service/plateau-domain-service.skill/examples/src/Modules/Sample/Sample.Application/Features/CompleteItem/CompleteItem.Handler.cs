using Ardalis.Result;
using MediatR;
using Sample.Application.Specifications;
using Sample.Domain.Entities;
using Sample.Interfaces.Commands;
using Sample.Interfaces.Events;
using Shared.Repositories;

namespace Sample.Application.Features.CompleteItem;

public sealed class CompleteItemHandler(IRepository<TodoItem> repository, IPublisher publisher)
    : IRequestHandler<CompleteItemCommand, Result>
{
    public async Task<Result> Handle(CompleteItemCommand request, CancellationToken ct)
    {
        var item = await repository.FirstOrDefaultAsync(new TodoItemByIdSpec(request.ItemId), ct);
        if (item is null)
            return Result.NotFound();

        item.Complete();                                   // domain guard: throws if already done
        item.RecordUpdatedByUser(request.ActionTimeStamp);
        await repository.UpdateAsync(item, ct);

        await publisher.Publish(new ItemCompleted(item.Id, request.ActionTimeStamp), ct);
        return Result.Success();
    }
}
