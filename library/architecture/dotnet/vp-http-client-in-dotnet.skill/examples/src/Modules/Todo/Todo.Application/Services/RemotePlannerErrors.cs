using Shared.Errors;

namespace Todo.Application.Services;

/// <summary>
/// The errors of <see cref="RemotePlannerService"/>, one factory method each,
/// each carrying its kind (skill error-kinds): what the todo API's outcomes
/// mean to this application.
/// </summary>
public static class RemotePlannerErrors
{
    public static ErrorKindException Unavailable(Exception cause) =>
        new(ErrorKind.Unavailable, "the todo service is unavailable", cause);

    public static ErrorKindException TitlesRejected(string reason, Exception cause) =>
        new(ErrorKind.Invalid, $"the todo service rejected the titles: {reason}", cause);
}
