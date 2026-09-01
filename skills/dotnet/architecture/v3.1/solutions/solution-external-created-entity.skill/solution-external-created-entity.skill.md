---
name: solution-external-created-entity
description: Defines the full external-created entity stack — Guid property and unique index on entity, IHasGuid and IGuidResolver<TResponse> in Shared, ConflictResult<T> in Shared, GuidResolvingBehavior in BuildingBlocks that returns the resolver's ConflictResult<TResponse> on duplicate Guid, {Entity}ByGuidSpec in Application, GuidResolver implementation in Application
whenToUse: when an entity is created by an external, client-generated Guid and creation must be idempotent — preventing duplicate creation via GuidResolvingBehavior and IHasGuid
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - stack/dotnet
  - domain
  - application
  - infrastructure
  - guid
  - idempotency
  - framework/mediatr
  - framework/ef-core
  - pipeline
  - conflict
  - result
  - idempotent-create
  - concern/architecture
  - solution/external-created-entity

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
  - App.Host.csproj
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]]"
built_on_plateau:
adr:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/adr/use-conflict-result-for-duplicate-guid.md|Use ConflictResult<T> for duplicate Guid handling]]"
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

# Capabilities
- Idempotent creation endpoints using a client-generated `Guid`
- Automatic duplicate detection before the handler runs
- Symmetric `201`/`409` response contracts
- Database-level unique index guard against race conditions
- Clear correlation handle without leaking external identity into domain logic

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

- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/adr/use-conflict-result-for-duplicate-guid.md|Use ConflictResult<T> for duplicate Guid handling]]
  - Selected variant: return `ConflictResult<T>` from the resolver and pass it through `GuidResolvingBehavior`
  - Avoids exceptions for flow control
  - Avoids dedicated conflict middleware
  - Keeps 201 and 409 response types symmetric

# Full idempotent creation flow

See [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/doc/full-idempotent-creation-flow.mmd|full-idempotent-creation-flow.mmd]] for the Mermaid sequence diagram.

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - provides EF Core configuration pattern for unique `Guid` index
    - [[skills/dotnet/architecture/v3.1/solutions/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md|{Entity}Config.cs]] - configures unique index on `Guid` with named constant
- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - provides `IReadRepository<T>` used by Guid resolver
    - [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md|IReadRepository.cs]] - used by `Create{Entity}GuidResolver` to check Guid existence

NUGET:
- `Ardalis.Result` — required for `ConflictResult<T>` and `Result<Create{Entity}Result>` command responses.

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - extend - Add `ConflictResult<T>`, `IHasGuid`, and `IGuidResolver<TResponse>`
  - [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/ConflictResult.cs.create.md|ConflictResult.cs]] - create - Result carrying existing entity result for 409 responses
  - [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IHasGuid.cs.create.md|IHasGuid.cs]] - create - Marker interface for commands carrying a client-generated Guid
  - [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IGuidResolver.cs.create.md|IGuidResolver.cs]] - create - Per-entity resolver contract returning command response
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - extend - Add `GuidResolvingBehavior`
  - [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend/GuidResolvingBehavior.cs.create.md|GuidResolvingBehavior.cs]] - create - Pipeline behavior that short-circuits on duplicate Guid
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - extend - Add Guid property and unique index to externally-created entities
  - [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs]] - extend - Add Guid property with internal set
  - [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs]] - extend - Configure unique index on Guid with named constant
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj]] - extend - Add {Entity}ByGuidSpec and Create{Entity}GuidResolver
  - [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create.md|{Entity}ByGuidSpec.cs]] - create - Specification for looking up entity by Guid
  - [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/Create{Entity}GuidResolver.cs.create.md|Create{Entity}GuidResolver.cs]] - create - Per-entity IGuidResolver implementation returning conflict result
  - [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.extend.md|{Module}ApplicationRegistration.cs]] - extend - Register IGuidResolver in module DI
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj]] - extend - Add IHasGuid to create commands for externally-created entities
  - [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs]] - extend - Create command implements IHasGuid and returns Result<Create{Entity}Result>
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj]] - extend - Add ConflictResult<T> mapping for idempotent create endpoints
  - [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Api.csproj.extend/ConflictResultExtensions.cs.create.md|ConflictResultExtensions.cs]] - create - Maps ConflictResult<T> to HTTP 409 with existing entity result body
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Register GuidResolvingBehavior in the centralized pipeline
  - [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs]] - extend - Insert `GuidResolvingBehavior` after `ConcurrencyBehavior`

