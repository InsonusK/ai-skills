// Source: skill error-kinds-in-dotnet

namespace Shared.Errors;

/// <summary>
/// An error a caller can act on: the <see cref="Kind"/> says how every entry
/// point answers it, the message says what happened, for the caller. The
/// application never throws it directly: each error is a factory method of
/// its module's errors class (<c>TodoErrors.EmptyTitle()</c>), so the kind and
/// the message of one error are written once.
/// </summary>
public class ErrorKindException(ErrorKind kind, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    /// <summary>The kind of the error, which the entry point answers.</summary>
    public ErrorKind Kind { get; } = kind;

    /// <summary>
    /// The kind of <paramref name="error"/>: of the first
    /// <see cref="ErrorKindException"/> in it or in its chain of inner
    /// exceptions, so an error wrapped on its way up keeps its kind. Null when
    /// it carries none - a failure of the service, which a client is answered
    /// without its message.
    /// </summary>
    public static ErrorKind? KindOf(Exception? error)
    {
        for (var current = error; current is not null; current = current.InnerException)
        {
            if (current is ErrorKindException withKind)
            {
                return withKind.Kind;
            }
        }
        return null;
    }
}
