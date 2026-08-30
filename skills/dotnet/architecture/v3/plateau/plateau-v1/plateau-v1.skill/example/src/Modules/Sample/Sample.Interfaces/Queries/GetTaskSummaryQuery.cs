using Ardalis.Result;
using Sample.Interfaces.DTOs;
using Shared;

namespace Sample.Interfaces.Queries;

public record GetTaskSummaryQuery(int TaskId) : IQuery<Result<TaskSummaryDto>>;
