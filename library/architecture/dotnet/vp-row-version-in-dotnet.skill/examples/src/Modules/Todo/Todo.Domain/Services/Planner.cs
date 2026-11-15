using Todo.Domain.Entities;
using Todo.Domain.Errors;

namespace Todo.Domain.Services;

/// <summary>
/// A domain service: it holds the rules that span several items, which no
/// single item can check on its own. A domain service calls entities and,
/// through interfaces it declares itself, services - never infrastructure.
/// </summary>
public sealed class Planner
{
    /// <summary>
    /// Creates one open item per title; titles must be valid items and must not
    /// repeat, compared case-insensitively after trimming.
    /// </summary>
    public IReadOnlyList<Item> Plan(IReadOnlyList<string> titles)
    {
        if (titles.Count == 0)
        {
            throw TodoErrors.NoItems();
        }
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var items = new List<Item>(titles.Count);
        foreach (var title in titles)
        {
            var item = new Item(title);
            if (!seen.Add(item.Title))
            {
                throw TodoErrors.DuplicateTitle();
            }
            items.Add(item);
        }
        return items;
    }
}
