using Ardalis.Specification;
using Sample.Domain.Entities;
using Sample.Interfaces.DTOs;

namespace Sample.Application.Specifications;

public sealed class TaskAttachmentByGuidSpec : Specification<Attachment, AttachmentSummaryDto>
{
    public TaskAttachmentByGuidSpec(int taskId, System.Guid guid)
    {
        Query.Where(a => a.TaskId == taskId && a.Guid == guid)
            .Select(a => new AttachmentSummaryDto(a.Id, a.Guid, a.FileName));
    }
}
