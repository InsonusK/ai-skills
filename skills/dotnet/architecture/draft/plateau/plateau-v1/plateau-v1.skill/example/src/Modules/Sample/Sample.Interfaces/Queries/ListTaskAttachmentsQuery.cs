using Ardalis.Result;
using Sample.Interfaces.DTOs;
using Shared;

namespace Sample.Interfaces.Queries;

public record ListTaskAttachmentsQuery(int TaskId) : IQuery<Result<IReadOnlyList<AttachmentSummaryDto>>>;
