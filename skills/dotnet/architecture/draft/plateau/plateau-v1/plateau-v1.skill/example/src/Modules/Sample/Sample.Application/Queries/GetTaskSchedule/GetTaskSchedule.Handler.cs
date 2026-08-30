using Ardalis.Result;
using MediatR;
using Sample.Application.Specifications;
using Sample.Domain.Entities;
using Sample.Interfaces.DTOs;
using Sample.Interfaces.Queries;
using Shared.Repositories;

namespace Sample.Application.Queries.GetTaskSchedule;

public class GetTaskScheduleHandler(IReadRepository<TaskItem> repository)
    : IRequestHandler<GetTaskScheduleQuery, Result<TaskScheduleDto>>
{
    public async Task<Result<TaskScheduleDto>> Handle(GetTaskScheduleQuery query, CancellationToken ct)
    {
        var dto = await repository.FirstOrDefaultAsync(new TaskScheduleSpec(query.TaskId), ct);
        return dto is null ? Result.NotFound() : Result.Success(dto);
    }
}
