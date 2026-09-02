---
name: plateau-offline-sync-service--csproj-module-api
description: Project {Module}.Api in the plateau-offline-sync-service plateau — the module's thin inbound-API adapter (HTTP endpoints and/or gRPC services), mapping transport requests to Commands/Queries and dispatching via ISender
whenToUse: when adding or editing an HTTP endpoint or gRPC service for a module, or checking that the API layer stays a thin adapter with no business logic
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/csproj
  - plateau/offline-sync-service
created_by:
  - "[[../../../../solutions/solution-api-project.skill/solution-api-project.skill.md|solution-api-project]]"
  - "[[../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]]"
---

# Goal
- Provide the module's inbound synchronous API surface: map a transport request to a Command/Query, dispatch via `ISender`, map the `Result` back. No business logic, no domain rules, no persistence.
- Exist only for a module that exposes an API (VP8 HTTP / VP9 gRPC); `solution-api-project` is their shared prerequisite.

__Applied solutions:__
- [[../../../../solutions/solution-api-project.skill/solution-api-project.skill.md|solution-api-project]] - [[../../../../solutions/solution-api-project.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj.create]]

# Core Principles
- Thin adapter — the class sees only `ICommand`/`IQuery`/DTOs from `{Module}.Interfaces`; it never touches `Domain` or `Application`.
- One project, many transports — HTTP endpoints in `/Http`, gRPC services in `/Grpc`, sharing the same `ISender` dispatch and reference rules.
- `App.Host` owns composition: `ApiRegistration` exposes `AddModuleApi()` / `UseModuleApi()`; each transport solution implements a `static partial void` hook (`HttpApiRegistration` / `GrpcApiRegistration`); `Program.cs` calls only the top-level pair.
- `Result` → HTTP status mapping is fixed by `solution-http-api-publication`; `409`/`412` and `ETag`/`If-Match` come from `solution-entity-concurrency-change` once a mutable entity is exposed.
- No dedicated test project (`{Module}.Api.Tests` does not exist) — the adapter has no logic of its own.

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Api
```

## Project Structure
- /{ModuleName}.Api
  - /Http/[{Entity}Endpoints.cs](./classes/plateau-offline-sync-service--class-endpoint.skill.md)
  - /Grpc/{Module}GrpcService.cs
  - {ModuleName}.Api.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Http/{Entity}Endpoints.cs | Minimal-API/controller endpoints dispatching via `ISender` | [[./classes/plateau-offline-sync-service--class-endpoint.skill.md\|class-endpoint]] |

## NuGet Packages
| Package | Purpose |
| --- | --- |
| MediatR | `ISender` |
| Microsoft.AspNetCore.App (framework ref) | endpoints |

## What Does NOT Belong Here
- Business logic, domain rules, persistence — belong to `{Module}.Application` / `{Module}.Domain`.
- The `Result` → status table — owned by `solution-http-api-publication`.

## Allowed Dependencies
- `{Module}.Interfaces`, `Shared`, `BuildingBlocks` — never `{Module}.Domain` or `{Module}.Application`.

# Rules
MUST:
- Reference only `{Module}.Interfaces`, `Shared`, `BuildingBlocks`.
- Keep every endpoint/service a thin adapter: request → Command/Query → `ISender.Send` → map `Result`.
- Wire transports only through `ApiRegistration`'s partial hooks; `Program.cs` calls only `AddModuleApi()` / `UseModuleApi()`.
- Never contain a business rule, a repository call, or a domain-type reference.

__Applied solutions:__
- [[../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]]

# Check list
- [ ] `{Module}.Api.csproj` references only `{Module}.Interfaces` + `Shared` + `BuildingBlocks`.
- [ ] Every endpoint/service dispatches via `ISender`, contains no logic.
- [ ] No `{Module}.Api.Tests` project.
- [ ] Transports wired only through `ApiRegistration` partial hooks.
