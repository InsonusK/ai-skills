---
description: Maps Ardalis.Result's ResultStatus to RpcException/StatusCode — gRPC's equivalent of ResultExtensions.ToProblemDetails
project_name: "{Module}.Api"
name: "RpcExceptionExtensions.cs"
element_kind: class
change_kind: create
tags:
  - solution/grpc-integration
  - element/rpc-exception-extensions-cs
---

# Goals
- Give every gRPC service one shared, tested mapping from a failed `Result`/`Result<T>` to an `RpcException` — never hand-rolled per RPC method
- Make an undocumented `ResultStatus` a build-visible defect, exactly like `ResultExtensions.ToProblemDetails()` does for the HTTP layer

# Core Principles
- Mirrors `solution-http-api-publication`'s `ResultExtensions.ToProblemDetails()` — same switch-per-status shape, same "unmatched status throws" discipline, different target type (`RpcException`/`StatusCode` instead of `ProblemDetails`/HTTP status)
- Only failure statuses are mapped here — the success path returns the mapped proto reply directly, same as the HTTP layer's success path

# Implementation changes

```csharp
// {Module}.Api/Extensions/RpcExceptionExtensions.cs
using Ardalis.Result;
using Grpc.Core;

namespace {Module}.Api.Extensions;

public static class RpcExceptionExtensions
{
    public static RpcException ToRpcException(this Ardalis.Result.Result result) => result.Status switch
    {
        ResultStatus.NotFound => new RpcException(new Status(
            StatusCode.NotFound, string.Join("; ", result.Errors))),
        ResultStatus.Invalid => new RpcException(new Status(
            StatusCode.InvalidArgument, string.Join("; ", result.ValidationErrors.Select(e => e.ErrorMessage)))),
        ResultStatus.Conflict => new RpcException(new Status(
            StatusCode.AlreadyExists, string.Join("; ", result.Errors))),
        ResultStatus.Unauthorized => new RpcException(new Status(
            StatusCode.Unauthenticated, "Unauthorized")),
        ResultStatus.Forbidden => new RpcException(new Status(
            StatusCode.PermissionDenied, "Forbidden")),
        ResultStatus.Error or ResultStatus.CriticalError => new RpcException(new Status(
            StatusCode.Internal, "An unexpected error occurred. Please try again later.")),
        _ => throw new InvalidOperationException(
            $"Unhandled ResultStatus '{result.Status}' — every status this module's handlers can return must be mapped here explicitly.")
    };
}
```

Usage in a gRPC service — the failure branch of every RPC method looks identical:

```csharp
if (!result.IsSuccess)
    throw result.ToRpcException();
```

The status-code choices are gRPC's closest analogue to the HTTP mapping `ResultExtensions.ToProblemDetails()` makes (`NotFound`→`NotFound`, `Invalid`→`InvalidArgument`, `Conflict`→`AlreadyExists`, `Unauthorized`→`Unauthenticated`, `Forbidden`→`PermissionDenied`, `Error`/`CriticalError`→`Internal`) — a client-side gRPC interceptor reacts to `StatusCode` the same way an HTTP client reacts to a status code family.

# Rule changes

## MUST
- Cover every `ResultStatus` this module's handlers can actually return, one arm each
- `default`/unmatched arm throw `InvalidOperationException` naming the unhandled status
- Every failure path in every `{Entity}GrpcService` method call this extension, never construct `RpcException` inline

## MUST NOT
- Include a status this module's handlers never return
- Put the original exception message or stack trace into the `Status.Detail`

# Check list
- [ ] Every `ResultStatus` value returned anywhere in this module's handlers has a matching `switch` arm
- [ ] Unmatched status throws `InvalidOperationException`
- [ ] Every `{Entity}GrpcService`'s failure branch calls `ToRpcException()`, no inline `RpcException` construction
