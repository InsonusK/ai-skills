// Source: skill error-kinds-in-dotnet

namespace Shared.Errors;

/// <summary>
/// An error a caller can act on: the <see cref="Kind"/> says how every entry
/// point answers it, the message says what happened, for the caller, and the
/// inner exception, when given, is its cause - the technical failure behind
/// it, which only the log writes. The application never throws it directly: each error is a factory method of
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
    public static ErrorKind? KindOf(Exception? error) => ErrorOf(error)?.Kind;

    /// <summary>
    /// The first <see cref="ErrorKindException"/> in <paramref name="error"/>
    /// or in its chain of inner exceptions, whose message an entry point
    /// answers; null when there is none.
    /// </summary>
    public static ErrorKindException? ErrorOf(Exception? error)
    {
        for (var current = error; current is not null; current = current.InnerException)
        {
            if (current is ErrorKindException withKind)
            {
                return withKind;
            }
        }
        return null;
    }

    /// <summary>
    /// The cause of the first <see cref="ErrorKindException"/> in
    /// <paramref name="error"/>, as text for the log: the messages of its chain
    /// of inner exceptions joined by ": ". Null when the error has no kind or
    /// its error with a kind has no inner exception.
    /// </summary>
    public static string? CauseOf(Exception? error)
    {
        var cause = ErrorOf(error)?.InnerException;
        if (cause is null)
        {
            return null;
        }
        var messages = new List<string>();
        for (var current = cause; current is not null; current = current.InnerException)
        {
            messages.Add(current.Message);
        }
        return string.Join(": ", messages);
    }
}
