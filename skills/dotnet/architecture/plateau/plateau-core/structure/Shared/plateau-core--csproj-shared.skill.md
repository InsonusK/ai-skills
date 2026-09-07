---
name: plateau-core--csproj-shared
description: Project Shared in the plateau-core plateau — cross-cutting contracts (the MediatR request markers, the LogEvents catalogue) that every layer may depend on
whenToUse: when adding or editing a cross-cutting marker, contract, or primitive in Shared, or deciding whether a new type belongs in Shared rather than BuildingBlocks or a module
domain: skill
type: template
plateau: core
version: 20260902000000
tags:
  - skill/template/csproj
  - plateau/core
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/solutions/solution-mediator-integration.skill/solution-mediator-integration.skill|solution-mediator-integration]]"
  - "[[skills/dotnet/architecture/solutions/solution-app-logging.skill/solution-app-logging.skill|solution-app-logging]]"
---

# Goal
- Give every layer one project it may reference for cross-cutting contracts and the `Result` primitive, with no coupling — `Shared` is a leaf that references nothing else in the solution.
- Make `ICommand` / `IQuery` / `INotificationEvent` available to every layer without coupling to `BuildingBlocks`.
- Give milestone and critical log lines stable `EventId`s that survive a logging-provider change.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]]
- [[skills/dotnet/architecture/solutions/solution-mediator-integration.skill/solution-mediator-integration.skill|solution-mediator-integration]] - [[skills/dotnet/architecture/solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend|Shared.csproj]]
- [[skills/dotnet/architecture/solutions/solution-app-logging.skill/solution-app-logging.skill|solution-app-logging]] - [[skills/dotnet/architecture/solutions/solution-app-logging.skill/Implementation/Shared.csproj.extend|Shared.csproj]]

# Core Principles
- `Shared` holds interfaces, marker types, and small value primitives — no implementations, no behaviors, no entities.
- Any project at any layer may reference `Shared`; `Shared` references nothing.
- The three request markers pass straight through to MediatR's `IRequest<T>` / `INotification`; a command declares its `Result<T>` explicitly as the type argument.
- `LogEvents` is a `static class` of `EventId` constants only — no logging logic; the `ILogger<T>` call stays at the call site.
- Later features add their own folders here (`/Exceptions` from VP1, `/Repositories` + `/UnitOfWork` from VP2, `/Concurrency` from VP5, `/Timestamps` from VP7) — none exist at plateau-core.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]]
- [[skills/dotnet/architecture/solutions/solution-mediator-integration.skill/solution-mediator-integration.skill|solution-mediator-integration]] - [[skills/dotnet/architecture/solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend|Shared.csproj]]

# Structure

## Solution place
```
/src/Shared
```

