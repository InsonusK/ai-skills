---
uid: 9c8d7e6f-5a4b-3c2d-1e0f-9a8b7c6d5e4f
name: external-created-entity
description: Defines the full external-created entity stack — Guid property and unique index on entity, IHasGuid and IGuidResolver<TResponse> in Shared, ConflictResult<T> in Shared, GuidResolvingBehavior in BuildingBlocks that returns the resolver's ConflictResult<TResponse> on duplicate Guid, {Entity}ByGuidSpec in Application, GuidResolver implementation in Application
domain: skill
type: architecture
version: 20260615
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
  - conflict
  - result
  - idempotent-create
triggers:
  - external created entity
  - client-generated guid
  - idempotent create
  - async creation
  - prevent duplicate creation
  - GuidResolvingBehavior
  - IHasGuid
creates:
  - Shared.Results.ConflictResult.cs
  - Shared.Guid.IHasGuid.cs
  - Shared.Guid.IGuidResolver.cs
  - BuildingBlocks.MediatR.GuidResolvingBehavior.cs
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
  - "{Module}.Api.csproj"
depends_on:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/command-integration.solution.skill/command-integration.solution.skill.md|command-integration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration.solution.skill]]"
adr:
  - "[[./adr/use-conflict-result-for-duplicate-guid.md|Use ConflictResult<T> for duplicate Guid handling]]"
---

# Goal
- Define `Guid` as an immutable correlation property on externally-created entities — set once on creation, never changed
- Define a unique database index on `Guid` as the final idempotency guard — duplicate requests that bypass the pipeline are rejected at the DB level
- Define `IHasGuid` and `IGuidResolver<TResponse>` in `Shared` as the command marker and resolver contract
- Define `ConflictResult<T>` in `Shared` — a `Result<T>` with `ResultStatus.Conflict` that carries the existing entity result
- Define `GuidResolvingBehavior` in `BuildingBlocks` as the pipeline behavior that short-circuits with the resolver's conflict result when the `Guid` already exists
- Define `{Entity}ByGuidSpec` in `{Module}.Application/Specifications` — the spec used by the resolver
- Define `Create{Entity}GuidResolver` in `{Module}.Application/Resolvers` — one resolver per external-created entity type
- The resolver returns the same response type as the command handler so 201 Created and 409 Conflict share the same API contract
- The API layer maps `ConflictResult<T>` to HTTP 409 with the existing entity result body

# Core Principles
- External system (frontend, partner API) generates the `Guid` — the backend never generates it for external creation flows
- `Guid` is a correlation handle only — never used in domain logic, never exposed as a foreign key, never used in routing after creation
- Internal `int Id` is the only identity used inside the domain after the entity is created
- 409 response body contains the existing entity result — for external-created entities this result contains only the entity Id, so the body is `{ id: ... }`
- `GuidResolvingBehavior` is generic — one implementation handles all entity types via `IGuidResolver<TResponse>` resolved from DI
- `IGuidResolver<TResponse>` is NOT registered as open generic — each external-created entity type registers its own concrete resolver
- `ConflictResult<TResponse>` is returned by the resolver and passed through by the behavior — no exceptions are thrown for duplicate Guid detection
- `ConflictResult<T>` lives in Shared so both the pipeline behavior and the API layer can reference it
- The resolver returns the same response type as the command handler — this keeps the 201 and 409 responses symmetric
- `Create{Entity}Result` for external-created entities contains only the entity Id — the resolver and handler both construct the same minimal record
- The unique database index on `Guid` is the last line of defence — it catches duplicate Guids that bypass the pipeline (e.g. concurrent requests that both pass the pipeline check simultaneously)

# Adr

- [[./adr/use-conflict-result-for-duplicate-guid.md|Use ConflictResult<T> for duplicate Guid handling]]
  - Selected variant: return `ConflictResult<T>` from the resolver and pass it through `GuidResolvingBehavior`
  - Avoids exceptions for flow control
  - Avoids dedicated conflict middleware
  - Keeps 201 and 409 response types symmetric

# Full idempotent creation flow

See [[./doc/full-idempotent-creation-flow.mmd|full-idempotent-creation-flow.mmd]] for the Mermaid sequence diagram.

