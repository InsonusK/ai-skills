// Source: skill error-kinds-in-dotnet

namespace Shared.Errors;

/// <summary>
/// The error kinds of the application (skill error-kinds). Every error a
/// command raises on purpose is an <see cref="ErrorKindException"/> carrying
/// exactly one of them, so an entry point answers it by its kind (status code,
/// exit code) without knowing the individual error. An exception that carries
/// none is a failure of the service.
/// </summary>
public enum ErrorKind
{
    /// <summary>The input breaks a rule whatever the state (an empty title).</summary>
    Invalid,
    /// <summary>The object the input names does not exist.</summary>
    NotFound,
    /// <summary>The object to create already exists (the name is taken).</summary>
    Conflict,
    /// <summary>The object was changed meanwhile; the whole operation may be retried.</summary>
    ConcurrentChange,
    /// <summary>The input is valid, but the state does not allow the operation (the item is already done).</summary>
    FailedPrecondition,
    /// <summary>The caller is not identified.</summary>
    Unauthenticated,
    /// <summary>The caller is identified but may not do this.</summary>
    PermissionDenied,
    /// <summary>A quota or rate limit of the caller is used up.</summary>
    ResourceExhausted,
    /// <summary>The operation is not offered (deprecated, or not yet).</summary>
    Unimplemented,
    /// <summary>A dependency is down for now; a retry may succeed.</summary>
    Unavailable,
}
