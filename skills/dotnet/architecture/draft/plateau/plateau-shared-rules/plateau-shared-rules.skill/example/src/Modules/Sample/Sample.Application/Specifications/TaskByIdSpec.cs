using Ardalis.Specification;
using Sample.Domain.Entities;

namespace Sample.Application.Specifications;

public sealed class TaskByIdSpec : Specification<TaskItem>
{
    public TaskByIdSpec(int id) => Query.Where(t => t.Id == id);
}
