using Ardalis.Result;
using Shared.MediatR;

namespace Sample.Interfaces.Queries;

public record GetItemQuery(int ItemId) : IQuery<Result<ItemDto>>;

public record ItemDto(int Id, string Title, bool IsDone, uint Version, DateTimeOffset ServerCreatedDateTime);
