using Ardalis.Result;
using Sample.Interfaces.ValueObjects;
using Shared;
using Shared.Timestamps;

namespace Sample.Interfaces.Commands;

public record CreateTaskCommand(
    SoftTitle Title,
    int AssigneeId,
    SoftEmail AssigneeEmail,
    DateTimeOffset ActionTimeStamp,
    DateTimeOffset? StartDateTime = null,
    DateTimeOffset? DueDateTime = null
) : ICommand<Result<CreateTaskResult>>, ICommandWithTimestamp;

public record CreateTaskResult { }