# Rules

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend.md#MUST|BuildingBlocks.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend/GuidResolvingBehavior.cs.create.md#MUST|GuidResolvingBehavior.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/Shared.csproj.extend.md#MUST|Shared.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/ConflictResult.cs.create.md#MUST|ConflictResult.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IGuidResolver.cs.create.md#MUST|IGuidResolver.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IHasGuid.cs.create.md#MUST|IHasGuid.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Api.csproj.extend.md#MUST|{Module}.Api.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Api.csproj.extend/ConflictResultExtensions.cs.create.md#MUST|ConflictResultExtensions.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend.md#MUST|{Module}.Application.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/Create{Entity}GuidResolver.cs.create.md#MUST|Create{Entity}GuidResolver.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create.md#MUST|{Entity}ByGuidSpec.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.extend.md#MUST|{Module}ApplicationRegistration.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend.md#MUST|{Module}.Domain.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md#MUST|{EntityName}.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md#MUST|{EntityName}Config.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Interfaces.csproj.extend.md#MUST|{Module}.Interfaces.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md#MUST|{Command}.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/App.Host.csproj.extend.md#MUST|App.Host.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md#MUST|PipelineRegistration.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend.md#MUST|BuildingBlocks.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend/GuidResolvingBehavior.cs.create.md#MUST|GuidResolvingBehavior.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/Shared.csproj.extend.md#MUST|Shared.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/ConflictResult.cs.create.md#MUST|ConflictResult.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IGuidResolver.cs.create.md#MUST|IGuidResolver.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IHasGuid.cs.create.md#MUST|IHasGuid.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Api.csproj.extend.md#MUST|{Module}.Api.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Api.csproj.extend/ConflictResultExtensions.cs.create.md#MUST|ConflictResultExtensions.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/App.Host.csproj.extend.md#MUST|App.Host.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md#MUST|PipelineRegistration.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend.md#MUST|{Module}.Application.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/Create{Entity}GuidResolver.cs.create.md#MUST|Create{Entity}GuidResolver.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create.md#MUST|{Entity}ByGuidSpec.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.extend.md#MUST|{Module}ApplicationRegistration.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend.md#MUST|{Module}.Domain.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md#MUST|{EntityName}.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md#MUST|{EntityName}Config.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Interfaces.csproj.extend.md#MUST|{Module}.Interfaces.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-external-created-entity.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md#MUST|{Command}.cs]]
- Never define a dedicated HTTP middleware for conflict handling

## SHOULD
- Avoid handler checks for duplicate Guid manually — duplicates pipeline logic, not reusable
- Avoid `IGuidResolver` implemented in Domain — resolver uses `IReadRepository<T>`, belongs in Application
- Avoid `IGuidResolver` registered as open generic — breaks DI resolution per command result type
- Avoid `Guid` used as foreign key in a relation — leaks external identity into domain relationships
- Avoid `Guid` route parameter after creation — internal `Id` is the only identity in routes
- Avoid `IHasGuid` or `IGuidResolver<TResponse>` defined in BuildingBlocks — they are contracts that belong in Shared per solution-sln-structure.skill
- Avoid throwing `ConflictException` from `GuidResolvingBehavior` — breaks the no-exceptions-for-flow-control principle
- Avoid `GuidResolvingBehavior` constructing response DTOs — belongs in the resolver/handler
- Avoid resolver returning a response type different from the command handler — breaks 201/409 symmetry
- Avoid `Create{Entity}Result` with fields beyond `Id` for external-created entities — violates "server returns only Id"

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
