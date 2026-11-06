// Source: skill vp-postgres-in-dotnet

using Ardalis.Specification;

namespace Shared.Persistence;

/// <summary>
/// The rule every repository keeps for a page: a specification with
/// <c>Skip</c> or <c>Take</c> serves a list only. A count, an existence check, or
/// a delete over it would apply its conditions and ignore its page, and so work
/// on other roots than the list of the same specification.
/// </summary>
public static class SpecificationPage
{
    /// <exception cref="ArgumentException">The specification holds a page.</exception>
    public static void Refuse<TRoot>(ISpecification<TRoot> specification)
    {
        if (specification.Skip > 0 || specification.Take >= 0)
        {
            throw new ArgumentException("a specification with a page serves a list; a count, an existence check, or a delete takes one without", nameof(specification));
        }
    }
}
