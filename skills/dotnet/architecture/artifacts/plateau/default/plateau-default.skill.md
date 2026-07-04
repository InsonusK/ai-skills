---
name: plateau-default
description: Default v3 architecture plateau — modular DDD solution with entity classification, optimistic concurrency, external-created entities, entity edit timestamps, command/query integration, repository abstractions, soft value objects and DTO validators, global exception handling, and centralized pipeline/host wiring
domain: skill
type: template
version: 20260704153836
tags:
  - skill/template/plateau
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill|solution-command-integration]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill|class-validation-behavior]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill|class-pipeline-registration]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-pipeline-registration-order.skill/solution-pipeline-registration-order.skill|solution-pipeline-registration-order]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]"
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]]"
---

# Core Principals
- Every domain entity is classified along two axes: ownership (internal vs external identity) and mutability (immutable vs mutable). The resulting four types determine exactly which infrastructure patterns apply.
- Cross-cutting contracts and primitives live in `Shared`; concrete technical patterns live in `BuildingBlocks`; persistence implementations live in `App.Infrastructure`; composition root lives in `App.Host`.
- Mutable entities use PostgreSQL `xmin` as an optimistic concurrency token. The version is never set by application code; it is transported via ETag/If-Match and checked by a pipeline behavior before the handler runs.
- Concurrency checks are performed through a factory-resolver chain: `IEntityVersionResolverFactory` resolves the correct `IEntityVersionResolver` for a stable business entity name; per-entity resolvers in module Application projects read the current version using the module's own specifications and repositories.
- External-created entities carry a client-supplied `Guid` used for idempotency. A dedicated pipeline behavior short-circuits duplicate creates before the handler runs.
- User-initiated entities record creation and update timestamps: the client supplies `ActionTimeStamp` via `ICommandWithTimestamp`, the handler assigns user timestamps, and `AppDbContext` assigns authoritative server timestamps before saving.
- Write operations are commands, read operations are queries, and both are routed through MediatR. Pipeline behaviors (exception handling, validation, concurrency, Guid resolution, unit-of-work) are registered centrally in `App.Host`.
- Each module owns its Domain, Application, Interfaces, and Api projects. Cross-module read models live in `App.Queries`. Cross-module foreign key configurations live in `App.Infrastructure`.
- A global `ExceptionHandlingBehavior` is registered first in the MediatR pipeline. It catches all unhandled exceptions, logs them as critical, and returns a generic `Result.Error` so the API never sees raw exceptions.

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill|solution-command-integration]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill|class-pipeline-registration]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-sln-structure.skill/Implementation/Repository.create|Repository]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]]

# Capabilities

## Solution structure
- Defines modules as self-contained bounded contexts with four projects (`Api`, `Application`, `Domain`, `Interfaces`).
- Enforces inward dependency direction and prevents hidden coupling between modules.
- Establishes solution-wide layer responsibilities (`Shared`, `BuildingBlocks`, `App.Host`, `App.Infrastructure`, `App.Queries`).
- Provides clear file and project placement rules across the entire solution.

## Domain modeling
- Encodes domain semantics into immutable, self-validating Value Objects.
- Encapsulates reusable business predicates as stateless, deterministic Domain Rules.
- Keeps domain entities free of EF attributes and infrastructure concerns via dedicated `IEntityTypeConfiguration<T>` classes.
- Extracts cross-module Value Objects and Rules into `Shared`.

## Validation
- Exposes soft, validation-agnostic value objects from `{Module}.Interfaces` so other modules can use them in commands and DTOs without referencing Domain.
- Keeps strict invariant enforcement in Domain Value Objects that inherit from their soft counterparts.
- Registers FluentValidation validators for public RequestDto and Soft Value Object properties in `{Module}.Application`.
- Does not register validators for ResponseDto unless a concrete requirement explicitly demands it.
- Allows other modules to validate values by resolving `IValidator<T>` from DI without direct coupling to the owning module's Application or Domain.

