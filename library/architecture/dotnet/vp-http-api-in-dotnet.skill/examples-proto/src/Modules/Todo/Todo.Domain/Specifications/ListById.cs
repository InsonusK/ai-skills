using Ardalis.Specification;
using Todo.Domain.Entities;

namespace Todo.Domain.Specifications;

/// <summary>The list the caller named, with its items: at most one, so it serves <c>SingleOrDefaultAsync</c>.</summary>
public sealed class ListById : Specification<ItemList>, ISingleResultSpecification<ItemList>
{
    public ListById(string listId) => Query.Where(list => list.Id == listId);
}
