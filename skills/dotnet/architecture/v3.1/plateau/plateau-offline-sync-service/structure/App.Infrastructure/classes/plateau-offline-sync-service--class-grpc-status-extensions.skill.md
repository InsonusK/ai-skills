---
name: plateau-offline-sync-service--class-grpc-status-extensions
description: Class GrpcStatusExtensions in the plateau-offline-sync-service plateau — the single RpcException / StatusCode -> Ardalis.Result mapping used by every outbound gRPC adapter
whenToUse: when editing the outbound gRPC status-to-Result mapping, or checking it mirrors the inbound Result-to-status table
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-grpc-client.skill/solution-grpc-client.skill.md|solution-grpc-client]]"
---

# Goal
- One `ToResult<T>(this RpcException)` used by every `{Dependency}GrpcClient`, matching `solution-grpc-integration`'s inbound `Result` → `RpcException` table so a `Result` round-trips faithfully across a gRPC hop.

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-client.skill/solution-grpc-client.skill.md|solution-grpc-client]] - [[../../../../../solutions/solution-grpc-client.skill/Implementation/App.Infrastructure.csproj.extend/GrpcStatusExtensions.cs.create.md|GrpcStatusExtensions.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `static class` in `/App.Infrastructure/Clients`, one `ToResult<T>` extension on `RpcException`.
- `NotFound` → `Result.NotFound()`; `InvalidArgument` / `FailedPrecondition` → `Result.Invalid`; `AlreadyExists` → `Result.Conflict`; `PermissionDenied` → `Result.Forbidden()`; `Unauthenticated` → `Result.Unauthorized()`; every other status → `Result.Error(detail)`.
- Every adapter routes through this — no per-adapter status handling.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-grpc-status-extensions
// Plateau: domain-service
// Version: 20260902000000
using Ardalis.Result;
using Grpc.Core;

namespace App.Infrastructure.Clients;

public static class GrpcStatusExtensions
{
    public static Result<T> ToResult<T>(this RpcException e) => e.StatusCode switch
    {
        StatusCode.NotFound           => Result.NotFound(),
        StatusCode.InvalidArgument    => Result.Invalid(new ValidationError(e.Status.Detail)),
        StatusCode.FailedPrecondition => Result.Invalid(new ValidationError(e.Status.Detail)),
        StatusCode.AlreadyExists      => Result.Conflict(e.Status.Detail),
        StatusCode.PermissionDenied   => Result.Forbidden(),
        StatusCode.Unauthenticated    => Result.Unauthorized(),
        _                             => Result.Error(e.Status.Detail),
    };
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-client.skill/solution-grpc-client.skill.md|solution-grpc-client]] - [[../../../../../solutions/solution-grpc-client.skill/Implementation/App.Infrastructure.csproj.extend/GrpcStatusExtensions.cs.create.md|GrpcStatusExtensions.cs.create]]

# Rules
MUST:
- Be the single status→`Result` mapping for every outbound gRPC adapter.
- Cover `NotFound` / `InvalidArgument` / `FailedPrecondition` / `AlreadyExists` / `PermissionDenied` / `Unauthenticated`; map every other status to `Result.Error` in the `_` arm.
- Stay in sync with `solution-grpc-integration`'s `Result` → `StatusCode` mapping.
- Never apply several plateau templates per class.

# Check list
- [ ] `ToResult<T>` covers the six named statuses + a `_` arm returning `Result.Error`.
- [ ] Mapping mirrors the inbound `Result` → `StatusCode` table.

# Unittest TestCases
- [ ] WHEN each `StatusCode` is mapped THEN it produces the paired `Result` status.
- [ ] WHEN an unlisted status is mapped THEN `Result.Error(detail)` is returned.
