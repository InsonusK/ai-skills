---
uid: 9c8d7e6f-5a4b-3c2d-1e0f-9a8b7c6d5e4f
name: external-created-entity
description: Defines the full external-created entity stack — Guid property and unique index on entity, IHasGuid and IGuidResolver in Shared, ConflictException in Shared, GuidResolvingBehavior in BuildingBlocks, ConflictExceptionMiddleware in BuildingBlocks that catches ConflictException and writes 409 with existing entity body, {Entity}ByGuidSpec in Application, GuidResolver implementation in Application
domain: skill
type: architecture
version: 20260611
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - application
  - infrastructure
  - guid
  - idempotency
  - mediatr
  - pipeline
  - middleware
  - conflict
  - exception-handling
triggers:
  - external created entity
  - client-generated guid
  - idempotent create
  - async creation
  - prevent duplicate creation
  - GuidResolvingBehavior
  - IHasGuid
creates:
  - Shared.Exceptions.ConflictException.cs
  - Shared.Guid.IHasGuid.cs
  - Shared.Guid.IGuidResolver.cs
  - BuildingBlocks.MediatR.GuidResolvingBehavior.cs
  - BuildingBlocks.Middleware.ConflictExceptionMiddleware.cs
  - "{Module}.Application.Specifications.{Entity}ByGuidSpec.cs"
  - "{Module}.Application.Resolvers.Create{Entity}GuidResolver.cs"
extends:
  - Shared.csproj
  - BuildingBlocks.csproj
  - "{Module}.Domain.csproj"
  - "{Module}.Domain.Entities.{EntityName}.cs"
  - "{Module}.Domain.Configurations.{EntityName}Config.cs"
  - "{Module}.Interfaces.csproj"
  - "{Module}.Application.csproj"
  - "{Module}.Application.{Module}ApplicationRegistration.cs"
  - App.Host.csproj
  - App.Host.DependencyInjection.MiddlewareRegistration.cs
depends_on:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/command-integration.solution.skill/command-integration.solution.skill.md|command-integration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/middleware-registration.solution.skill/middleware-registration.solution.skill.md|middleware-registration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration.solution.skill]]"
---

# Goal
- Define `Guid` as an immutable correlation property on externally-created entities — set once on creation, never changed
- Define a unique database index on `Guid` as the final idempotency guard — duplicate requests that bypass the pipeline are rejected at the DB level
- Define `IHasGuid` and `IGuidResolver<TResult>` in `Shared` as the command marker and resolver contract
- Define `GuidResolvingBehavior` in `BuildingBlocks` as the pipeline behavior that short-circuits with `ConflictException<TResult>` when the `Guid` already exists
- Define `ConflictException<T>` in `Shared` — carries the existing entity result so middleware can return 409 with a body the client can use to recover without a second GET
- Define `ConflictExceptionMiddleware` in `BuildingBlocks` — catches any `ConflictException` thrown during a request and writes 409 with the existing entity body
- Define `{Entity}ByGuidSpec` in `{Module}.Application/Specifications` — the spec used by the resolver
- Define `Create{Entity}GuidResolver` in `{Module}.Application/Resolvers` — one resolver per external-created entity type

# Core Principles
- External system (frontend, partner API) generates the `Guid` — the backend never generates it for external creation flows
- `Guid` is a correlation handle only — never used in domain logic, never exposed as a foreign key, never used in routing after creation
- Internal `int Id` is the only identity used inside the domain after the entity is created
- 409 response body contains the existing entity result — client recovers without a second GET request
- `GuidResolvingBehavior` is generic — one implementation handles all entity types via `IGuidResolver<TResult>` resolved from DI
- `IGuidResolver<TResult>` is NOT registered as open generic — each external-created entity type registers its own concrete resolver
- `ConflictException<T>` is thrown by the behavior — the middleware catches it and maps to 409
- `ConflictExceptionMiddleware` is centralized in BuildingBlocks — no per-controller try/catch required
- Middleware extracts the existing entity body from `ConflictException.GetValue()` so the response shape is consistent across all endpoints
- The unique database index on `Guid` is the last line of defence — it catches duplicate Guids that bypass the pipeline (e.g. concurrent requests that both pass the pipeline check simultaneously)

# Full idempotent creation flow

See [[./doc/full-idempotent-creation-flow.mmd|full-idempotent-creation-flow.mmd]] for the Mermaid sequence diagram.

