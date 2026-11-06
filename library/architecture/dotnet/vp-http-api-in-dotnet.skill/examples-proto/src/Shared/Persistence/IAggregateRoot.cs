namespace Shared.Persistence;

/// <summary>
/// Marks the root of an aggregate: the one entity of it a repository loads and
/// saves. Its children are loaded and saved with it, never on their own, so the
/// rules the root guards over them hold in what is stored.
/// </summary>
public interface IAggregateRoot
{
}
