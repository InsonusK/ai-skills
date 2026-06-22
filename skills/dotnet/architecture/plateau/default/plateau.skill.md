---
uid: 7f8e9d2c-3b4a-4c5d-9e6f-0a1b2c3d4e5f
name: default-plateau
description: Default v3 architecture plateau — modular DDD solution with entity classification, optimistic concurrency, external-created entities, command/query integration, repository abstractions, and centralized pipeline/host wiring
domain: skill
type: template
version: 20260622
tags:
  - skill/template/plateau
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-classification.solution.skill/entity-classification.solution.skill.md|entity-classification]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md|value-objects-and-rules]]"
---

# Core Principals
- Every domain entity is classified along two axes: ownership (internal vs external identity) and mutability (immutable vs mutable). The resulting four types determine exactly which infrastructure patterns apply.
- Cross-cutting contracts and primitives live in `Shared`; concrete technical patterns live in `BuildingBlocks`; persistence implementations live in `App.Infrastructure`; composition root lives in `App.Host`.
- Mutable entities use PostgreSQL `xmin` as an optimistic concurrency token. The version is never set by application code; it is transported via ETag/If-Match and checked by a pipeline behavior before the handler runs.
- Concurrency checks are performed through a factory-resolver chain: `IEntityVersionResolverFactory` resolves the correct `IEntityVersionResolver` for a stable business entity name; per-entity resolvers in module Application projects read the current version using the module's own specifications and repositories.
- External-created entities carry a client-supplied `Guid` used for idempotency. A dedicated pipeline behavior short-circuits duplicate creates before the handler runs.
- Write operations are commands, read operations are queries, and both are routed through MediatR. Pipeline behaviors (validation, concurrency, Guid resolution, unit-of-work) are registered centrally in `App.Host`.
- Each module owns its Domain, Application, Interfaces, and Api projects. Cross-module read models live in `App.Queries`. Cross-module foreign key configurations live in `App.Infrastructure`.

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-classification.solution.skill/entity-classification.solution.skill.md|entity-classification]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/Repository.create.md|Repository.create]]

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
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]]
