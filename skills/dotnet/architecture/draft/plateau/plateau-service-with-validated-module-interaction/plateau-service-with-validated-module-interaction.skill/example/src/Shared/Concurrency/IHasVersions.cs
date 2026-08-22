namespace Shared.Concurrency;

public interface IHasVersions
{
    IReadOnlyCollection<uint> Versions { get; }
}
