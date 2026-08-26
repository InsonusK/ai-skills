using Ardalis.Specification;
using Sample.Domain.Entities;
using Sample.Interfaces.DTOs;

namespace Sample.Application.Specifications;

public sealed class TasksSpec : Specification<TaskItem, TaskSummaryDto>
{
    public TasksSpec()
    {
        Query.Select(t => new TaskSummaryDto(t.Id, t.Title));
    }
}
