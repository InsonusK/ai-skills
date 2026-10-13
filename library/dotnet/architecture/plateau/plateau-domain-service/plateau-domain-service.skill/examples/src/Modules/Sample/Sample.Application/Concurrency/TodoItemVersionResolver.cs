using Sample.Application.Specifications;
using Sample.Domain.Entities;
using Shared.Concurrency;
using Shared.Repositories;

namespace Sample.Application.Concurrency;

public sealed class TodoItemVersionResolver(IReadRepository<TodoItem> repository) : IEntityVersionResolver
{
    public const string VersionedEntityName = "TodoItem";

    public async Task<int> GetCurrentVersionForAsync(int id, CancellationToken ct = default)
    {
        var item = await repository.FirstOrDefaultAsync(new TodoItemByIdSpec(id), ct);
        return item is null ? 0 : (int)item.Version;
    }
}
