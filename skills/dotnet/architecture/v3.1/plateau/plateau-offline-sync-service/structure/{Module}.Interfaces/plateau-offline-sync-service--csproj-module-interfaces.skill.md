---
name: plateau-offline-sync-service--csproj-module-interfaces
description: Project {Module}.Interfaces in the plateau-offline-sync-service plateau — a module's only public surface (commands, queries, notifications, DTOs, Soft Value Objects), declarations only
whenToUse: when adding or editing a command, query, notification, DTO, or Soft Value Object that other modules will consume, or deciding whether a type belongs in the public contract rather than in Application or Domain
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
  - "[[../../../../solutions/solution-soft-value-objects.skill/solution-soft-value-objects.skill.md|solution-soft-value-objects]]"
---

# Goal
- Provide the single stable public surface through which other modules interact with this module.
- Declare all write-intent contracts (commands + their result records), read-intent contracts (queries + response DTOs), notification contracts, and Soft Value Objects.
- Stay a declarations-only project with no business logic and no implementation.

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj.create]]
- [[../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]

# Core Principles
- Declarations only — `record` types, marker interfaces, DTOs. No methods, no logic.
- A command declares its result type explicitly as `ICommand<Result<{Command}Result>>` (or `ICommand<Result>` when there is no payload); the result record sits in the same file.
- A query is `IQuery<Result<{Thing}Dto>>` (or `IQuery<IReadOnlyList<{Thing}Dto>>` when it always returns data); response DTOs use `Soft{ValueObject}` / primitives, never a domain `{ValueObject}`.
- A notification is a past-tense `record ... : INotificationEvent` carrying value data only, never an entity.
- `Soft{ValueObject}` is a permissive `record` that may hold an invalid value on purpose — no validation, no throwing — so a bad DTO still reaches the collect-all validator.
- `{Module}.Interfaces` references only `Shared`.

__Applied solutions:__
- [[../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]
- [[../../../../solutions/solution-soft-value-objects.skill/solution-soft-value-objects.skill.md|solution-soft-value-objects]] - [[../../../../solutions/solution-soft-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs.create]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Interfaces
```

## Project Structure
- /{ModuleName}.Interfaces
  - /Commands
    - [{Command}.cs](./classes/plateau-offline-sync-service--class-command.skill.md) — command record + `{Command}Result` record
  - /Queries
    - [{Query}.cs](./classes/plateau-offline-sync-service--class-query.skill.md) — query record + response DTO
  - /Events
    - [{Event}.cs](./classes/plateau-offline-sync-service--class-event.skill.md) — past-tense `INotificationEvent`
  - /DTOs — response shapes shared by several queries
  - /ValueObjects
    - [Soft{ValueObject}.cs](./classes/plateau-offline-sync-service--class-soft-value-object.skill.md) — permissive value record
  - {ModuleName}.Interfaces.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Commands/{Command}.cs | Write-intent record + result record | [[./classes/plateau-offline-sync-service--class-command.skill.md\|class-command]] |
| /Queries/{Query}.cs | Read-intent record + response DTO | [[./classes/plateau-offline-sync-service--class-query.skill.md\|class-query]] |
| /Events/{Event}.cs | Domain fact as a notification | [[./classes/plateau-offline-sync-service--class-event.skill.md\|class-event]] |
| /ValueObjects/Soft{ValueObject}.cs | Permissive value record | [[./classes/plateau-offline-sync-service--class-soft-value-object.skill.md\|class-soft-value-object]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| MediatR | central | `IRequest<T>` behind the request markers |
| Ardalis.Result | central | `Result<T>` in command/query type arguments |

## What Does NOT Belong Here
- Handlers, validators — belong to [[../{Module}.Application/plateau-offline-sync-service--csproj-module-application.skill.md|{Module}.Application]].
- Business logic, entities — belong to `{Module}.Domain` (does not exist at plateau-core).
- The strict `{ValueObject}` (throws at construction) — belongs to `solution-value-objects` (VP3).

## Allowed Dependencies
- `Shared`
- NuGet: `MediatR`, `Ardalis.Result`

# Rules
MUST:
- Keep every type here a declaration — a `record`, a marker interface, or a DTO. Never a method body, never logic.
- Declare a command as `record {Command}(...) : ICommand<Result<{Command}Result>>` (or `: ICommand<Result>`), with `{Command}Result` in the same file; follow the fixed command property order (business fields, then VP6 `Guid`, then VP7 `ActionTimeStamp`, then VP5 version token) — none of the VP fields exist at plateau-core.
- Type command/query/event properties with primitives or `Soft{ValueObject}` — never a domain entity or a strict `{ValueObject}`.
- Name a notification `{Thing}{PastTenseVerb}`, place it in `/Events`, publish it via `IPublisher.Publish` only.
- Keep `Soft{ValueObject}` a plain `record` that never validates or throws; give a multi-property one a `protected` parameterless constructor.
- Never reference `{Module}.Domain`, `{Module}.Application`, FluentValidation, EF Core, or another module's non-Interfaces project.

__Applied solutions:__
- [[../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[../../../../solutions/solution-soft-value-objects.skill/solution-soft-value-objects.skill.md|solution-soft-value-objects]] - [[../../../../solutions/solution-soft-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]

# Check list
- [ ] `{Module}.Interfaces.csproj` references only `Shared` (+ `MediatR`, `Ardalis.Result`, versionless).
- [ ] `/Commands`, `/Queries`, `/Events`, `/DTOs`, `/ValueObjects` folders exist.
- [ ] Every command file has the command record and its result record; every query response DTO uses `Soft{ValueObject}`/primitives.
- [ ] No implementation code, no domain-type reference, no FluentValidation/EF Core package.
