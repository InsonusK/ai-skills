using Ardalis.Result;
using Sample.Interfaces.DTOs;
using Shared;

namespace Sample.Interfaces.Queries;

public record ListTasksQuery : IQuery<Result<IReadOnlyList<TaskSummaryDto>>>;