## Entity identity and lifecycle
- Classifies every entity by ownership (internal/external) and mutability (immutable/mutable).
- Applies the right infrastructure deterministically based on entity type.
- Supports externally-created entities with client-generated `Guid` correlation handles and idempotent create flows.
- Prevents over-engineering by avoiding unnecessary concurrency or external-identity infrastructure.

## Concurrency and consistency
- Adds optimistic concurrency control with `Version`/`xmin` for all mutable entities.
- Encodes entity versions into ETags and validates `If-Match` preconditions before handlers run.
- Returns consistent `409 Conflict`/`412 Precondition Failed` semantics for stale or missing preconditions.

## Commands and queries
- Standardizes command/handler/validator structure with a fixed load → guard → domain call → stage → return flow.
- Separates read and write operations via `ICommand<T>` and `IQuery<T>` markers.
- Enables cross-module writes and reads through MediatR without direct coupling.
- Supports cross-module JOIN queries in a dedicated `App.Queries` layer.

## Persistence
- Decouples the Application layer from EF Core via `IRepository<T>` and `IReadRepository<T>` abstractions.
- Encapsulates query intent into named, reusable specifications.
- Commits all staged changes atomically through a single Unit of Work after the top-level command.
- Prevents premature commits for nested sub-commands via a scoped depth tracker.

## Entity edit timestamps
- Classifies every user-initiated entity by mutability to decide whether it carries creation-only or creation+update timestamp contracts.
- Commands carry the client action time through `ICommandWithTimestamp`; validators ensure it is present and not in the future.
- Handlers assign user timestamps through explicit timestamp interface casts so entity class-level setters can remain `internal`.
- `AppDbContext.OnBeforeSaving` assigns `ServerCreatedDateTime` and `ServerUpdatedDateTime` using `DateTimeOffset.UtcNow`.

## Pipeline
- Centralizes all MediatR pipeline behavior registrations in `App.Host`.
- Defines canonical execution order: Exception handling → Validation → Guid resolution → Concurrency → Unit of work.
- Wraps the entire pipeline with `ExceptionHandlingBehavior` first so no unhandled exception reaches the API.
- Validates transport input before any expensive checks run.
- Short-circuits duplicate external creates and stale updates before handlers execute.
- Logs every unhandled exception as critical and returns a safe generic `Result.Error` to API consumers.

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill|class-pipeline-registration]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-pipeline-registration-order.skill/solution-pipeline-registration-order.skill|solution-pipeline-registration-order]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]]

## API publication
- Publishes a thin HTTP adapter layer over MediatR with no business logic leakage.
- Provides consistent controller naming, route conventions, and `Result<T>`-to-HTTP mapping.
- Returns uniform `ProblemDetails` error responses with explicit `ProducesResponseType` declarations.
- Separates entity lifecycle controllers from system-level Minimal API endpoints.

# Usecases

## Update a mutable entity with optimistic concurrency control

```mermaid
sequenceDiagram
    participant Client
    participant Api as {Module}.Api
    participant Behavior as ConcurrencyBehavior
    participant Factory as EntityVersionResolverFactory
    participant Resolver as {Entity}VersionResolver
    participant Handler as Update{Entity}CommandHandler

    Client->>Api: GET /{entity}/2
    Api-->>Client: 200 OK + ETag("Task":{"2":3})

    Client->>Api: PUT /{entity}/2 If-Match: <etag>
    Api->>Behavior: Send(command with Versions)
    Behavior->>Factory: GetFor("Task")
    Factory->>Resolver: resolve TaskVersionResolver
    loop for each (id, expectedVersion)
        Behavior->>Resolver: GetCurrentVersionForAsync(2)
        Resolver->>Resolver: FirstOrDefaultAsync({Entity}ByIdSpec)
        Resolver-->>Behavior: 3
        Behavior->>Behavior: assert 3 == expected
    end
    Behavior->>Handler: next()
    Handler-->>Api: Result.NoContent
    Api-->>Client: 204 No Content

    alt version mismatch
        Resolver-->>Behavior: 7
        Behavior-->>Api: Result.Conflict
        Api-->>Client: 409 Conflict
    end
```

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]]
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]

