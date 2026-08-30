using Ardalis.Specification;
using Sample.Domain.Entities;

namespace Sample.Application.Specifications;

public sealed class AttachmentByTaskAndGuidSpec : Specification<Attachment>
{
    public AttachmentByTaskAndGuidSpec(int taskId, System.Guid guid)
        => Query.Where(a => a.TaskId == taskId && a.Guid == guid);
}
