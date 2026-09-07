using Ardalis.Result;
using Sample.Interfaces.ValueObjects;
using Shared.Guid;
using Shared.MediatR;
using Shared.Timestamps;

namespace Sample.Interfaces.Commands;

// Fixed command property order: business fields -> Guid (VP6) -> ActionTimeStamp (VP7).
// TodoItem is "External Mutable": a client generates the Guid offline; the server owns Version.
public record AddItemCommand(SoftItemTitle Title, System.Guid Guid, DateTimeOffset ActionTimeStamp)
    : ICommand<Result<AddItemResult>>, IHasGuid, ICommandWithTimestamp;

public record AddItemResult(int Id);
