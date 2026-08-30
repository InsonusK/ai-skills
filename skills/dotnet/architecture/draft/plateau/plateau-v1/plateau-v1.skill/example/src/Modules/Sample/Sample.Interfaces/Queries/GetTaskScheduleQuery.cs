using Ardalis.Result;
using Sample.Interfaces.DTOs;
using Shared;

namespace Sample.Interfaces.Queries;

public record GetTaskScheduleQuery(int TaskId) : IQuery<Result<TaskScheduleDto>>;
