---
name: csproj-building-blocks
description: Implement reusable framework-level patterns consumed by App.Host and infrastructure across all modules
domain: skill
type: template
version: 20260622
plateau: default
tags:
  - skill/template/csproj
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change.skill]]"
---

# Goal
- Own the generic `ValidationBehavior` pipeline behavior that intercepts any `IRequest<TResponse>`
- Keep input validation as a cross-cutting technical concern implemented once and reused across modules
- Own `UnitOfWorkContext` and `UnitOfWorkBehavior` — the depth tracking and pipeline commit enforcement
- Reference `ICommand` and `IUnitOfWork` from Shared
- Implement application technical patterns used by App.Host and infrastructure across all modules
- Provide pipeline behaviors, repository implementations, and cross-cutting utilities
- Own `GuidResolvingBehavior` — the MediatR pipeline behavior that consumes `IHasGuid` and `IGuidResolver<TResponse>` from Shared
- Own `ETagEncoder` and `ConcurrencyBehavior` — the concrete client-facing concurrency helpers and pipeline enforcement
- Reference `IHasVersions` and `IEntityVersionResolverFactory` from Shared for version checking in `ConcurrencyBehavior`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill.md|class-validation-behavior]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create|BuildingBlocks.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

# Core Principals
- BuildingBlocks implements technical patterns using interfaces defined in Shared or provided by MediatR
- `ValidationBehavior` is generic — one implementation handles all commands and queries across all modules
- BuildingBlocks does not define request markers — it consumes `IRequest<T>` from MediatR
- `UnitOfWorkContext` is a plain class with a counter — no infrastructure dependency
- `UnitOfWorkBehavior` depends on `IUnitOfWork` and `UnitOfWorkContext` — both resolved from DI
- BuildingBlocks implements reusable technical patterns, not business logic
- BuildingBlocks depends only on Shared
- BuildingBlocks does NOT define common interfaces — it consumes interfaces from Shared
- All pipeline behavior implementations live here — registered once in App.Host, used by all modules
- `ETagEncoder` lives in BuildingBlocks — referenced by Api layers
- `ConcurrencyBehavior` lives in BuildingBlocks — consumes `IEntityVersionResolverFactory` from Shared
- `ConcurrencyBehavior` constrained on `where TRequest : IHasVersions` — only update commands are checked
- No per-entity loading logic in BuildingBlocks — version loading is delegated to `IEntityVersionResolver` implementations in module Application projects

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill.md|class-validation-behavior]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create|BuildingBlocks.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

# Structure

## Solution place
```
/src/BuildingBlocks
```


## Project Structure
- /BuildingBlocks
  - /MediatR
    - [ValidationBehavior.cs](skills/dotnet/architecture/plateau/default/structure/BuildingBlocks/classes/class-validation-behavior.skill.md)
    - [GuidResolvingBehavior.cs](skills/dotnet/architecture/plateau/default/structure/BuildingBlocks/classes/class-guid-resolving-behavior.skill.md)
    - [ConcurrencyBehavior.cs](skills/dotnet/architecture/plateau/default/structure/BuildingBlocks/classes/class-concurrency-behavior.skill.md)
    - [UnitOfWorkContext.cs](skills/dotnet/architecture/plateau/default/structure/BuildingBlocks/classes/class-unit-of-work-context.skill.md)
    - [UnitOfWorkBehavior.cs](skills/dotnet/architecture/plateau/default/structure/BuildingBlocks/classes/class-unit-of-work-behavior.skill.md)
  - /Outbox
    - OutboxMessage.cs
    - OutboxDispatcher.cs
  - /Concurrency
    - [ETagEncoder.cs](skills/dotnet/architecture/plateau/default/structure/BuildingBlocks/classes/class-etag-encoder.skill.md)
  - BuildingBlocks.csproj

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill.md|class-validation-behavior]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create|BuildingBlocks.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