## Project Structure
- /Shared
  - /MediatR
    - [ICommand.cs](skills/dotnet/architecture/plateau/plateau-core/structure/Shared/classes/plateau-core--class-i-command.skill.md) — `ICommand : IRequest<Result>`, `ICommand<TResponse> : IRequest<TResponse>`
    - [IQuery.cs](skills/dotnet/architecture/plateau/plateau-core/structure/Shared/classes/plateau-core--class-i-query.skill.md) — `IQuery<TResponse> : IRequest<TResponse>`
    - [INotificationEvent.cs](skills/dotnet/architecture/plateau/plateau-core/structure/Shared/classes/plateau-core--class-i-notification-event.skill.md) — `INotificationEvent : INotification`
  - /Logging
    - [LogEvents.cs](skills/dotnet/architecture/plateau/plateau-core/structure/Shared/classes/plateau-core--class-log-events.skill.md) — stable `EventId` constants
  - Shared.csproj

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-mediator-integration.skill/solution-mediator-integration.skill|solution-mediator-integration]] - [[skills/dotnet/architecture/solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend|Shared.csproj]]
- [[skills/dotnet/architecture/solutions/solution-app-logging.skill/solution-app-logging.skill|solution-app-logging]] - [[skills/dotnet/architecture/solutions/solution-app-logging.skill/Implementation/Shared.csproj.extend|Shared.csproj]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /MediatR/ICommand.cs | `ICommand` + `ICommand<TResponse>` request markers | [[skills/dotnet/architecture/plateau/plateau-core/structure/Shared/classes/plateau-core--class-i-command.skill\|class-i-command]] |
| /MediatR/IQuery.cs | `IQuery<TResponse>` request marker | [[skills/dotnet/architecture/plateau/plateau-core/structure/Shared/classes/plateau-core--class-i-query.skill\|class-i-query]] |
| /MediatR/INotificationEvent.cs | `INotificationEvent` notification marker | [[skills/dotnet/architecture/plateau/plateau-core/structure/Shared/classes/plateau-core--class-i-notification-event.skill\|class-i-notification-event]] |
| /Logging/LogEvents.cs | `EventId` catalogue for searched-for log lines | [[skills/dotnet/architecture/plateau/plateau-core/structure/Shared/classes/plateau-core--class-log-events.skill\|class-log-events]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| Ardalis.Result | central (`Directory.Packages.props`) | `Result` / `Result<T>` — the outcome type every handler returns |
| MediatR | central | `IRequest<T>` / `INotification` the markers extend |
| Microsoft.Extensions.Logging.Abstractions | central | `EventId` for `LogEvents` |

## What Does NOT Belong Here
- Business logic, domain rules, entities — belong to `{Module}.Domain` (does not exist at plateau-core).
- Pipeline behaviors — belong to [[skills/dotnet/architecture/plateau/plateau-core/structure/BuildingBlocks/plateau-core--csproj-building-blocks.skill|BuildingBlocks]].
- The logging provider configuration — belongs to [[skills/dotnet/architecture/plateau/plateau-core/structure/App.Host/classes/plateau-core--class-logging-registration.skill|LoggingRegistration.cs]].
- Any implementation — belongs to `BuildingBlocks` or `App.Infrastructure`.

## Allowed Dependencies
- No project references — `Shared` is a leaf.
- NuGet: `Ardalis.Result`, `MediatR`, `Microsoft.Extensions.Logging.Abstractions`.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]]

# Rules
MUST:
- Keep `Shared` with zero project references — it depends only on the BCL and the three NuGet packages above.
- Put only interfaces, marker types, and small value primitives in `Shared` — never an implementation, a behavior, or an entity.
- Place all three request markers in `/Shared/MediatR` with `namespace Shared.MediatR`; keep `LogEvents` in `/Shared/Logging` with `namespace Shared.Logging`.
- Keep `LogEvents` a `static class` of `EventId` constants with fixed numbers and `nameof` names — never renumber a used id.
- Never add FluentValidation, EF Core, or any implementation code to `Shared`; never reference a module, `BuildingBlocks`, or an infrastructure project.
- Never define a request marker in `BuildingBlocks` — a module would then have to reference the technical-pattern layer to declare a request.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]]
- [[skills/dotnet/architecture/solutions/solution-mediator-integration.skill/solution-mediator-integration.skill|solution-mediator-integration]] - [[skills/dotnet/architecture/solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend|Shared.csproj]]
- [[skills/dotnet/architecture/solutions/solution-app-logging.skill/solution-app-logging.skill|solution-app-logging]] - [[skills/dotnet/architecture/solutions/solution-app-logging.skill/Implementation/Shared.csproj.extend/LogEvents.cs.create|LogEvents.cs]]

# Check list
- [ ] `Shared.csproj` has no project references; references only `Ardalis.Result`, `MediatR`, `Microsoft.Extensions.Logging.Abstractions` (all versionless).
- [ ] `/Shared/MediatR/{ICommand,IQuery,INotificationEvent}.cs` exist, `namespace Shared.MediatR`.
- [ ] `/Shared/Logging/LogEvents.cs` exists, only `static readonly EventId` fields.
- [ ] No `/Exceptions`, `/Repositories`, `/Concurrency`, `/Timestamps` folder (those arrive with later features).
- [ ] No implementation, behavior, or entity in any `Shared` type.
