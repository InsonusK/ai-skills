---
uid: 626c5f22-c4bc-41c7-8a9d-cb3f60b70a53
status: draft
name: cqrs-architecture
description: architecture decision record for CQRS — command and query separation, MediatR as dispatcher, read and write stack design
domain: skill
type: architecture
tags:
  - dotnet
  - architecture
  - cqrs
  - mediatr
  - commands
  - queries
triggers:
  - cqrs design
  - command query separation
  - mediatr architecture
  - read write separation
---
# Goal

Define the system-level architecture for command and query separation. This skill is the entry point for understanding how write and read operations are structured, dispatched, and implemented across the application. It records the decisions made, explains why, and maps to implementation skills for each component.

# Architecture Decision

## Decision: MediatR as dispatcher, EF Core for both reads and writes, App.Queries for cross-module reads

### Why CQRS

A single model serving both reads and writes creates tension:

- Write model enforces invariants — it needs full entity graphs, domain logic, and transactional consistency
- Read model serves UI — it needs flat DTOs, projections, and fast queries

Separating them means each model is optimized for its purpose. Write side enforces correctness. Read side optimizes for presentation.

### Why MediatR as the dispatcher

MediatR decouples the caller from the handler. The API layer sends a command or query without knowing which class handles it, where it lives, or what module owns it. This makes cross-module communication possible without direct assembly references — any module can dispatch a command to another module via `IMediator.Send()`.

### Why not separate read and write databases

A separate read database (read replica, event-projected read store) adds operational complexity — replication lag, eventual consistency, synchronization failures. For the current scale, a single PostgreSQL database with `AsNoTracking()` on reads provides sufficient performance without the operational overhead. If read performance becomes a bottleneck, a read replica can be added without changing the application architecture.

### Why App.Queries for cross-module reads

Single-module queries are served by `{Module}.Application` using `IReadRepository<T>`. Cross-module queries require JOINs across module boundaries — this is not allowed in Application because Application must not reference other modules' Domain directly. `App.Queries` has access to all entity types and DbContext — it is the only place where cross-module JOINs are intentional and correct.

### Why AsNoTracking on all read queries

EF change tracking adds overhead on every loaded entity — it registers the entity in the identity map, tracks property snapshots, and participates in SaveChanges detection. Read queries never modify entities — tracking is wasted overhead. `AsNoTracking()` applied in `IReadRepository<T>` ensures all read operations are tracking-free without requiring each handler to remember to add it.

# Full Request Flow

## Write (Command)

```
HTTP POST/PUT/PATCH/DELETE
    ↓
API Controller — extracts If-Match, maps to Command, dispatches via IMediator.Send()
    ↓
ValidationBehavior — validates command via FluentValidation, returns Result.Invalid if invalid
    ↓
GuidResolvingBehavior — (creation only) checks Guid uniqueness, returns 409 if duplicate
    ↓
ConcurrencyBehavior — (update only) validates entity versions from If-Match ETag
    ↓
UnitOfWorkBehavior — begins depth tracking
    ↓
Command Handler — loads entities via IRepository<T> + Specifications, calls domain, dispatches sub-commands
    ↓
UnitOfWorkBehavior — SaveChanges if depth == 1 (top-level command only)
    ↓
API Controller — maps Result<T> to HTTP response, sets ETag on response
```

## Read (Query)

```
HTTP GET
    ↓
API Controller — maps route/query string to Query, dispatches via IMediator.Send()
    ↓
Query Handler — loads data via IReadRepository<T> + Specifications (AsNoTracking)
    or
App.Queries Handler — direct DbContext with LINQ projection (cross-module JOIN)
    ↓
Returns Result<TDto>
    ↓
API Controller — maps Result<T> to HTTP response
```

# Component Map