# Requirements
SOLUTION:
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/Shared.csproj.create.md|Shared.csproj]] - hosts `IHasGuid`, `IGuidResolver<TResult>`, and `ConflictException`
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj]] - hosts `GuidResolvingBehavior` and `ConflictExceptionMiddleware`
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj]] - hosts `{Entity}ByGuidSpec` and `Create{Entity}GuidResolver`
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj]] - hosts create commands implementing `IHasGuid`
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj]] - hosts middleware registration
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-configuration.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - provides EF Core configuration pattern for unique `Guid` index
    - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-configuration.solution.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md|{Entity}Config.cs]] - configures unique index on `Guid` with named constant
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-behaviour.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - defines entity factory method where `Guid` is set once
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/repository-integration.solution.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - provides `IReadRepository<T>` used by Guid resolver
    - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/repository-integration.solution.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md|IReadRepository.cs]] - used by `Create{Entity}GuidResolver` to check Guid existence
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/command-integration.solution.skill/command-integration.solution.skill.md|command-integration.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/command-integration.solution.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - provides `ICommand<T>` marker for create commands
    - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/command-integration.solution.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs]] - create commands implement both `ICommand<Result<T>>` and `IHasGuid`
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/middleware-registration.solution.skill/middleware-registration.solution.skill.md|middleware-registration.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/middleware-registration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - provides centralized `MiddlewareRegistration` where `ConflictExceptionMiddleware` is registered
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - provides centralized `PipelineRegistration` where `GuidResolvingBehavior` is registered

NUGET:
- None — relies only on patterns and packages already required by dependency solutions.

# Template Skill Mutations

PROJECT:
- [[./Implementation/Shared.csproj.extend.md|Shared.csproj]] - extend - Add `ConflictException`, `IHasGuid`, and `IGuidResolver`
  - [[./Implementation/Shared.csproj.extend/ConflictException.cs.create.md|ConflictException.cs]] - create - Exception carrying existing entity result for 409 responses
  - [[./Implementation/Shared.csproj.extend/IHasGuid.cs.create.md|IHasGuid.cs]] - create - Marker interface for commands carrying a client-generated Guid
  - [[./Implementation/Shared.csproj.extend/IGuidResolver.cs.create.md|IGuidResolver.cs]] - create - Per-entity resolver contract
