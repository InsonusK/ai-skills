---
uid: a7816fc9-bccb-4c80-9b88-7a0fc69a5f0b
name: buildingblocks-csproj
description: Implement reusable framework-level patterns consumed by App.Host and infrastructure across all modules
domain: skill
type: template
version: 20260616
tags:
  - skill/template/csproj
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change.solution.skill]]"
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
- Reference `IReadRepository<T>`, `IHasVersions`, `IEntityVersionResolver`, and `IVersioned` from Shared for version loading in `ConcurrencyBehavior`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

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
- `ConcurrencyBehavior` lives in BuildingBlocks — consumes contracts from Shared
- `ConcurrencyBehavior` constrained on `where TRequest : IHasVersions` — only update commands are checked

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

# Structure

## Solution place
```
/src/BuildingBlocks
```


## Project Structure
```
/BuildingBlocks
  /MediatR
    ValidationBehavior.cs
```

```
/BuildingBlocks
  /MediatR
    UnitOfWorkContext.cs
    UnitOfWorkBehavior.cs
```

```
/BuildingBlocks
  /MediatR
    UnitOfWorkContext.cs
    UnitOfWorkBehavior.cs
    ValidationBehavior.cs
  /Outbox
    OutboxMessage.cs
    OutboxDispatcher.cs
  /Concurrency
    ETagEncoder.cs
    EntityVersionResolver.cs
  BuildingBlocks.csproj
```

```
/BuildingBlocks
  /MediatR
    ValidationBehavior.cs      ← validation-behavior.solution.skill
    GuidResolvingBehavior.cs
    ConcurrencyBehavior.cs     ← entity-concurrency-change.solution.skill
    UnitOfWorkContext.cs       ← unit-of-work.solution.skill
    UnitOfWorkBehavior.cs      ← unit-of-work.solution.skill
```

```
/BuildingBlocks
  /Concurrency
    ETagEncoder.cs
  /MediatR
    ConcurrencyBehavior.cs
  /Specifications
    EntityByIdSpec.cs
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

## Directory and class skills
| `Directory|file` | Description | Pattern skill |
| ---------------- | ----------- | ------------- |
| /MediatR | MediatR pipeline behaviors |  |
| ValidationBehavior.cs | Pipeline behavior that validates any `IRequest<TResponse>` | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/BuildingBlocks/classes/ValidationBehavior.class.skill.md|ValidationBehavior.class.skill]] |
| /MediatR | MediatR pipeline behaviors and context |  |
| UnitOfWorkContext.cs | Scoped depth counter preventing premature sub-command commit | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/BuildingBlocks/classes/UnitOfWorkContext.class.skill.md|UnitOfWorkContext.class.skill]] |
| UnitOfWorkBehavior.cs | Pipeline behavior that commits at depth 1 after handler completes | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/BuildingBlocks/classes/UnitOfWorkBehavior.class.skill.md|UnitOfWorkBehavior.class.skill]] |
| /MediatR | Pipeline behavior implementations and context |  |
| /Outbox | OutboxMessage model and dispatcher |  |
| /Concurrency | ETag encoder and entity version resolver |  |
| /MediatR/GuidResolvingBehavior.cs | Pipeline behavior that short-circuits on duplicate Guid | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/BuildingBlocks/classes/GuidResolvingBehavior.class.skill.md|GuidResolvingBehavior.class.skill]] |
| /Concurrency/ETagEncoder.cs | Encodes/decodes entity versions as base64 JSON ETag | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/BuildingBlocks/classes/ETagEncoder.class.skill.md|ETagEncoder.class.skill]] |
| /MediatR/ConcurrencyBehavior.cs | Pipeline behavior validating versions before handler runs | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/BuildingBlocks/classes/ConcurrencyBehavior.class.skill.md|ConcurrencyBehavior.class.skill]] |
| /Specifications/EntityByIdSpec.cs | Generic by-Id spec used by ConcurrencyBehavior at runtime | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/BuildingBlocks/classes/EntityByIdSpec.class.skill.md|EntityByIdSpec.class.skill]] |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

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
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Module-specific handlers or validators — belong to module Application
- Common interface definitions — belong to Shared

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

## Allowed Dependencies
- Shared
- Ardalis.Result
- MediatR

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

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
	- `ETagEncoder`, `EntityByIdSpec<T>`, and `ConcurrencyBehavior` defined in BuildingBlocks
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
	- Define `IHasVersions`, `IEntityVersionResolver`, or `IVersioned` in BuildingBlocks — they belong in Shared

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

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

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]

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
- [ ] `EntityByIdSpec<T>` defined in `BuildingBlocks/Specifications/EntityByIdSpec.cs`
- [ ] `ConcurrencyBehavior` defined in `BuildingBlocks/MediatR/ConcurrencyBehavior.cs`
- [ ] `IHasVersions`, `IEntityVersionResolver`, and `IVersioned` imported from Shared, not defined in BuildingBlocks

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj.create]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj.extend]]
