---
name: plateau-domain-service--csproj-app-host
description: Project App.Host in the plateau-domain-service plateau — the single composition root wiring logging, modules, and the MediatR pipeline; contains only wiring
whenToUse: when editing the composition root — adding a module, changing pipeline behavior order, configuring logging — or deciding whether a registration belongs here rather than in a module
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/csproj
  - plateau/domain-service
created_by:
  - "[[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]]"
  - "[[../../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]]"
  - "[[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]]"
  - "[[../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]"
  - "[[../../../../solutions/solution-app-logging.skill/solution-app-logging.skill.md|solution-app-logging]]"
---

# Goal
- Be the single composition root — the only project that references every module's `Application` at once.
- Wire the service through three extension calls in `Program.cs`: `AddAppLogging()`, `AddModules()`, `AddPipeline()`.
- Contain only wiring — no business logic.

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj.create]]

# Core Principles
- `App.Host` references `BuildingBlocks` directly and every `{Module}.Application`; `Shared` comes transitively.
- `Program.cs` uses `Host.CreateApplicationBuilder` and calls only the three composition extensions — nothing else.
- `ModuleRegistration.AddModules()` is the one place a module's `Register{ModuleName}Module()` is called; `PipelineRegistration.AddPipeline()` is the one place a pipeline behavior is registered and the authoritative record of behavior order.
- Behavior order at plateau-core: `ExceptionHandlingBehavior` first (wraps everything), then `ValidationBehavior`. Later features insert `UnitOfWorkBehavior` (last), `ConcurrencyBehavior`, `GuidResolvingBehavior` between them.
- `LoggingRegistration.AddAppLogging()` is the one place the logging provider and levels are configured; levels bind from `appsettings.json`'s `Logging` section.

__Applied solutions:__
- [[../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md|PipelineRegistration.cs.create]]
- [[../../../../solutions/solution-app-logging.skill/solution-app-logging.skill.md|solution-app-logging]] - [[../../../../solutions/solution-app-logging.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

# Structure

## Solution place
```
/src/App/App.Host
```

## Project Structure
- /App.Host
  - /DependencyInjection
    - [ModuleRegistration.cs](./classes/plateau-domain-service--class-module-registration.skill.md) — `AddModules()`
    - [PipelineRegistration.cs](./classes/plateau-domain-service--class-pipeline-registration.skill.md) — `AddPipeline()`, behavior order
    - [LoggingRegistration.cs](./classes/plateau-domain-service--class-logging-registration.skill.md) — `AddAppLogging()`
  - Program.cs — `Host.CreateApplicationBuilder`, the three calls
  - appsettings.json / appsettings.Development.json — `Logging` section
  - App.Host.csproj

`InfrastructureRegistration.cs` (`AddInfrastructure()`) arrives with VP2.

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /DependencyInjection/ModuleRegistration.cs | `AddModules()` — every module's registration | [[./classes/plateau-domain-service--class-module-registration.skill.md\|class-module-registration]] |
| /DependencyInjection/PipelineRegistration.cs | `AddPipeline()` — pipeline behavior order | [[./classes/plateau-domain-service--class-pipeline-registration.skill.md\|class-pipeline-registration]] |
| /DependencyInjection/LoggingRegistration.cs | `AddAppLogging()` — provider + levels | [[./classes/plateau-domain-service--class-logging-registration.skill.md\|class-logging-registration]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| Microsoft.Extensions.Hosting | central | `Host.CreateApplicationBuilder` |
| Microsoft.Extensions.Logging.Console | central | console provider |
| MediatR | central | `IPipelineBehavior<,>` registration |

## What Does NOT Belong Here
- Business logic, handlers, validators — belong to `{Module}.Application`.
- Module-internal registration — belongs to each module's `Register{ModuleName}Module()`.
- Log call sites — in the class that logs; the `LogEvents` catalogue is in `Shared`.

## Allowed Dependencies
- `BuildingBlocks`, every `{Module}.Application`
- NuGet: `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.Logging.Console`, `MediatR`

# Rules
MUST:
- Keep `Program.cs` to `Host.CreateApplicationBuilder` plus the three composition calls; add nothing else there.
- Call `AddModules()`, `AddPipeline()`, `AddAppLogging()` exactly once each; register every module inside `ModuleRegistration.AddModules`, never from `Program.cs`.
- Register every pipeline behavior inside `PipelineRegistration.AddPipeline()` as an open generic (`services.AddTransient(typeof(IPipelineBehavior<,>), typeof(XBehavior<,>))`); keep `ExceptionHandlingBehavior` first and `ValidationBehavior` immediately after it.
- Configure logging only in `LoggingRegistration` — `ClearProviders()`, then bind `configuration.GetSection("Logging")`, then add the console provider.
- Never put a pipeline behavior registration, a module registration, or a business decision anywhere but its designated extension method.

__Applied solutions:__
- [[../../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] - [[../../../../solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]] - [[../../../../solutions/solution-validation-behavior.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]
- [[../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../solutions/solution-mediator-exception-handler.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]
- [[../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../solutions/solution-mediator-integration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]
- [[../../../../solutions/solution-app-logging.skill/solution-app-logging.skill.md|solution-app-logging]] - [[../../../../solutions/solution-app-logging.skill/Implementation/App.Host.csproj.extend/LoggingRegistration.cs.create.md|LoggingRegistration.cs.create]]

# Check list
- [ ] `Program.cs` calls `AddAppLogging()`, `AddModules()`, `AddPipeline()` once each and nothing else.
- [ ] `AddPipeline()` registers `ExceptionHandlingBehavior` then `ValidationBehavior`, both as open generics, in that order.
- [ ] Every module is registered inside `ModuleRegistration.AddModules`.
- [ ] `LoggingRegistration` clears providers, binds the `Logging` config section, adds the console provider; no other file names a provider.
- [ ] No `InfrastructureRegistration.cs` (arrives with VP2).
