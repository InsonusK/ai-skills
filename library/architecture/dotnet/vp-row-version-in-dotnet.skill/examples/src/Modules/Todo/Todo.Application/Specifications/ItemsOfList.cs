using Ardalis.Specification;
using Todo.Domain.Entities;

namespace Todo.Application.Specifications;

/// <summary>The items of one list, in the order they were added.</summary>
public sealed class ItemsOfList : Specification<Item>
{
    public ItemsOfList(string listId) => Query.Where(item => item.ListId == listId).OrderBy(item => item.Id);
}

/// <summary>The item of one list with one title: at most one, by the list's unique index.</summary>
public sealed class ItemOfListTitled : Specification<Item>
{
    public ItemOfListTitled(string listId, string title) => Query.Where(item => item.ListId == listId && item.Title == title);
}

/// <summary>
/// One item of a list by its id, read untracked (skill vp-row-version): the
/// change that follows is saved under the version the caller read, which
/// DbContext.Update takes as the original, so a tracked read would compare
/// the version just read instead and the check would cover the command's own
/// milliseconds only.
/// </summary>
public sealed class ItemOfListById : Specification<Item>, ISingleResultSpecification<Item>
{
    public ItemOfListById(string listId, long itemId) =>
        Query.Where(item => item.ListId == listId && item.Id == itemId).AsNoTracking();
}
