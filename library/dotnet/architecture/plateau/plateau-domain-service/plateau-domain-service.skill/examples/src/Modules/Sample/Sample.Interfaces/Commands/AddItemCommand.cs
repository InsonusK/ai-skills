using Ardalis.Result;
using Sample.Interfaces.ValueObjects;
using Shared.MediatR;
using Shared.Timestamps;

namespace Sample.Interfaces.Commands;

// Business field first, then ActionTimeStamp (VP7 slot). No Guid (no VP6), no version (create).
public record AddItemCommand(SoftItemTitle Title, DateTimeOffset ActionTimeStamp)
    : ICommand<Result<AddItemResult>>, ICommandWithTimestamp;

public record AddItemResult(int Id);
