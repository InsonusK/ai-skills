---
name: plateau-service-with-api--class-rpc-exception-extensions
description: Class RpcExceptionExtensions in the service-with-api plateau
whenToUse: when a gRPC service needs to map a failed Result to an RpcException/StatusCode
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/class
  - plateau/service-with-api
created_by:
  - "[[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]]"
---

# Goal
- Give every gRPC service one shared, tested mapping from a failed `Result`/`Result<T>` to an `RpcException` — gRPC's equivalent of `ResultExtensions.ToProblemDetails()`

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/RpcExceptionExtensions.cs.create.md|RpcExceptionExtensions.cs.create]]

# Implementation
```csharp
//Skill: class-rpc-exception-extensions
//Plateau: service-with-api
//Version: 20260825120000

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
```

See [[../../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/RpcExceptionExtensions.cs.create.md|RpcExceptionExtensions.cs.create]] for the status-code mapping rationale.

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/RpcExceptionExtensions.cs.create.md|RpcExceptionExtensions.cs.create]]

# Rules
MUST:
- Cover every `ResultStatus` this module's handlers can return; unmatched arm throws `InvalidOperationException`
- Every `{Entity}GrpcService` failure path call this extension, never construct `RpcException` inline

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/RpcExceptionExtensions.cs.create.md|RpcExceptionExtensions.cs.create]]

# Check list
- [ ] Every `ResultStatus` returned anywhere in this module has a matching arm
- [ ] No inline `RpcException` construction anywhere else in the module

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/RpcExceptionExtensions.cs.create.md|RpcExceptionExtensions.cs.create]]
