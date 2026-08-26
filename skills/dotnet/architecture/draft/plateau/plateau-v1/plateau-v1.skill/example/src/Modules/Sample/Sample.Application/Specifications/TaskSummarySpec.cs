using Ardalis.Specification;
using Sample.Domain.Entities;
using Sample.Interfaces.DTOs;

namespace Sample.Application.Specifications;

public sealed class TaskSummarySpec : Specification<TaskItem, TaskSummaryDto>
{
    public TaskSummarySpec(int id)
    {
        Query.Where(t => t.Id == id)
            .Select(t => new TaskSummaryDto(t.Id, t.Title));
    }
}
