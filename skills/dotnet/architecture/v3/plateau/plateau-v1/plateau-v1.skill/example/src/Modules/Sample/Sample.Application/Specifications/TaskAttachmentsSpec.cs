using Ardalis.Specification;
using Sample.Domain.Entities;
using Sample.Interfaces.DTOs;

namespace Sample.Application.Specifications;

public sealed class TaskAttachmentsSpec : Specification<Attachment, AttachmentSummaryDto>
{
    public TaskAttachmentsSpec(int taskId)
    {
        Query.Where(a => a.TaskId == taskId)
            .Select(a => new AttachmentSummaryDto(a.Id, a.Guid, a.FileName));
    }
}