- [[./Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - extend - Add `GuidResolvingBehavior` and `ConflictExceptionMiddleware`
  - [[./Implementation/BuildingBlocks.csproj.extend/GuidResolvingBehavior.cs.create.md|GuidResolvingBehavior.cs]] - create - Pipeline behavior that short-circuits on duplicate Guid
  - [[./Implementation/BuildingBlocks.csproj.extend/ConflictExceptionMiddleware.cs.create.md|ConflictExceptionMiddleware.cs]] - create - Middleware that catches ConflictException and writes 409
- [[./Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - extend - Add Guid property and unique index to externally-created entities
  - [[./Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs]] - extend - Add Guid property with internal set
  - [[./Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs]] - extend - Configure unique index on Guid with named constant
- [[./Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj]] - extend - Add {Entity}ByGuidSpec and Create{Entity}GuidResolver
  - [[./Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create.md|{Entity}ByGuidSpec.cs]] - create - Specification for looking up entity by Guid
  - [[./Implementation/{Module}.Application.csproj.extend/Create{Entity}GuidResolver.cs.create.md|Create{Entity}GuidResolver.cs]] - create - Per-entity IGuidResolver implementation
  - [[./Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.extend.md|{Module}ApplicationRegistration.cs]] - extend - Register IGuidResolver in module DI
- [[./Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj]] - extend - Add IHasGuid to create commands for externally-created entities
  - [[./Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs]] - extend - Create command implements IHasGuid
- [[./Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Extend MiddlewareRegistration with ConflictExceptionMiddleware
  - [[./Implementation/App.Host.csproj.extend/MiddlewareRegistration.cs.extend.md|MiddlewareRegistration.cs]] - extend - Register ConflictExceptionMiddleware in the centralized HTTP middleware pipeline

# Rules

MUST:
- External-created entities have `public Guid Guid { get; internal set; }`
- `Guid` set exactly once in the entity factory method — never reassigned
- Unique index on `Guid` configured with named constant `UX_Guid` in entity configuration
- `{Entity}ByGuidSpec` defined in `/{Module}.Application/Specifications`
- `IHasGuid`, `IGuidResolver<TResult>` defined in Shared
- `GuidResolvingBehavior` defined in `BuildingBlocks/MediatR/GuidResolvingBehavior.cs`
- `ConflictException<T>` defined in `Shared/Exceptions/ConflictException.cs` with non-generic `ConflictException` base class
- `ConflictExceptionMiddleware` defined in `BuildingBlocks/Middleware/ConflictExceptionMiddleware.cs`
- `ConflictExceptionMiddleware` registered in the HTTP pipeline in App.Host
- Create commands for external-created entities implement both `ICommand<Result<T>>` and `IHasGuid`
- One `Create{Entity}GuidResolver` per external-created entity type in `/{Module}.Application/Resolvers`
- Each `IGuidResolver<TResult>` registered as `Scoped` in module DI registration
- `ConflictExceptionMiddleware` writes 409 with the existing entity body extracted via `ConflictException.GetValue()`
- `Guid` is first property in create command record
- Pipeline behaviors registered via centralized `PipelineRegistration` in App.Host

MUST NOT:
- Guid used in domain logic, domain events, relationships, or routes after creation
- Guid regenerated or changed after entity creation
- Update, delete, or internal-create commands implement `IHasGuid`
- `IGuidResolver` registered as open generic — each entity type registers its own concrete resolver
- Resolver throw exceptions — null means not found, non-null means exists
- `ConflictException<T>` carry only the Id — must carry the full result so middleware can extract the entity body
- Per-controller try/catch for `ConflictException` — handling is centralized in middleware

SHOULD:
- `Guid` be the first property in the command record — signals external-created entity at a glance

# Anti-patterns
- Handler checks for duplicate Guid manually — duplicates pipeline logic, not reusable
- 409 returns `ProblemDetails` instead of existing entity — client forced to make a second GET to recover
- `IGuidResolver` implemented in Domain — resolver uses `IReadRepository<T>`, belongs in Application
- `IGuidResolver` registered as open generic — breaks DI resolution per command result type
- `Guid` used as foreign key in a relation — leaks external identity into domain relationships
- `Guid` route parameter after creation — internal `Id` is the only identity in routes
- `IHasGuid` or `IGuidResolver<TResult>` defined in BuildingBlocks — they are contracts that belong in Shared per solution-structure.solution.skill
- Per-controller `try/catch` for `ConflictException` instead of using `ConflictExceptionMiddleware`
- `ConflictExceptionMiddleware` not registered before `MapControllers()` or endpoint routing

# Check list
- [ ] `Guid Guid { get; internal set; }` on every external-created entity
- [ ] `Guid` set in entity factory method — never reassigned
- [ ] `UX_Guid` constant defined on entity configuration class
- [ ] Unique index on `Guid` configured with `HasDatabaseName(UX_Guid)` and `IsUnique()`
- [ ] `{Entity}ByGuidSpec` in `/{Module}.Application/Specifications`
- [ ] `IHasGuid` defined in `Shared/Guid/IHasGuid.cs`
- [ ] `IGuidResolver<TResult>` defined in `Shared/Guid/IGuidResolver.cs`
- [ ] `GuidResolvingBehavior` defined in `BuildingBlocks/MediatR/GuidResolvingBehavior.cs`
- [ ] `ConflictException<T>` defined in `Shared/Exceptions/ConflictException.cs` with non-generic base
- [ ] `ConflictExceptionMiddleware` defined in `BuildingBlocks/Middleware/ConflictExceptionMiddleware.cs`
- [ ] `ConflictExceptionMiddleware` registered in App.Host HTTP pipeline
- [ ] `Create{Entity}GuidResolver` in `/{Module}.Application/Resolvers`
- [ ] Resolver uses `IReadRepository<T>` and `{Entity}ByGuidSpec` — no inline LINQ
- [ ] Resolver returns null when not found, `Result.Success(...)` when found
- [ ] `IGuidResolver<Result<Create{Entity}Result>>` registered as `Scoped` in module registration
- [ ] Create command implements `ICommand<Result<T>>` and `IHasGuid`
- [ ] `Guid` is first property in create command record
- [ ] `ConflictExceptionMiddleware` writes 409 with existing entity body from `GetValue()`

# Unittest TestCases
- [ ] When create command with new Guid Then resolver returns null — handler runs — 201 Created returned
- [ ] When create command with duplicate Guid Then resolver returns existing — `ConflictException` thrown — middleware writes 409 returned
- [ ] When 409 returned Then response body contains existing entity Id — not empty, not ProblemDetails
- [ ] When two concurrent requests with same Guid both pass pipeline Then unique index raises `DbUpdateException` with `PostgresException` where `SqlState == "23505"` and `ConstraintName == {EntityName}Config.UX_Guid`
- [ ] When entity created Then `Guid` is immutable — update attempt has no effect on Guid property
- [ ] When `ConflictExceptionMiddleware` is registered Then any `ConflictException<T>` thrown from any endpoint returns 409 without controller catch
