using Ardalis.Result;
using MediatR;
using Sample.Domain.Entities;
using Sample.Domain.ValueObjects;
using Sample.Interfaces.Commands;
using Shared.Repositories;

namespace Sample.Application.Features.AddItem;

public sealed class AddItemHandler(IRepository<TodoItem> repository)
    : IRequestHandler<AddItemCommand, Result<AddItemResult>>
{
    public async Task<Result<AddItemResult>> Handle(AddItemCommand request, CancellationToken ct)
    {
        var item = TodoItem.Create(new ItemTitle(request.Title.Value));
        item.RecordCreatedByUser(request.ActionTimeStamp);

        await repository.AddAsync(item, ct);
        return Result.Success(new AddItemResult(item.Id));
    }
}
