---
name: statefull-service
description: Composes the validated-module-interaction plateau with real persistence — App.Infrastructure (AppDbContext, generic Repository<T>, UnitOfWork), App.Queries for cross-module reads, optimistic concurrency, idempotent external-created-entity handling, entity classification, and creation/update timestamps. A command can now load, mutate, and durably commit a real entity — still with no HTTP API surface.
whenToUse: when a module needs to actually persist and read back entities — or when reviewing whether AppDbContext, Repository<T>, UnitOfWork, concurrency control, idempotent creation, or timestamp assignment follow this baseline
domain: skill
type: template
version: 20260824100000
tags:
  - skill/template/plateau
  - plateau/statefull-service
parent_plateaus:
  - "[[skills/dotnet/architecture/draft/plateau/plateau-service-with-validated-module-interaction/plateau-service-with-validated-module-interaction.skill/plateau-service-with-validated-module-interaction.skill.md|plateau-service-with-validated-module-interaction]]"
standalone: false
created_by:
  - "[[../../../solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]]"
  - "[[../../../solutions/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]]"
  - "[[../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]]"
  - "[[../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
  - "[[../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]]"
  - "[[../../../solutions/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]]"
  - "[[../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]]"
  - "[[../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]]"
---

# Goal
Give a module real, durable persistence and reads, on top of everything the validated-interaction plateau already gives it (Value Objects, entity behavior, validation pipeline, full command chain):
- Create `App.Infrastructure` — the single home for outbound infrastructure integrations, starting with EF Core persistence
- Give every entity a single `DbContext` (`AppDbContext`), a generic `Repository<T>`, and atomic commits via `IUnitOfWork` — commit happens exactly once per top-level command, even across nested sub-command dispatch
- Classify every entity along two independent axes — ownership (internal/external) and mutability (immutable/mutable) — and apply exactly the infrastructure each classification requires, never more
- Give mutable entities optimistic concurrency control, and externally-created entities idempotent creation, both enforced in the pipeline before the handler runs
- Give user-initiated entities creation/update timestamps, split between the user-supplied action time and the server-authoritative commit time
- Create `App.Queries` — the only project with access to every module's entity types simultaneously, for reads that must JOIN across modules

