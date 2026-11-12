using Shared.Errors;

namespace Middleware.Access;

/// <summary>The errors of the access variation point, each with its kind.</summary>
public static class AccessErrors
{
    public static ErrorKindException NoCaller(string command) =>
        new(ErrorKind.Unauthenticated, $"{command} needs a caller");

    public static ErrorKindException UnknownCaller() =>
        new(ErrorKind.Unauthenticated, "the bearer token names no known caller");

    public static ErrorKindException MissingRole(string command, string role) =>
        new(ErrorKind.PermissionDenied, $"{command} needs the role {role}");

    public static ErrorKindException BatchTooLarge(string command, int limit, int size) =>
        new(ErrorKind.ResourceExhausted, $"{command} takes at most {limit} items at once, got {size}");
}