## Directory and class skills
| `Directory|file` | Description | Pattern skill |
| ---------------- | ----------- | ------------- |
| /MediatR | MediatR pipeline behaviors |  |
| ValidationBehavior.cs | Pipeline behavior that validates any `IRequest<TResponse>` | [[skills/dotnet/architecture/plateau/default/structure/BuildingBlocks/classes/class-validation-behavior.skill.md|class-ValidationBehavior.skill]] |
| /MediatR | MediatR pipeline behaviors and context |  |
| UnitOfWorkContext.cs | Scoped depth counter preventing premature sub-command commit | [[skills/dotnet/architecture/plateau/default/structure/BuildingBlocks/classes/class-unit-of-work-context.skill.md|class-UnitOfWorkContext.skill]] |
| UnitOfWorkBehavior.cs | Pipeline behavior that commits at depth 1 after handler completes | [[skills/dotnet/architecture/plateau/default/structure/BuildingBlocks/classes/class-unit-of-work-behavior.skill.md|class-UnitOfWorkBehavior.skill]] |
| /MediatR | Pipeline behavior implementations and context |  |
| /Outbox | OutboxMessage model and dispatcher |  |
| /Concurrency | ETag encoder |  |
| /MediatR/GuidResolvingBehavior.cs | Pipeline behavior that short-circuits on duplicate Guid | [[skills/dotnet/architecture/plateau/default/structure/BuildingBlocks/classes/class-guid-resolving-behavior.skill.md|class-GuidResolvingBehavior.skill]] |
| /Concurrency/ETagEncoder.cs | Encodes/decodes entity versions as base64 JSON ETag | [[skills/dotnet/architecture/plateau/default/structure/BuildingBlocks/classes/class-etag-encoder.skill.md|class-ETagEncoder.skill]] |
| /MediatR/ConcurrencyBehavior.cs | Pipeline behavior validating versions before handler runs | [[skills/dotnet/architecture/plateau/default/structure/BuildingBlocks/classes/class-concurrency-behavior.skill.md|class-ConcurrencyBehavior.skill]] |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill.md|class-validation-behavior]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create|BuildingBlocks.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `FluentValidation` | latest stable | Provides `IValidator<T>` injected into `ValidationBehavior` |
| `MediatR` | latest stable | Provides `IPipelineBehavior<TRequest, TResponse>` and `IRequest<T>` |
| `Ardalis.Result` | latest stable | Provides `Result.Invalid`, `ValidationError`, and `IResult` |
| `MediatR` | latest stable | Provides `IPipelineBehavior<TRequest, TResponse>` implemented by `UnitOfWorkBehavior` |
| `System.Text.Json` | latest stable | `JsonSerializer` used in `ETagEncoder` |
| `MediatR` | latest stable | `IPipelineBehavior<TRequest, TResponse>` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill.md|class-validation-behavior]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create|BuildingBlocks.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Module-specific handlers or validators — belong to module Application
- Common interface definitions — belong to Shared

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill.md|class-validation-behavior]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create|BuildingBlocks.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

## Allowed Dependencies
- Shared
- Ardalis.Result
- MediatR

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill.md|class-validation-behavior]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create|BuildingBlocks.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

# Rules
MUST:
	- `FluentValidation`, `MediatR`, and `Ardalis.Result` packages referenced in `BuildingBlocks.csproj`
	- `ValidationBehavior` placed in `/BuildingBlocks/MediatR`
	- `ValidationBehavior` constrained to `IRequest<TResponse>` from MediatR
	- `UnitOfWorkContext` and `UnitOfWorkBehavior` defined in BuildingBlocks
	- Both reference `ICommand` and `IUnitOfWork` from Shared
	- `UnitOfWorkBehavior` constrained on `where TRequest : ICommand`
	- All pipeline behavior implementations defined here
	- BuildingBlocks depends only on Shared
	- `GuidResolvingBehavior` defined in `BuildingBlocks/MediatR/GuidResolvingBehavior.cs`
	- `GuidResolvingBehavior` consumes `IHasGuid` and `IGuidResolver<TResponse>` from Shared
	- `GuidResolvingBehavior` returns the resolver's conflict result — never throws an exception
	- `ETagEncoder` and `ConcurrencyBehavior` defined in BuildingBlocks
		- `ConcurrencyBehavior` uses `IEntityVersionResolverFactory` from Shared
	- `ConcurrencyBehavior` constrained on `where TRequest : IHasVersions`
	- `ETagEncoder` available to Api layers via BuildingBlocks reference
