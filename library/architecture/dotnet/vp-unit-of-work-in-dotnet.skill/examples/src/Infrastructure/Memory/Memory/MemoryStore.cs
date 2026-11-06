namespace Memory;

/// <summary>
/// The roots every <see cref="MemoryRepository{TRoot}"/> keeps, one list per type
/// of root, for the life of the process. One instance for the application, so
/// the repositories of every command see the same roots.
/// </summary>
public sealed class MemoryStore
{
    private readonly Dictionary<Type, List<object>> _roots = [];

    /// <summary>Held while a repository reads or writes, so concurrent commands see whole roots.</summary>
    internal Lock Writing { get; } = new();

    /// <summary>The roots of <typeparamref name="TRoot"/>, in the order they were added; call it under <see cref="Writing"/>.</summary>
    internal List<object> Of<TRoot>()
    {
        if (!_roots.TryGetValue(typeof(TRoot), out var roots))
        {
            _roots[typeof(TRoot)] = roots = [];
        }
        return roots;
    }
}
