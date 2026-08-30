using Ardalis.Specification;
using Sample.Domain.Entities;
using Sample.Interfaces.DTOs;

namespace Sample.Application.Specifications;

public sealed class TaskScheduleSpec : Specification<TaskItem, TaskScheduleDto>
{
    public TaskScheduleSpec(int taskId)
    {
        Query.Where(t => t.Id == taskId)
            .Select(t => new TaskScheduleDto(t.StartDateTime, t.DueDateTime));
    }
}