# Requirements
SOLUTION:
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/solution-structure.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/Shared.csproj.create.md|Shared.csproj]] - hosts `IHasGuid`, `IGuidResolver<TResponse>`, and `ConflictResult<T>`
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj]] - hosts `GuidResolvingBehavior`
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj]] - hosts `{Entity}ByGuidSpec` and `Create{Entity}GuidResolver`
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj]] - hosts create commands implementing `IHasGuid` and returning `Result<Create{Entity}Result>`
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
    - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/command-integration.solution.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs]] - create commands implement `ICommand<Result<Create{Entity}Result>>` and `IHasGuid`
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - provides centralized `PipelineRegistration` where `GuidResolvingBehavior` is registered

NUGET:
- `Ardalis.Result` — required for `ConflictResult<T>` and `Result<Create{Entity}Result>` command responses.

# Template Skill Mutations

PROJECT:
- [[./Implementation/Shared.csproj.extend.md|Shared.csproj]] - extend - Add `ConflictResult<T>`, `IHasGuid`, and `IGuidResolver<TResponse>`
  - [[./Implementation/Shared.csproj.extend/ConflictResult.cs.create.md|ConflictResult.cs]] - create - Result carrying existing entity result for 409 responses
  - [[./Implementation/Shared.csproj.extend/IHasGuid.cs.create.md|IHasGuid.cs]] - create - Marker interface for commands carrying a client-generated Guid
  - [[./Implementation/Shared.csproj.extend/IGuidResolver.cs.create.md|IGuidResolver.cs]] - create - Per-entity resolver contract returning command response
