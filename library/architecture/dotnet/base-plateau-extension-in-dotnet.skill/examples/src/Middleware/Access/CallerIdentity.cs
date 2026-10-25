namespace Middleware.Access;

/// <summary>
/// The caller of a command as plain values, the way the API layer passes it to
/// the command layer (skill app-layers, "Identity resolved in the command layer").
/// </summary>
public sealed record Identity(string User, IReadOnlyList<string> Roles)
{
    /// <summary>Whether the caller has <paramref name="role"/>.</summary>
    public bool HasRole(string role) => Roles.Contains(role);
}

/// <summary>
/// The caller of the call being served, carried along its async calls: an
/// API middleware begins it once it has identified the caller, and the access
/// check reads it.
/// </summary>
public static class CallerIdentity
{
    private static readonly AsyncLocal<Identity?> CurrentIdentity = new();

    /// <summary>The caller <see cref="Begin"/> set for the running call, or null when it came without credentials.</summary>
    public static Identity? Current => CurrentIdentity.Value;

    /// <summary>Sets the caller of the call that runs from here on, and restores the previous one when disposed.</summary>
    public static IDisposable Begin(Identity identity)
    {
        var previous = CurrentIdentity.Value;
        CurrentIdentity.Value = identity;
        return new Restore(previous);
    }

    private sealed class Restore(Identity? previous) : IDisposable
    {
        public void Dispose() => CurrentIdentity.Value = previous;
    }
}
