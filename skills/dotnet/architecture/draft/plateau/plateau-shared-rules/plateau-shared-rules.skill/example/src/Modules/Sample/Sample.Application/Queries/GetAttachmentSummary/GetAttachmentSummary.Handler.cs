using Ardalis.Result;
using MediatR;
using Sample.Domain.Entities;
using Sample.Interfaces.DTOs;
using Sample.Interfaces.Queries;
using Sample.Application.Specifications;
using Shared.Repositories;

namespace Sample.Application.Queries.GetAttachmentSummary;

public class GetAttachmentSummaryHandler(IReadRepository<Attachment> repository)
    : IRequestHandler<GetAttachmentSummaryQuery, Result<AttachmentSummaryDto>>
{
    public async Task<Result<AttachmentSummaryDto>> Handle(GetAttachmentSummaryQuery query, CancellationToken ct)
    {
        var dto = await repository.FirstOrDefaultAsync(new AttachmentSummarySpec(query.AttachmentGuid), ct);
        return dto is null ? Result.NotFound() : Result.Success(dto);
    }
}
