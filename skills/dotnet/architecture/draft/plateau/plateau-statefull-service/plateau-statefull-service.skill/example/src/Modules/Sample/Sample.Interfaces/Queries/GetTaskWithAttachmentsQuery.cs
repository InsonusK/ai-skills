using Ardalis.Result;
using Sample.Interfaces.DTOs;
using Shared;

namespace Sample.Interfaces.Queries;

public record GetTaskWithAttachmentsQuery(int TaskId) : IQuery<Result<TaskWithAttachmentsDto>>;
