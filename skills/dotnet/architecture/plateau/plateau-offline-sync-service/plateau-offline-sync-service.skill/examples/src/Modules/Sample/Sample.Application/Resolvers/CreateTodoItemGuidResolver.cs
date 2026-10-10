using Ardalis.Result;
using Sample.Application.Specifications;
using Sample.Domain.Entities;
using Sample.Interfaces.Commands;
using Shared.Guid;
using Shared.Repositories;
using Shared.Results;

namespace Sample.Application.Resolvers;

// VP6: one resolver per external-created entity. null -> first request, run the handler;
// non-null ConflictResult -> the Guid already exists, return the existing Id as a 409.
public sealed class CreateTodoItemGuidResolver(IReadRepository<TodoItem> repository)
    : IGuidResolver<Result<AddItemResult>>
{
    public async Task<Result<AddItemResult>?> ResolveAsync(System.Guid guid, CancellationToken ct)
    {
        var existing = await repository.FirstOrDefaultAsync(new TodoItemByGuidSpec(guid), ct);
        return existing is null ? null : new ConflictResult<AddItemResult>(new AddItemResult(existing.Id));
    }
}
