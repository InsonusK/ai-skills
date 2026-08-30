using Ardalis.Result;
using Shared;

namespace Sample.Interfaces.Commands;

public record DeleteAttachmentCommand(int TaskId, System.Guid AttachmentGuid) : ICommand<Result>;
