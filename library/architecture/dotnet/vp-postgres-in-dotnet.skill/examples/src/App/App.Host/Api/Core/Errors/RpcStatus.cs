// Source: skill error-kinds-in-dotnet

using Shared.Errors;

namespace App.Host.Api.Core.Errors;

/// <summary>
/// A google.rpc.Code; the numbers are those of Grpc.Core's StatusCode, so
/// <c>(StatusCode)code</c> converts it.
/// </summary>
public enum RpcCode
{
    Ok = 0,
    Cancelled = 1,
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
/// Answers an exception of a command by its kind, for every entry point at
/// once: the google.rpc.Code a gRPC call returns, the same code in the
/// google.rpc.Status body of an HTTP error, and the HTTP status. It is the
/// table of skill error-kinds in code; a new kind is a row here, never a
/// change of an entry point. It references no gRPC package, so an HTTP-only
/// service does not pull one in.
/// </summary>
public static class RpcStatus
{
    /// <summary>All a client learns of an error with no kind.</summary>
    public const string InternalMessage = "internal error";

    /// <summary>
    /// The code and the message a client is answered with: the kind's code
    /// with the error's message, or <see cref="RpcCode.Internal"/> with
    /// <see cref="InternalMessage"/> when the error has no kind - its own
    /// message may hold connection strings or stack details and goes only to
    /// the log.
    /// </summary>
    public static (RpcCode Code, string Message) Of(Exception error)
    {
        var kind = ErrorKindException.KindOf(error);
        if (kind is null)
        {
            return (RpcCode.Internal, InternalMessage);
        }
        // The message of the exception that carries the kind, not of a wrapper around it.
        var withKind = error;
        while (withKind is not ErrorKindException)
        {
            withKind = withKind!.InnerException;
        }
        return (CodeOf(kind.Value), withKind.Message);
    }

    /// <summary>The error-kinds table: the code each kind is answered with.</summary>
    public static RpcCode CodeOf(ErrorKind kind) => kind switch
    {
        ErrorKind.Invalid => RpcCode.InvalidArgument,
        ErrorKind.NotFound => RpcCode.NotFound,
        ErrorKind.Conflict => RpcCode.AlreadyExists,
        ErrorKind.ConcurrentChange => RpcCode.Aborted,
        ErrorKind.FailedPrecondition => RpcCode.FailedPrecondition,
        ErrorKind.Unauthenticated => RpcCode.Unauthenticated,
        ErrorKind.PermissionDenied => RpcCode.PermissionDenied,
        ErrorKind.ResourceExhausted => RpcCode.ResourceExhausted,
        ErrorKind.Unimplemented => RpcCode.Unimplemented,
        ErrorKind.Unavailable => RpcCode.Unavailable,
        _ => RpcCode.Internal,
    };

    /// <summary>The HTTP status of the code by the canonical gRPC-to-HTTP mapping (skill error-kinds).</summary>
    public static int HttpStatus(this RpcCode code) => code switch
    {
        RpcCode.Ok => 200,
        RpcCode.InvalidArgument or RpcCode.FailedPrecondition or RpcCode.OutOfRange => 400,
        RpcCode.Unauthenticated => 401,
        RpcCode.PermissionDenied => 403,
        RpcCode.NotFound => 404,
        RpcCode.AlreadyExists or RpcCode.Aborted => 409,
        RpcCode.ResourceExhausted => 429,
        RpcCode.Cancelled => 499, // Client Closed Request
        RpcCode.Unimplemented => 501,
        RpcCode.Unavailable => 503,
        RpcCode.DeadlineExceeded => 504,
        _ => 500, // Internal, Unknown, DataLoss
    };
}