|Component|Lives in|Skill|
|---|---|---|
|`ICommand<T>` marker interface|BuildingBlocks|[[command-handler-pattern.skill]]|
|`IQuery<T>` marker interface|BuildingBlocks|[[query-handler-pattern.skill]]|
|`ValidationBehavior`|BuildingBlocks|[[command-handler-pattern.skill]]|
|`GuidResolvingBehavior`|BuildingBlocks|[[guid-resolving-pipeline.skill]]|
|`ConcurrencyBehavior`|BuildingBlocks|[[concurrency-control-pattern.skill]]|
|`UnitOfWorkBehavior` + `UnitOfWorkContext`|BuildingBlocks|[[repository-pattern.skill]]|
|`IRepository<T>` / `IReadRepository<T>`|BuildingBlocks|[[repository-pattern.skill]]|
|`IUnitOfWork`|BuildingBlocks|[[repository-pattern.skill]]|
|Command declaration|`{Module}.Interfaces/Commands`|[[command-handler-pattern.skill]]|
|Query declaration|`{Module}.Interfaces/Queries`|[[query-handler-pattern.skill]]|
|DTO declaration|`{Module}.Interfaces/DTOs`|[[query-handler-pattern.skill]]|
|Command handler|`{Module}.Application/Features`|[[command-handler-pattern.skill]]|
|Query handler (single-module)|`{Module}.Application/Features`|[[query-handler-pattern.skill]]|
|Query handler (cross-module)|`App.Queries/Queries`|[[query-handler-pattern.skill]]|
|Validator|`{Module}.Application/Features`|[[command-handler-pattern.skill]]|
|Ardalis Specification|`{Module}.Domain` or `{Module}.Application`|[[ardalis-specification-pattern.skill]]|

# Pipeline Registration Order

Order matters — behaviors execute in registration order.

```
1. ValidationBehavior      — fail fast on invalid input
2. GuidResolvingBehavior   — fail fast on duplicate creation
3. ConcurrencyBehavior     — fail fast on stale version
4. UnitOfWorkBehavior      — commit only after all checks pass
```

# Identity Rules

- Entities are addressed by `int Id` in all operations after creation
- `Guid` appears only on creation commands — correlation handle for external systems
- `Guid` never appears in routes, query strings, or update commands
- See [[async-external-creation.skill]] for the creation flow

# Read Stack Design

|Scenario|Handler location|Data access|Tracking|
|---|---|---|---|
|Single entity by Id|`{Module}.Application`|`IReadRepository<T>` + Spec|AsNoTracking|
|Collection with filters|`{Module}.Application`|`IReadRepository<T>` + Spec|AsNoTracking|
|Projection to DTO|`{Module}.Application`|`Specification<T, TDto>`|AsNoTracking|
|Cross-module JOIN|`App.Queries`|Direct DbContext + LINQ|AsNoTracking|

# Core Principles

- Commands express write intent — they change state and return `Result<T>`
- Queries express read intent — they never change state and return `Result<T>`
- `UnitOfWorkBehavior` activates only for `ICommand` — queries never trigger SaveChanges
- All pipeline behaviors live in BuildingBlocks — registered once in App.Host
- Module owns its contracts — commands and queries declared in `{Module}.Interfaces`
- Cross-module writes go through MediatR command dispatch — never direct method calls
- Cross-module reads go through App.Queries — never direct DbContext in Application

# Anti-patterns

- Command handler reads data and returns DTO — split into command + query
- Query handler modifies entity state — never acceptable, use command
- Pipeline behavior registered inside module — register once in App.Host
- Cross-module write via direct Application method call — use `_mediator.Send()`
- Cross-module JOIN in `{Module}.Application` — belongs in App.Queries
- Raw LINQ in single-module handler — use Ardalis Specification

# Checklist

- [ ] All write operations use `ICommand<Result<T>>`
- [ ] All read operations use `IQuery<Result<T>>`
- [ ] Pipeline behaviors registered in correct order in App.Host
- [ ] `IReadRepository<T>` uses `AsNoTracking()` on all queries
- [ ] Commands declared in `{Module}.Interfaces/Commands`
- [ ] Queries and DTOs declared in `{Module}.Interfaces/Queries` and `/DTOs`
- [ ] Cross-module queries implemented in App.Queries
- [ ] Guid used only on creation commands — int Id everywhere else

# Relations

- [[command-handler-pattern.skill]] — command structure, validation, handler pattern
- [[query-handler-pattern.skill]] — query structure, single-module vs cross-module
- [[repository-pattern.skill]] — IRepository, IReadRepository, UnitOfWork, UnitOfWorkBehavior
- [[ardalis-specification-pattern.skill]] — query descriptors used in all read operations
- [[module-application.skill]] — Application layer structure and DI registration
- [[backend-project-structure.skill]] — where each component lives in the solution
- [[cross-module-interaction.skill]] — cross-module command and query contracts
- [[concurrency-control-pattern.skill]] — ConcurrencyBehavior in the pipeline
- [[guid-resolving-pipeline.skill]] — GuidResolvingBehavior in the pipeline
- [[async-external-creation.skill]] — Guid identity rules for creation