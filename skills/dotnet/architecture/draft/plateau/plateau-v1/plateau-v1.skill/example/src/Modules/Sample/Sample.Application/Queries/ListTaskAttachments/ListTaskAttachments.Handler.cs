using Ardalis.Result;
using MediatR;
using Sample.Application.Specifications;
using Sample.Domain.Entities;
using Sample.Interfaces.DTOs;
using Sample.Interfaces.Queries;
using Shared.Repositories;

namespace Sample.Application.Queries.ListTaskAttachments;

public class ListTaskAttachmentsHandler(IReadRepository<Attachment> repository)
    : IRequestHandler<ListTaskAttachmentsQuery, Result<IReadOnlyList<AttachmentSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<AttachmentSummaryDto>>> Handle(ListTaskAttachmentsQuery query, CancellationToken ct)
    {
        var dtos = await repository.ListAsync(new TaskAttachmentsSpec(query.TaskId), ct);
        return Result.Success<IReadOnlyList<AttachmentSummaryDto>>(dtos);
    }
}
