using Ardalis.Specification;
using Postgres.Abstractions.Rows;

namespace Postgres.Abstractions.Specifications;

/// <summary>The rows of one list. It says which rows; what is done with them, and in what order, the selector says.</summary>
public sealed class ItemsOfList : Specification<ItemRow>
{
    public ItemsOfList(string listId) => Query.Where(item => item.ListId == listId);
}
