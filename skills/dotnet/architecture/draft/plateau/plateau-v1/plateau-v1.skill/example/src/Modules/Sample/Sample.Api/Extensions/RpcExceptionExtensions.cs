using Ardalis.Result;
using Grpc.Core;

namespace Sample.Api.Extensions;

public static class RpcExceptionExtensions
{
    public static RpcException ToRpcException(this Ardalis.Result.Result result) => result.Status switch
    {
        ResultStatus.NotFound => new RpcException(new Status(StatusCode.NotFound, string.Join("; ", result.Errors))),
        ResultStatus.Invalid => new RpcException(new Status(StatusCode.InvalidArgument, string.Join("; ", result.ValidationErrors.Select(e => e.ErrorMessage)))),
        ResultStatus.Conflict => new RpcException(new Status(StatusCode.AlreadyExists, string.Join("; ", result.Errors))),
        ResultStatus.Unauthorized => new RpcException(new Status(StatusCode.Unauthenticated, "Unauthorized")),
        ResultStatus.Forbidden => new RpcException(new Status(StatusCode.PermissionDenied, "Forbidden")),
        ResultStatus.Error or ResultStatus.CriticalError => new RpcException(new Status(StatusCode.Internal, "An unexpected error occurred. Please try again later.")),
        _ => throw new InvalidOperationException($"Unhandled ResultStatus '{result.Status}' — every status this module's handlers can return must be mapped here explicitly.")
    };
}
