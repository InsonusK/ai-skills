namespace Shared.Access;

// The marks of the sample access variation point: the decorators of the
// Access project select a command by them. They live in Shared, which
// references nothing, so a module's Commands project marks a command without
// referencing the variation point.

/// <summary>
/// The mark of a command only a caller with a role may run; the access check
/// (<c>AccessCheckBehavior</c>) rejects it for any other caller.
/// </summary>
public interface IRestricted
{
    /// <summary>The role the caller needs.</summary>
    string RequiredRole { get; }
}

/// <summary>
/// The mark of a command that works on many items at once; the batch limit
/// (<c>BatchLimitBehavior</c>) rejects it above the limit.
/// </summary>
public interface IBatch
{
    /// <summary>How many items the command holds.</summary>
    int BatchSize { get; }
}
