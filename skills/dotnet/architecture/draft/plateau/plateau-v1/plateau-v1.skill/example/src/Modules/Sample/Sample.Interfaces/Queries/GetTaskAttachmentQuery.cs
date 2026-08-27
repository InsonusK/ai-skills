using Ardalis.Result;
using Sample.Interfaces.DTOs;
using Shared;

namespace Sample.Interfaces.Queries;

public record GetTaskAttachmentQuery(int TaskId, System.Guid AttachmentGuid) : IQuery<Result<AttachmentSummaryDto>>;
