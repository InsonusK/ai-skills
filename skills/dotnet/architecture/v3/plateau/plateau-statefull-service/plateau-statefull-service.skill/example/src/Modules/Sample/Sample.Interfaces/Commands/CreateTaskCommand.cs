using Ardalis.Result;
using Sample.Interfaces.ValueObjects;
using Shared;
using Shared.Timestamps;

namespace Sample.Interfaces.Commands;

public record CreateTaskCommand(
    string Title,
    int AssigneeId,
    SoftEmail AssigneeEmail,
    DateTimeOffset ActionTimeStamp
) : ICommand<Result<CreateTaskResult>>, ICommandWithTimestamp;

public record CreateTaskResult { }
