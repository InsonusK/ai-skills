namespace Shared.Concurrency;

// An update command carries the client's expected versions:
// entity name -> (entity id -> expected version).
public interface IHasVersions
{
    IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>> Versions { get; }
}
