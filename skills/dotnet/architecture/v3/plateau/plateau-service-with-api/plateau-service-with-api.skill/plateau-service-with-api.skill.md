---
name: service-with-api
description: Composes the validated-module-interaction plateau with an external surface — REST Controllers/Minimal API (solution-http-api-publication) and/or gRPC services (solution-grpc-integration), both thin MediatR adapters, both optional and independently applicable. A module composing this plateau is externally reachable for the first time in this hierarchy.
whenToUse: when a module needs to be callable from outside the process — over HTTP, gRPC, or both — or when reviewing whether a Controller, Minimal API endpoint, or gRPC service follows this baseline
domain: skill
type: template
version: 20260830090000
tags:
  - skill/template/plateau
  - plateau/service-with-api
parent_plateaus:
  - "[[skills/dotnet/architecture/v3/plateau/plateau-service-with-validated-module-interaction/plateau-service-with-validated-module-interaction.skill/plateau-service-with-validated-module-interaction.skill.md|plateau-service-with-validated-module-interaction]]"
standalone: true
created_by:
  - "[[../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]]"
  - "[[../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]]"
adr:
  - "[[skills/dotnet/architecture/v3/plateau/plateau-service-with-api/adr/require-at-least-one-mediatr-source.md|Require at least one MediatR handler source (query or command integration)]]"
---

# Goal
Give a module a real external surface, over HTTP and/or gRPC, without mandating both:
- Create five REST Controller archetypes covering entity-lifecycle operations (Collection, SingleEntity, Property, SubCollection, Relationship), plus Minimal API for system/webhook/batch/cross-aggregate operations
- Create one `.proto` contract and one gRPC service per entity, method-oriented rather than route-oriented
- Map `Result<T>` to the protocol's own error shape — `ProblemDetails` for HTTP, `RpcException`/`StatusCode` for gRPC
- Wire whichever protocol(s) are applied into App.Host — controller/Swagger registration and/or gRPC service registration, neither aware of the other

# Core Principles
- Inherited from [[skills/dotnet/architecture/v3/plateau/plateau-service-with-validated-module-interaction/plateau-service-with-validated-module-interaction.skill/plateau-service-with-validated-module-interaction.skill.md|plateau-service-with-validated-module-interaction]] (and, transitively, the foundation plateau): fixed four-project module shape, centralized pipeline/module registration, global unhandled-exception handling, Value Objects at both strengths, guarded entity behavior, the `ValidationBehavior` pipeline gate, cross-module DTO/VO validators, and the full `ICommand`/handler/validator/registration chain — this plateau does not change any of that, it gives the module something reachable from outside the process for the first time.
- Optional and independent, never bundled: `solution-http-api-publication` and `solution-grpc-integration` each work completely on their own. A module composing this plateau applies at least one — HTTP-only, gRPC-only, and both are all complete, valid applications; applying both is never required just because this plateau exists. Neither solution's `Implementation` files, nor either `AddXxxApi()`/`UseXxxApi()` pair in App.Host, reference or assume the other.
- Thin adapter, regardless of protocol: map input → dispatch exactly one `ISender.Send()` → map output. Every controller action, Minimal API endpoint, and gRPC RPC method follows this shape; a conditional beyond mapping is a defect, not a feature.
- HTTP is resource-route-oriented (five controller archetypes over `/api/{plural}[/{id}[/{related}[/{relatedId}]]]`); gRPC is method-oriented (one `{Entity}GrpcService` per entity, one RPC method per operation) — neither model is forced onto the other's protocol.
- Same command/query, both adapters: when both solutions are applied, HTTP and gRPC dispatch the exact same `ICommand`/`IQuery` from `{Module}.Interfaces` — never two parallel command definitions for the same operation.
- `ResultExtensions.ToProblemDetails()` (HTTP) and `RpcExceptionExtensions.ToRpcException()` (gRPC) are each the single place their protocol's status mapping is declared — one `switch` arm per `ResultStatus` the module's handlers can return, unmatched status throws `InvalidOperationException`, never a silent fallback.
- A module needs `solution-command-integration` for write endpoints/RPCs and `solution-query-integration` for read ones — at least one, not necessarily both. `solution-query-integration` is not yet composed at this plateau's own `built_on_plateau`; a module with only commands still gets a complete, valid write-only API until persistence (`plateau-statefull-service`) is composed too, typically via `plateau-v1`. This requirement is now also structurally encoded, not just stated in prose — see [[skills/dotnet/architecture/v3/plateau/plateau-service-with-api/adr/require-at-least-one-mediatr-source.md|the ADR]].
- `App.Host` is now an ASP.NET Core web host (`WebApplication.CreateBuilder`), not the plain console host the foundation plateau's own example used — required by both `Microsoft.AspNetCore.Mvc` and `Grpc.AspNetCore`.
- Standalone, finally: `standalone: true` — this is the first plateau in the hierarchy where "does anything external have a way to interact with it" becomes true. A module composing this plateau (with at least one protocol applied) is genuinely deployable and callable, even with no persistence yet — a command with no persisted state is still a complete, valid endpoint.
- Known, disclosed gap: `{Module}.Api` has no dedicated test project, carried over from a decision made when Api was a bare placeholder. `ResultExtensions`/`RpcExceptionExtensions` are now real, pure, testable mapping functions with zero coverage anywhere in this plateau — not a silent gap, see the sln-level structure skill's own note.