- [[./Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - extend - Add `GuidResolvingBehavior`
  - [[./Implementation/BuildingBlocks.csproj.extend/GuidResolvingBehavior.cs.create.md|GuidResolvingBehavior.cs]] - create - Pipeline behavior that short-circuits on duplicate Guid
- [[./Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - extend - Add Guid property and unique index to externally-created entities
  - [[./Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs]] - extend - Add Guid property with internal set
  - [[./Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs]] - extend - Configure unique index on Guid with named constant
- [[./Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj]] - extend - Add {Entity}ByGuidSpec and Create{Entity}GuidResolver
  - [[./Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create.md|{Entity}ByGuidSpec.cs]] - create - Specification for looking up entity by Guid
  - [[./Implementation/{Module}.Application.csproj.extend/Create{Entity}GuidResolver.cs.create.md|Create{Entity}GuidResolver.cs]] - create - Per-entity IGuidResolver implementation returning conflict result
  - [[./Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.extend.md|{Module}ApplicationRegistration.cs]] - extend - Register IGuidResolver in module DI
- [[./Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj]] - extend - Add IHasGuid to create commands for externally-created entities
  - [[./Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs]] - extend - Create command implements IHasGuid and returns Result<Create{Entity}Result>
- [[./Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj]] - extend - Add ConflictResult<T> mapping for idempotent create endpoints
  - [[./Implementation/{Module}.Api.csproj.extend/ConflictResultExtensions.cs.create.md|ConflictResultExtensions.cs]] - create - Maps ConflictResult<T> to HTTP 409 with existing entity result body

# Rules

MUST:
- External-created entities have `public Guid Guid { get; internal set; }`
- `Guid` set exactly once in the entity factory method — never reassigned
- Unique index on `Guid` configured with named constant `UX_Guid` in entity configuration
- `{Entity}ByGuidSpec` defined in `/{Module}.Application/Specifications`
- `IHasGuid`, `IGuidResolver<TResponse>` defined in Shared
- `ConflictResult<T>` defined in `Shared/Results/ConflictResult.cs`
- `GuidResolvingBehavior` defined in `BuildingBlocks/MediatR/GuidResolvingBehavior.cs`
- `GuidResolvingBehavior` constrained to `where TRequest : IHasGuid`
- Create commands for external-created entities implement both `ICommand<Result<Create{Entity}Result>>` and `IHasGuid`
- `Guid` is first property in create command record
- One `Create{Entity}GuidResolver` per external-created entity type in `/{Module}.Application/Resolvers`
- Each `IGuidResolver<TResponse>` registered as `Scoped` in module DI registration
- `IGuidResolver<TResponse>` returns `Task<TResponse?>` — null means not found, non-null means conflict
- `IGuidResolver<TResponse>` `TResponse` matches the command handler response type exactly
- Resolver returns `ConflictResult<Create{Entity}Result>` when entity exists — same type as handler success response
- `GuidResolvingBehavior` returns the resolver's result when it returns non-null — never throws
- `Create{Entity}Result` contains only the entity Id
- 409 response body contains the existing entity result — which is `{ id: ... }` because the result contains only Id
- Pipeline behaviors registered via centralized `PipelineRegistration` in App.Host
- API layer maps `ConflictResult<Create{Entity}Result>` to HTTP 409 with the result body

MUST NOT:
- Guid used in domain logic, domain events, relationships, or routes after creation
- Guid regenerated or changed after entity creation
- Update, delete, or internal-create commands implement `IHasGuid`
- `IGuidResolver` registered as open generic — each entity type registers its own concrete resolver
- Resolver throw exceptions — null means not found, non-null means exists
- Resolver return a different response type than the command handler
- `GuidResolvingBehavior` throw exceptions for duplicate Guid detection
- `GuidResolvingBehavior` construct response DTOs
- `Create{Entity}Result` carry fields beyond the entity Id for external-created entities
- Per-controller handling for Guid conflicts — conflict is expressed as `Result<T>` and mapped by the API layer
- Define a dedicated HTTP middleware for conflict handling

SHOULD:
- `Guid` be the first property in the command record — signals external-created entity at a glance
- Return `Result<Create{Entity}Result>.Created(new Create{Entity}Result(id))` from the handler on successful creation

# Anti-patterns
- Handler checks for duplicate Guid manually — duplicates pipeline logic, not reusable
- `IGuidResolver` implemented in Domain — resolver uses `IReadRepository<T>`, belongs in Application
- `IGuidResolver` registered as open generic — breaks DI resolution per command result type
- `Guid` used as foreign key in a relation — leaks external identity into domain relationships
- `Guid` route parameter after creation — internal `Id` is the only identity in routes
- `IHasGuid` or `IGuidResolver<TResponse>` defined in BuildingBlocks — they are contracts that belong in Shared per solution-structure.solution.skill
- Throwing `ConflictException` from `GuidResolvingBehavior` — breaks the no-exceptions-for-flow-control principle
- `GuidResolvingBehavior` constructing response DTOs — belongs in the resolver/handler
- Resolver returning a response type different from the command handler — breaks 201/409 symmetry
- `Create{Entity}Result` with fields beyond `Id` for external-created entities — violates "server returns only Id"

# Check list
- [ ] `Guid Guid { get; internal set; }` on every external-created entity
- [ ] `Guid` set in entity factory method — never reassigned
- [ ] `UX_Guid` constant defined on entity configuration class
- [ ] Unique index on `Guid` configured with `HasDatabaseName(UX_Guid)` and `IsUnique()`
- [ ] `{Entity}ByGuidSpec` in `/{Module}.Application/Specifications`
- [ ] `IHasGuid` defined in `Shared/Guid/IHasGuid.cs`
- [ ] `IGuidResolver<TResponse>` defined in `Shared/Guid/IGuidResolver.cs`
- [ ] `ConflictResult<T>` defined in `Shared/Results/ConflictResult.cs`
- [ ] `GuidResolvingBehavior` defined in `BuildingBlocks/MediatR/GuidResolvingBehavior.cs`
- [ ] `Create{Entity}GuidResolver` in `/{Module}.Application/Resolvers`
- [ ] Resolver uses `IReadRepository<T>` and `{Entity}ByGuidSpec` — no inline LINQ
- [ ] Resolver returns null when not found, `ConflictResult<Create{Entity}Result>` when found
- [ ] `IGuidResolver<Result<Create{Entity}Result>>` registered as `Scoped` in module registration
- [ ] Create command implements `ICommand<Result<Create{Entity}Result>>` and `IHasGuid`
- [ ] `Guid` is first property in create command record
- [ ] `Create{Entity}Result` contains only the entity Id
- [ ] Handler returns `Result<Create{Entity}Result>.Created(...)` on success
- [ ] API layer maps `ConflictResult<Create{Entity}Result>` to 409 with the result body

# Unittest TestCases
- [ ] When create command with new Guid Then resolver returns null — handler runs — 201 Created returned with CreateEntityResult
- [ ] When create command with duplicate Guid Then resolver returns ConflictResult<CreateEntityResult> — 409 Conflict returned with existing CreateEntityResult
- [ ] When 409 returned Then response body contains only existing entity Id
- [ ] When two concurrent requests with same Guid both pass pipeline Then unique index raises `DbUpdateException` with `PostgresException` where `SqlState == "23505"` and `ConstraintName == {EntityName}Config.UX_Guid`
- [ ] When entity created Then `Guid` is immutable — update attempt has no effect on Guid property
- [ ] When `GuidResolvingBehavior` detects duplicate Then handler does not run
- [ ] When API layer maps ConflictResult<CreateEntityResult> Then response status is 409 and body contains existing Id
