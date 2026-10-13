using Ardalis.Result;
using MediatR;
using Sample.Application.Specifications;
using Sample.Domain.Entities;
using Sample.Interfaces.Queries;
using Shared.Repositories;

namespace Sample.Application.Features.GetItem;

public sealed class GetItemHandler(IReadRepository<TodoItem> repository)
    : IRequestHandler<GetItemQuery, Result<ItemDto>>
{
    public async Task<Result<ItemDto>> Handle(GetItemQuery request, CancellationToken ct)
    {
        var item = await repository.FirstOrDefaultAsync(new TodoItemByIdSpec(request.ItemId), ct);
        return item is null
            ? Result<ItemDto>.NotFound()
            : Result.Success(new ItemDto(item.Id, item.Title.Value, item.IsDone, item.Version, item.ServerCreatedDateTime));
    }
}
