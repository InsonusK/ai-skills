using Ardalis.Specification;
using Sample.Domain.Entities;

namespace Sample.Application.Specifications;

public sealed class TodoItemByGuidSpec : Specification<TodoItem>
{
    public TodoItemByGuidSpec(System.Guid guid) => Query.Where(e => e.Guid == guid);
}
