using Ardalis.Result;
using Sample.Interfaces.ValueObjects;
using Shared;

namespace Sample.Interfaces.Commands;

public record CreateTaskCommand(
    string Title,
    int AssigneeId,
    SoftEmail AssigneeEmail
) : ICommand<Result<CreateTaskResult>>;

public record CreateTaskResult(int Id);
