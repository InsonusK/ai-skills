using Ardalis.Specification;
using Sample.Domain.Entities;

namespace Sample.Application.Specifications;

public sealed class TodoItemByIdSpec : Specification<TodoItem>
{
    public TodoItemByIdSpec(int id) => Query.Where(e => e.Id == id);
}
