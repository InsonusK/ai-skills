using Ardalis.Specification;
using Sample.Domain.Entities;

namespace Sample.Application.Specifications;

public sealed class AttachmentByGuidSpec : Specification<Attachment>
{
    public AttachmentByGuidSpec(System.Guid guid) => Query.Where(a => a.Guid == guid);
}