# Capabilities
- rest-api (optional, via `solution-http-api-publication`)
  - Five controller archetypes covering full entity-lifecycle addressing: collection root, single entity, addressable property, sub-collection, relationship instance.
  - Minimal API for system/webhook/batch/cross-aggregate operations with no natural resource identity.
  - `ProblemDetails` for every error response, `[ProducesResponseType]` for every status a handler can return, `InvalidOperationException` on anything undocumented.
  - One Swagger document per module (`{Module}ApiSwaggerRegistration`), never one solution-wide `v1` document.
- grpc-api (optional, via `solution-grpc-integration`)
  - One `.proto` contract + one `{Entity}GrpcService` per entity, method-oriented, generated server base classes never hand-edited.
  - `RpcException`/`StatusCode` for every error response, mirroring the HTTP mapping's classification.
- composability
  - Both capabilities dispatch the same `{Module}.Interfaces` commands/queries — apply one now, add the other later, with zero changes to Application/Domain.
  - Removing either solution's App.Host wiring never breaks the other's.

# Usecases

## Publish a module over HTTP only
1. Apply `solution-http-api-publication`: add the five controller archetypes (as needed) and/or Minimal API endpoints under `{Module}.Api`, `ResultExtensions`, `{Module}ApiSwaggerRegistration`.
2. Wire `ApiRegistration.AddApi()`/`UseApi()` into `App.Host/Program.cs`.
3. Done — no gRPC folder, no `Grpc.AspNetCore` reference, nothing missing.

## Publish a module over gRPC only
1. Apply `solution-grpc-integration`: add one `.proto` + one `{Entity}GrpcService` per published entity under `{Module}.Api`, `RpcExceptionExtensions`.
2. Wire `GrpcRegistration.AddGrpcApi()`/`UseGrpcApi()` into `App.Host/Program.cs`.
3. Done — no Controllers folder, no `Microsoft.AspNetCore.Mvc`-specific wiring, nothing missing.

## Publish a module over both protocols
1. Apply both solutions independently, in either order.
2. `Program.cs` calls all four extensions: `AddApi()`, `AddGrpcApi()`, then (after `Build()`) `UseApi()`, `UseGrpcApi()`.
3. Both adapters dispatch the same commands/queries from `{Module}.Interfaces` — no operation is defined twice.

## Add a write-only endpoint before persistence exists
1. Confirm the module has `solution-command-integration` composed (inherited from the parent plateau).
2. Add the `POST`/`PUT`/`DELETE` actions (or RPC methods) the command needs — no `GET`/list/read RPC exists yet, since `solution-query-integration` isn't composed at this plateau's own level.
3. This is a complete, valid application — not a lesser one; read endpoints arrive once persistence and query-integration are composed too (typically at `plateau-v1`).

# Example
A runnable example lives in [`./example`](./example). It is built on top of the [`plateau-service-with-validated-module-interaction`](../../plateau-service-with-validated-module-interaction/plateau-service-with-validated-module-interaction.skill/plateau-service-with-validated-module-interaction.skill.md) example and extends it with:
- `App.Host` upgraded from a plain console host to an ASP.NET Core web host (`WebApplication.CreateBuilder`);
- `Sample.Api` publishing `CreateTaskCommand` over both HTTP (`TasksController`, `ResultExtensions`) and gRPC (`Task.proto` + `TaskGrpcService`, `RpcExceptionExtensions`), each independently wired via its own `Add*Api()`/`Use*Api()` pair in `App.Host`;
- a per-module Swagger document (`SampleApiSwaggerRegistration`).

See `example/README.md` for how to run it, and its "Known gap" section for why only `Create` is published (no query-integration composed yet at this plateau).
