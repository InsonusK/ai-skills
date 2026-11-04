// Source: skill base-plateau-in-dotnet

using System.Security.Cryptography;

namespace BuildingBlocks.Logging;

/// <summary>
/// The id of the request being served, carried along the async calls of that
/// request, so every record of it can be told apart from those of the requests
/// served at the same time. An entry point begins it when a call arrives; the
/// logging variation point writes it into every record.
/// </summary>
public static class RequestId
{
    private static readonly AsyncLocal<string?> CurrentId = new();

    /// <summary>The id <see cref="Begin"/> set for the running call, or null.</summary>
    public static string? Current => CurrentId.Value;

    /// <summary>
    /// Sets the id of the call that runs from here on - an empty
    /// <paramref name="id"/> is replaced by a new random one - and restores
    /// the previous id when disposed.
    /// </summary>
    public static IDisposable Begin(string? id)
    {
        var previous = CurrentId.Value;
        CurrentId.Value = string.IsNullOrEmpty(id) ? New() : id;
        return new Restore(previous);
    }

    /// <summary>A random id of 16 hex characters.</summary>
    public static string New() => Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(8));

    private sealed class Restore(string? previous) : IDisposable
    {
        public void Dispose() => CurrentId.Value = previous;
    }
}
