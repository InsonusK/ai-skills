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
