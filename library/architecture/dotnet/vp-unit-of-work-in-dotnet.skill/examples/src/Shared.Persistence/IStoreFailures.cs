// Source: skill vp-postgres-in-dotnet

using Shared.Errors;

namespace Shared.Persistence;

/// <summary>
/// What a failure of the store means for the caller: the store's variation
/// point implements it, the command middleware of the same name asks it, so
/// the codes of the engine are read in one place and no handler reads them.
/// </summary>
public interface IStoreFailures
{
    /// <summary>
    /// The error with the kind <paramref name="failure"/> tells, keeping it as the
    /// cause; null for a failure with no kind, a defect that stays as it is, and
    /// for an error that already has one.
    /// </summary>
    ErrorKindException? KindOf(Exception failure);
}
