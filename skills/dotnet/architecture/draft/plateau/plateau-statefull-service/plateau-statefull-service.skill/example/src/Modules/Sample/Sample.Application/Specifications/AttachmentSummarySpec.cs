using Ardalis.Specification;
using Sample.Domain.Entities;
using Sample.Interfaces.DTOs;

namespace Sample.Application.Specifications;

public sealed class AttachmentSummarySpec : Specification<Attachment, AttachmentSummaryDto>
{
    public AttachmentSummarySpec(System.Guid guid)
    {
        Query.Where(a => a.Guid == guid)
            .Select(a => new AttachmentSummaryDto(a.Id, a.Guid, a.FileName));
    }
}
