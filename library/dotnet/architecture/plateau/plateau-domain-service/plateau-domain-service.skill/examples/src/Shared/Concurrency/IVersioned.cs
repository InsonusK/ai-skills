namespace Shared.Concurrency;

// Marker for a mutable entity that carries a concurrency version.
public interface IVersioned
{
    uint Version { get; }
}
