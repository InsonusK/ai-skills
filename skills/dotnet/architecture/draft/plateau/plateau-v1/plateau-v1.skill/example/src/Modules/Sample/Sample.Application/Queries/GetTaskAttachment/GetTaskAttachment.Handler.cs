using Ardalis.Result;
using MediatR;
using Sample.Application.Specifications;
using Sample.Domain.Entities;
using Sample.Interfaces.DTOs;
using Sample.Interfaces.Queries;
using Shared.Repositories;

namespace Sample.Application.Queries.GetTaskAttachment;

public class GetTaskAttachmentHandler(IReadRepository<Attachment> repository)
    : IRequestHandler<GetTaskAttachmentQuery, Result<AttachmentSummaryDto>>
{
    public async Task<Result<AttachmentSummaryDto>> Handle(GetTaskAttachmentQuery query, CancellationToken ct)
    {
        var dto = await repository.FirstOrDefaultAsync(new TaskAttachmentByGuidSpec(query.TaskId, query.AttachmentGuid), ct);
        return dto is null ? Result.NotFound() : Result.Success(dto);
    }
}
