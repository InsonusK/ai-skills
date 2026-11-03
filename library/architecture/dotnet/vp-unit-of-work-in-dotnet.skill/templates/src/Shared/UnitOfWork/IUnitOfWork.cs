// Source: skill vp-unit-of-work-in-dotnet

namespace Shared.UnitOfWork;

/// <summary>
/// The transaction around one command and the commands it runs, as the command
/// middleware sees it, with no package of the database: every write of the
/// command between <see cref="TryBegin"/> and the commit is kept, or none is.
/// One instance lives per command scope. A service never commits.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// True for the first caller of the command scope, the outermost command,
    /// which then ends the unit; false for a nested command, which joins it.
    /// </summary>
    bool TryBegin();

    /// <summary>
    /// Keeps every write of the unit; a unit with no write does nothing.
    /// A database that does not answer throws <see cref="UnitOfWorkErrors.Unavailable"/>:
    /// the commit may or may not have reached it.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken);

    /// <summary>Drops every write of the unit. A connection already lost has dropped them, so that is no failure.</summary>
    Task RollbackAsync(CancellationToken cancellationToken);
}
