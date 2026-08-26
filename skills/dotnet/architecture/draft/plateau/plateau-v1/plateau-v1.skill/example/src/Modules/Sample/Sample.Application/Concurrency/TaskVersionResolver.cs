using Sample.Application.Specifications;
using Sample.Domain.Entities;
using Shared.Concurrency;
using Shared.Repositories;

namespace Sample.Application.Concurrency;

public sealed class TaskVersionResolver(IReadRepository<TaskItem> repository) : IEntityVersionResolver
{
    public string VersionedEntityName => "Task";

    public async Task<uint?> GetCurrentVersionForAsync(int id, CancellationToken ct)
    {
        var task = await repository.FirstOrDefaultAsync(new TaskByIdSpec(id), ct);
        return task?.Version;
    }
}
