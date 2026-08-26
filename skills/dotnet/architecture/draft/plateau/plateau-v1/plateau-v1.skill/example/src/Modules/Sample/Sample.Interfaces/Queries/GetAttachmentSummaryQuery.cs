using Ardalis.Result;
using Sample.Interfaces.DTOs;
using Shared;

namespace Sample.Interfaces.Queries;

public record GetAttachmentSummaryQuery(System.Guid AttachmentGuid) : IQuery<Result<AttachmentSummaryDto>>;
