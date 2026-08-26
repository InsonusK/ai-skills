using Ardalis.Result;
using Shared;
using Shared.Guid;
using Shared.Timestamps;

namespace Sample.Interfaces.Commands;

public record CreateAttachmentCommand(
    System.Guid Guid,
    int TaskId,
    string FileName,
    DateTimeOffset ActionTimeStamp
) : ICommand<Result<CreateAttachmentResult>>, IHasGuid, ICommandWithTimestamp;

public record CreateAttachmentResult { }