# Core Principles
- Inherited from [[skills/dotnet/architecture/draft/plateau/plateau-service-with-validated-module-interaction/plateau-service-with-validated-module-interaction.skill/plateau-service-with-validated-module-interaction.skill.md|plateau-service-with-validated-module-interaction]] (and, transitively, the foundation plateau): Value Objects at both strengths, guarded entity behavior, the `ValidationBehavior` pipeline gate, cross-module DTO/VO validators, and the full `ICommand`/handler/validator/registration chain — this plateau does not change any of that, it gives it something real to persist.
- Infrastructure ownership: `App.Infrastructure` is created once, empty, by [[../../../solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]] — every concern (persistence today, cache/other integrations later) extends that same project rather than each owning its own. `AddInfrastructure()` in App.Host is the single, centralized registration entry point, parallel to `AddModules()`/`AddPipeline()`.
- Persistence: `AppDbContext` is the service's only `DbContext`, applying every module's `IEntityTypeConfiguration<T>` via `ApplyConfigurationsFromAssembly`; `Repository<T>` is the single generic implementation for all entity types, staging changes but never committing; `IUnitOfWork` is the only component that calls `SaveChangesAsync`, and only once per top-level command (`UnitOfWorkContext` tracks nesting depth). See [[../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] and [[../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]].
- Classification decides infrastructure: every entity is Internal/External × Immutable/Mutable. Internal Immutable gets neither `Version` nor `Guid`; External Immutable gets `Guid` + creation timestamp only; Internal Mutable gets `Version` + creation/update timestamps; External Mutable gets both. Applying a classification's infrastructure partially, or to a type that doesn't need it, is a documented anti-pattern. See [[../../../solutions/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]].
- Concurrency: `ConcurrencyBehavior` validates every `IHasVersions` command's expected versions against the current stored versions before the handler runs, via `IEntityVersionResolverFactory` mapping a stable business entity name to a per-entity `IEntityVersionResolver` — transport-agnostic; the ETag/`If-Match` HTTP encoding (`ETagEncoder`) is created here but has no caller until an HTTP API layer exists. See [[../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]].
- Idempotent creation: `GuidResolvingBehavior` detects a duplicate client-generated `Guid` before the handler runs, via a per-entity `IGuidResolver<TResponse>` returning the same response shape the handler itself would return; a unique database index on `Guid` is the last line of defence against races that bypass the pipeline. See [[../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]].
- Timestamps: the user may only supply `ActionTimeStamp` (validated in the command validator); the handler assigns `UserCreatedDateTime`/`UserUpdatedDateTime`; only `AppDbContext.OnBeforeSaving` assigns `ServerCreatedDateTime`/`ServerUpdatedDateTime` — the two are never assigned from the same code path. See [[../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]].
- Reads: a single-module read uses `IReadRepository<T>` + a named spec, in `{Module}.Application/Queries`; a cross-module read uses `AppDbContext` directly with `AsNoTracking()`, in `App.Queries` — the only project allowed to reference every module's entity types at once. See [[../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]].
- Pipeline order is now: `ExceptionHandlingBehavior` → `ValidationBehavior` → `ConcurrencyBehavior` → `GuidResolvingBehavior` → `UnitOfWorkBehavior`. Each behavior assumes everything before it already passed.
- Not standalone: `standalone: false` — still no HTTP API surface (see `plateau-service-with-api`), so nothing external can reach a command except a test or another module's mediator call. Persistence alone does not make a service deployable.

# Capabilities
- persistence
  - One `AppDbContext`, one generic `Repository<T>`, atomic single-commit-per-top-level-command via `IUnitOfWork`.
  - Cross-module foreign-key configurations live in `App.Infrastructure/Persistence/Configurations`; intra-module entity mapping lives in each entity's own `{Entity}Config`.
- concurrency
  - Every `IHasVersions` command is version-checked before its handler runs, returning `Result.Conflict` on any stale write — regardless of transport.
- idempotent-creation
  - Every `IHasGuid` create command is duplicate-checked before its handler runs, returning the same response shape on both the original success and the detected duplicate.
- classification
  - A documented, reviewable decision per entity (ownership × mutability) drives exactly which of the above two capabilities — and which timestamp fields — that entity gets, with anti-patterns for over- or under-applying them.
- timestamps
  - Every user-initiated entity exposes both when a user acted and when the server actually committed it, never conflated.
- reads
  - Single-module reads via `IReadRepository<T>` + specs; cross-module JOIN reads via `App.Queries` + `AppDbContext` directly, both returning DTOs only, never domain entities.
- testing
  - Every new class from this plateau is provable the same way the foundation plateau already requires — a `.feature` scenario plus step definitions in the matching `*.Tests` project. `App.Infrastructure`/`App.Queries` themselves have no dedicated test project (thin adapter layers, like `{Module}.Api`), but the module-level classes that use them (`{Entity}VersionResolver`, `Create{Entity}GuidResolver`, query handlers) are tested in `{Module}.Application.Tests`.

# Usecases

## Classify a new entity and apply its infrastructure
1. Decide ownership (who generates the identity: backend `int Id` = Internal, client-supplied `Guid` = External) and mutability (created once = Immutable, can change = Mutable).
2. Implement exactly the interfaces that classification requires on the entity (`IVersioned`, `IHasGuid`, `ICreationInfoModel`, `IUpdateInfoModel`) — see `class-entity`'s matrix.
3. Map exactly those same fields in the entity's `{Entity}Config` — concurrency token, unique `Guid` index, timestamp columns — nothing the entity doesn't implement.
4. Give create/update commands the matching marker interfaces (`IHasVersions`, `IHasGuid`, `ICommandWithTimestamp`) and, for external creation, a `{Entity}ByGuidSpec` + `Create{Entity}GuidResolver`; for mutable entities, a `{Entity}VersionResolver`.

## Update a mutable entity with optimistic concurrency
1. Client sends an update command carrying `Versions` (from a prior read's version) and `ActionTimeStamp`.
2. `ValidationBehavior` checks transport correctness; `ConcurrencyBehavior` resolves the entity's current version via `EntityVersionResolverFactory` → `{Entity}VersionResolver` and compares — a mismatch returns `Result.Conflict`, handler never runs.
3. The handler loads via `IRepository<T>` + `{Entity}ByIdSpec`, calls the entity's own behavior method, sets `UserUpdatedDateTime`, and returns.
4. `UnitOfWorkBehavior` commits once, at the top level; `AppDbContext.OnBeforeSaving` sets `ServerUpdatedDateTime`; EF's own `xmin` check is the final guard against a race `ConcurrencyBehavior` somehow missed.

## Create an externally-created entity idempotently
1. Client sends a create command carrying a client-generated `Guid` and `ActionTimeStamp`.
2. `GuidResolvingBehavior` resolves via `Create{Entity}GuidResolver` — if the `Guid` already exists, it short-circuits with `ConflictResult<T>` (same shape as success), handler never runs.
3. Otherwise the handler creates the entity (`Guid` set once, in the factory method), stages it via `IRepository<T>`, and `UnitOfWorkBehavior` commits. The database's own unique index on `Guid` is the last-resort guard against a race between two requests that both passed the pipeline check.

## Answer a cross-module read
1. A read needs data from two or more modules' entity types (e.g. a task with its assignee's details).
2. The query and its DTO are declared in the owning module's `{Module}.Interfaces`; the handler lives in `App.Queries`, injecting `AppDbContext` directly with `AsNoTracking()` and an explicit `join` — never `Include()`.
3. `AppQueriesRegistration` is registered last, in `ModuleRegistration.AddModules()`, after every module's own registration.
