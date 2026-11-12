// Source: skill vp-http-client-in-dotnet

namespace Client.Abstractions;

/// <summary>
/// The code of a status an API answered: google.rpc.Code, which is also the
/// gRPC status code, so a service maps the same codes whichever transport
/// its adapter uses.
/// </summary>
public enum ApiStatusCode
{
    Ok = 0,
    Cancelled = 1,
    /// <summary>Also an HTTP error answer whose body is no google.rpc.Status.</summary>
    Unknown = 2,
    InvalidArgument = 3,
    DeadlineExceeded = 4,
    NotFound = 5,
    AlreadyExists = 6,
    PermissionDenied = 7,
    ResourceExhausted = 8,
    FailedPrecondition = 9,
    Aborted = 10,
    OutOfRange = 11,
    Unimplemented = 12,
    Internal = 13,
    Unavailable = 14,
    DataLoss = 15,
    Unauthenticated = 16,
}

/// <summary>
/// The technical outcome of a call the API did not answer: no connection, no
/// answer within the timeout, an HTTP 502, 503, or 504, a gRPC UNAVAILABLE or
/// DEADLINE_EXCEEDED. The service over the adapter turns it into the kind
/// unavailable (skill api-client); a retry may help.
/// </summary>
public sealed class ApiUnavailableException(string api, string message, Exception? innerException = null)
    : Exception($"{api} unavailable: {message}", innerException)
{
    /// <summary>The API, as the log names it ("todo-api").</summary>
    public string Api { get; } = api;
}

/// <summary>
/// The technical outcome of any other failed call: the status the API
/// answered. The service over the adapter turns the codes it expects into
/// errors of its own; any other is a failure of the service.
/// </summary>
public sealed class ApiStatusException(string api, ApiStatusCode code, string statusMessage, int? httpStatus = null)
    : Exception($"{api} answered {(httpStatus is { } status ? $"{status}, " : "")}{code}: {statusMessage}")
{
    /// <summary>The API, as the log names it ("todo-api").</summary>
    public string Api { get; } = api;

    /// <summary>The code of the status; <see cref="ApiStatusCode.Unknown"/> for an HTTP answer with no google.rpc.Status.</summary>
    public ApiStatusCode Code { get; } = code;

    /// <summary>The message of the status, as the API wrote it.</summary>
    public string StatusMessage { get; } = statusMessage;

    /// <summary>The HTTP status of the answer; null over gRPC.</summary>
    public int? HttpStatus { get; } = httpStatus;
}
