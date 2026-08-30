using Ardalis.Result;
using MediatR;
using Sample.Domain.Entities;
using Sample.Interfaces.DTOs;
using Sample.Interfaces.Queries;
using Sample.Application.Specifications;
using Shared.Repositories;

namespace Sample.Application.Queries.GetTaskSummary;

public class GetTaskSummaryHandler(IReadRepository<TaskItem> repository)
    : IRequestHandler<GetTaskSummaryQuery, Result<TaskSummaryDto>>
{
    public async Task<Result<TaskSummaryDto>> Handle(GetTaskSummaryQuery query, CancellationToken ct)
    {
        var dto = await repository.FirstOrDefaultAsync(new TaskSummarySpec(query.TaskId), ct);
        return dto is null ? Result.NotFound() : Result.Success(dto);
    }
}
