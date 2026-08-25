using Ardalis.Result;
using Sample.Interfaces.ValueObjects;
using Shared;

namespace Sample.Interfaces.Commands;

public record CreateTaskCommand(
    SoftTitle Title,
    int AssigneeId,
    SoftEmail AssigneeEmail,
    DateTimeOffset? StartDateTime = null,
    DateTimeOffset? DueDateTime = null
) : ICommand<CreateTaskResult>;

public record CreateTaskResult(int Id);
