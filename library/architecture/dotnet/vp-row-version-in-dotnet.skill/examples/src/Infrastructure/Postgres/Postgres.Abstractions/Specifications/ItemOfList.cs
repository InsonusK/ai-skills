using Ardalis.Specification;
using Postgres.Abstractions.Rows;

namespace Postgres.Abstractions.Specifications;

/// <summary>One item of a list, by its id.</summary>
public sealed class ItemOfList : Specification<ItemRow>
{
    public ItemOfList(string listId, long id) => Query.Where(item => item.ListId == listId && item.Id == id);
}
