---
description: RpcException / StatusCode -> Ardalis.Result mapping, the mirror of solution-grpc-integration's Result -> RpcException
project_name: "App.Infrastructure"
name: "GrpcStatusExtensions.cs"
element_kind: class
change_kind: create
tags:
  - solution/grpc-client
  - element/grpc-status-extensions-cs
---

# Goals
- One `ToResult<T>(this RpcException)` used by every `{Dependency}GrpcClient`, matching `solution-grpc-integration`'s `ToRpcException` table so a `Result` round-trips faithfully across a gRPC hop.

# Implementation changes

```csharp
// App.Infrastructure/Clients/GrpcStatusExtensions.cs
using Ardalis.Result;
using Grpc.Core;

namespace App.Infrastructure.Clients;

public static class GrpcStatusExtensions
{
    public static Result<T> ToResult<T>(this RpcException e) => e.StatusCode switch
    {
        StatusCode.NotFound            => Result.NotFound(),
        StatusCode.InvalidArgument     => Result.Invalid(new ValidationError(e.Status.Detail)),
        StatusCode.FailedPrecondition  => Result.Invalid(new ValidationError(e.Status.Detail)),
        StatusCode.AlreadyExists       => Result.Conflict(e.Status.Detail),
        StatusCode.PermissionDenied    => Result.Forbidden(),
        StatusCode.Unauthenticated     => Result.Unauthorized(),
        _                              => Result.Error(e.Status.Detail),
    };
}
```

# Rules

## MUST
- Keep this the single status→`Result` mapping for every outbound gRPC adapter.
  - Risk: per-adapter mapping drifts, and `module A → module B` stops round-tripping a `NotFound`.
  - Fix: every `{Dependency}GrpcClient` calls `e.ToResult<T>()`.
- Map any status not explicitly listed to `Result.Error` (the `_` arm).
  - Risk: an unmapped status throwing an exception defeats the "every failure is a Result" contract.
  - Fix: the default arm returns `Result.Error(e.Status.Detail)`.

# Check list
- [ ] `ToResult<T>` covers `NotFound`/`InvalidArgument`/`FailedPrecondition`/`AlreadyExists`/`PermissionDenied`/`Unauthenticated`, default `Result.Error`.
- [ ] Table matches `solution-grpc-integration`'s `Result` → `StatusCode` mapping.
