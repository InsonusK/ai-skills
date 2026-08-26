using Ardalis.Result;
using MediatR;
using Sample.Application.Specifications;
using Sample.Domain.Entities;
using Sample.Interfaces.DTOs;
using Sample.Interfaces.Queries;
using Shared.Repositories;

namespace Sample.Application.Queries.ListTasks;

public class ListTasksHandler(IReadRepository<TaskItem> repository)
    : IRequestHandler<ListTasksQuery, Result<IReadOnlyList<TaskSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<TaskSummaryDto>>> Handle(ListTasksQuery query, CancellationToken ct)
    {
        var dtos = await repository.ListAsync(new TasksSpec(), ct);
        return Result.Success<IReadOnlyList<TaskSummaryDto>>(dtos);
    }
}