MUST NOT:
	- Add business logic or request-specific conditions to `ValidationBehavior`
	- Throw exceptions for validation failures — always return `Result.Invalid`
	- Add EF Core dependency to BuildingBlocks
	- BuildingBlocks reference any module project
	- BuildingBlocks reference App.Infrastructure or App.Queries
	- BuildingBlocks contain business logic
	- BuildingBlocks define common interfaces — only Shared defines interfaces
	- `IHasGuid` or `IGuidResolver<TResponse>` defined in BuildingBlocks — they are contracts that belong in Shared
	- `GuidResolvingBehavior` registered as open generic — DI resolves `IGuidResolver<TResponse>` per concrete command result type
	- Define HTTP middleware for conflict handling — conflicts are expressed as `Result<T>` and mapped by the API layer
	- Define `IHasVersions`, `IEntityVersionResolver`, `IEntityVersionResolverFactory`, or `IVersioned` in BuildingBlocks — they belong in Shared
		- Add a generic `EntityByIdSpec<T>` to BuildingBlocks — per-entity specs belong in module Application projects

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill.md|class-validation-behavior]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create|BuildingBlocks.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

# Anti-patterns
- Implementing per-request validation logic inside `ValidationBehavior`
- Returning exceptions instead of `Result.Invalid`
- `UnitOfWorkBehavior` constrained on `IRequest<T>` instead of `ICommand` — would commit on queries
- Placing domain entities in BuildingBlocks — they belong in module Domain
- Adding module-specific handlers or validators in BuildingBlocks
- Defining common interfaces in BuildingBlocks — they belong in Shared
- `GuidResolvingBehavior` registered as open generic — breaks DI resolution per command result type
- Defining `IHasGuid` or `IGuidResolver<TResponse>` in BuildingBlocks — violates the rule that BuildingBlocks consumes interfaces from Shared
- Throwing exceptions from `GuidResolvingBehavior` — breaks the command-integration principle of no exceptions for flow control
- `ConcurrencyBehavior` constrained on `IRequest<T>` instead of `IHasVersions` — would check all commands including queries
- Defining common concurrency contracts in BuildingBlocks — forces modules to reference BuildingBlocks for contracts
- Duplicating `ByIdSpec` logic in BuildingBlocks instead of reusing module Application specs

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill.md|class-validation-behavior]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create|BuildingBlocks.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

# Check list
- [ ] `FluentValidation` referenced in `BuildingBlocks.csproj`
- [ ] `MediatR` referenced in `BuildingBlocks.csproj`
- [ ] `Ardalis.Result` referenced in `BuildingBlocks.csproj`
- [ ] `/BuildingBlocks/MediatR/ValidationBehavior.cs` exists
- [ ] `UnitOfWorkContext` defined in `BuildingBlocks/MediatR/UnitOfWorkContext.cs`
- [ ] `UnitOfWorkBehavior` defined in `BuildingBlocks/MediatR/UnitOfWorkBehavior.cs`
- [ ] `UnitOfWorkBehavior` constrained to `where TRequest : ICommand`
- [ ] No EF Core reference in BuildingBlocks
- [ ] BuildingBlocks.csproj references only Shared
- [ ] BuildingBlocks.csproj contains only pattern implementations
- [ ] No common interface definitions in BuildingBlocks
- [ ] `GuidResolvingBehavior` defined in `BuildingBlocks/MediatR/GuidResolvingBehavior.cs`
- [ ] `GuidResolvingBehavior` consumes `IHasGuid` and `IGuidResolver<TResponse>` from Shared
- [ ] `GuidResolvingBehavior` returns the resolver's conflict result on duplicate Guid
- [ ] No `ConflictExceptionMiddleware` defined in BuildingBlocks
- [ ] `ETagEncoder` defined in `BuildingBlocks/Concurrency/ETagEncoder.cs`
- [ ] `ConcurrencyBehavior` defined in `BuildingBlocks/MediatR/ConcurrencyBehavior.cs`
- [ ] No EF Core reference in BuildingBlocks
- [ ] `IHasVersions`, `IEntityVersionResolver`, `IEntityVersionResolverFactory`, and `IVersioned` imported from Shared, not defined in BuildingBlocks

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill.md|class-validation-behavior]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create|BuildingBlocks.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
