---
name: plateau-offline-sync-service--csproj-shared
description: Project Shared in the plateau-offline-sync-service plateau — cross-cutting contracts (the MediatR request markers, the LogEvents catalogue) that every layer may depend on
whenToUse: when adding or editing a cross-cutting marker, contract, or primitive in Shared, or deciding whether a new type belongs in Shared rather than BuildingBlocks or a module
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/csproj
  - plateau/offline-sync-service
created_by:
  - "[[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]"
  - "[[../../../../solutions/solution-app-logging.skill/solution-app-logging.skill.md|solution-app-logging]]"
  - "[[../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]]"
  - "[[../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]]"
  - "[[../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
  - "[[../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]]"
  - "[[../../../../solutions/solution-grpc-client.skill/solution-grpc-client.skill.md|solution-grpc-client]]"
  - "[[../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]]"
  - "[[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Give every layer one project it may reference for cross-cutting contracts and the `Result` primitive, with no coupling — `Shared` is a leaf that references nothing else in the solution.
- Make `ICommand` / `IQuery` / `INotificationEvent` available to every layer without coupling to `BuildingBlocks`.
- Give milestone and critical log lines stable `EventId`s that survive a logging-provider change.

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]
- [[../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[../../../../solutions/solution-app-logging.skill/solution-app-logging.skill.md|solution-app-logging]] - [[../../../../solutions/solution-app-logging.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

# Core Principles
- `Shared` holds interfaces, marker types, and small value primitives — no implementations, no behaviors, no entities.
- Any project at any layer may reference `Shared`; `Shared` references nothing.
- The three request markers pass straight through to MediatR's `IRequest<T>` / `INotification`; a command declares its `Result<T>` explicitly as the type argument.
- `LogEvents` is a `static class` of `EventId` constants only — no logging logic; the `ILogger<T>` call stays at the call site.
- These folders (`/Exceptions`, `/Repositories`, `/UnitOfWork`, `/Concurrency`, `/Timestamps`, `/Clients`) are the domain-service additions; `Shared` stays a leaf — every one of them holds only contracts.

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]
- [[../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

# Structure

## Solution place
```
/src/Shared
```

## Project Structure
- /Shared
  - /MediatR
    - [ICommand.cs](./classes/plateau-offline-sync-service--class-i-command.skill.md) — `ICommand : IRequest<Result>`, `ICommand<TResponse> : IRequest<TResponse>`
    - [IQuery.cs](./classes/plateau-offline-sync-service--class-i-query.skill.md) — `IQuery<TResponse> : IRequest<TResponse>`
    - [INotificationEvent.cs](./classes/plateau-offline-sync-service--class-i-notification-event.skill.md) — `INotificationEvent : INotification`
  - /Logging
    - [LogEvents.cs](./classes/plateau-offline-sync-service--class-log-events.skill.md) — stable `EventId` constants
  - /Exceptions
    - [DomainException.cs](./classes/plateau-offline-sync-service--class-domain-exception.skill.md) — the single invariant-violation exception (VP1)
  - /Repositories
    - [IReadRepository.cs / IRepository.cs](./classes/plateau-offline-sync-service--class-repository-contracts.skill.md) — data-access contracts (VP2)
  - /UnitOfWork
    - [IUnitOfWork.cs](./classes/plateau-offline-sync-service--class-i-unit-of-work.skill.md) — the commit contract (VP2)
  - /Concurrency
    - [IVersioned / IHasVersions / IEntityVersionResolver(Factory).cs](./classes/plateau-offline-sync-service--class-concurrency-contracts.skill.md) — optimistic-concurrency contracts (VP5)
  - /Timestamps
    - [ICreationInfoModel / IUpdateInfoModel / ICommandWithTimestamp.cs](./classes/plateau-offline-sync-service--class-timestamp-contracts.skill.md) — creation/update timestamp contracts (VP7)
  - /Clients
    - [I{Dependency}Client.cs](./classes/plateau-offline-sync-service--class-i-dependency-client.skill.md) — outbound service contracts returning `Result<T>` (VP11)
  - /Guid
    - [IHasGuid.cs / IGuidResolver.cs](./classes/plateau-offline-sync-service--class-guid-contracts.skill.md) — idempotent-create contracts (VP6)
  - /Results
    - [ConflictResult.cs](./classes/plateau-offline-sync-service--class-guid-contracts.skill.md) — `Result<T>` with Conflict status carrying the existing entity (VP6)
  - /Exceptions
    - [EntityNotLoadedException.cs](./classes/plateau-offline-sync-service--class-entity-not-loaded-exception.skill.md) — required-navigation-not-loaded (VP4)
  - Shared.csproj

__Applied solutions:__
- [[../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[../../../../solutions/solution-app-logging.skill/solution-app-logging.skill.md|solution-app-logging]] - [[../../../../solutions/solution-app-logging.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /MediatR/ICommand.cs | `ICommand` + `ICommand<TResponse>` request markers | [[./classes/plateau-offline-sync-service--class-i-command.skill.md\|class-i-command]] |
| /MediatR/IQuery.cs | `IQuery<TResponse>` request marker | [[./classes/plateau-offline-sync-service--class-i-query.skill.md\|class-i-query]] |
| /MediatR/INotificationEvent.cs | `INotificationEvent` notification marker | [[./classes/plateau-offline-sync-service--class-i-notification-event.skill.md\|class-i-notification-event]] |
| /Logging/LogEvents.cs | `EventId` catalogue for searched-for log lines | [[./classes/plateau-offline-sync-service--class-log-events.skill.md\|class-log-events]] |
| /Exceptions/DomainException.cs | Single invariant-violation exception | [[./classes/plateau-offline-sync-service--class-domain-exception.skill.md\|class-domain-exception]] |
| /Repositories/*.cs | `IRepository<T>` / `IReadRepository<T>` | [[./classes/plateau-offline-sync-service--class-repository-contracts.skill.md\|class-repository-contracts]] |
| /UnitOfWork/IUnitOfWork.cs | The commit contract | [[./classes/plateau-offline-sync-service--class-i-unit-of-work.skill.md\|class-i-unit-of-work]] |
| /Concurrency/*.cs | `IVersioned` / `IHasVersions` / resolver contracts | [[./classes/plateau-offline-sync-service--class-concurrency-contracts.skill.md\|class-concurrency-contracts]] |
| /Timestamps/*.cs | Creation/update timestamp contracts | [[./classes/plateau-offline-sync-service--class-timestamp-contracts.skill.md\|class-timestamp-contracts]] |
| /Clients/I{Dependency}Client.cs | Outbound service contract returning `Result<T>` | [[./classes/plateau-offline-sync-service--class-i-dependency-client.skill.md\|class-i-dependency-client]] |
| /Guid/*.cs, /Results/ConflictResult.cs | Idempotent-create contracts + conflict result | [[./classes/plateau-offline-sync-service--class-guid-contracts.skill.md\|class-guid-contracts]] |
| /Exceptions/EntityNotLoadedException.cs | Required-navigation-not-loaded exception | [[./classes/plateau-offline-sync-service--class-entity-not-loaded-exception.skill.md\|class-entity-not-loaded-exception]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| Ardalis.Result | central (`Directory.Packages.props`) | `Result` / `Result<T>` — the outcome type every handler returns |
| MediatR | central | `IRequest<T>` / `INotification` the markers extend |
| Microsoft.Extensions.Logging.Abstractions | central | `EventId` for `LogEvents` |

## What Does NOT Belong Here
- Business logic, domain rules, entities — belong to `{Module}.Domain` (does not exist at plateau-core).
- Pipeline behaviors — belong to [[../BuildingBlocks/plateau-offline-sync-service--csproj-building-blocks.skill.md|BuildingBlocks]].
- The logging provider configuration — belongs to [[../App.Host/classes/plateau-offline-sync-service--class-logging-registration.skill.md|LoggingRegistration.cs]].
- Any implementation — belongs to `BuildingBlocks` or `App.Infrastructure`.

## Allowed Dependencies
- No project references — `Shared` is a leaf.
- NuGet: `Ardalis.Result`, `MediatR`, `Microsoft.Extensions.Logging.Abstractions`, plus `Ardalis.Specification` (the repository contracts inherit its base interfaces).

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]

# Rules
MUST:
- Keep `Shared` with zero project references — it depends only on the BCL and the three NuGet packages above.
- Put only interfaces, marker types, and small value primitives in `Shared` — never an implementation, a behavior, or an entity.
- Place all three request markers in `/Shared/MediatR` with `namespace Shared.MediatR`; keep `LogEvents` in `/Shared/Logging` with `namespace Shared.Logging`.
- Keep `LogEvents` a `static class` of `EventId` constants with fixed numbers and `nameof` names — never renumber a used id.
- Never add FluentValidation, EF Core, or any implementation code to `Shared`; never reference a module, `BuildingBlocks`, or an infrastructure project.
- Never define a request marker in `BuildingBlocks` — a module would then have to reference the technical-pattern layer to declare a request.

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj.create]]
- [[../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
- [[../../../../solutions/solution-app-logging.skill/solution-app-logging.skill.md|solution-app-logging]] - [[../../../../solutions/solution-app-logging.skill/Implementation/Shared.csproj.extend/LogEvents.cs.create.md|LogEvents.cs.create]]

# Check list
- [ ] `Shared.csproj` has no project references; references only `Ardalis.Result`, `MediatR`, `Microsoft.Extensions.Logging.Abstractions` (all versionless).
- [ ] `/Shared/MediatR/{ICommand,IQuery,INotificationEvent}.cs` exist, `namespace Shared.MediatR`.
- [ ] `/Shared/Logging/LogEvents.cs` exists, only `static readonly EventId` fields.
- [ ] `/Exceptions`, `/Repositories`, `/UnitOfWork`, `/Concurrency`, `/Timestamps`, `/Clients` each hold only contracts — no implementation.
- [ ] No implementation, behavior, or entity in any `Shared` type.
