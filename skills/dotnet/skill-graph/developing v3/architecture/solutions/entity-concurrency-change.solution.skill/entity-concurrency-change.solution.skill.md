---
uid: 8ae45d1d-feb8-44fc-b207-a97c69c45522
name: entity-concurrency-change
description: Defines the optimistic concurrency control stack for mutable entities — Version/xmin concurrency token on every mutable entity, IHasVersions interface on update commands, ETagEncoder for HTTP transport, IEntityVersionResolver in BuildingBlocks with App.Infrastructure implementation, ConcurrencyBehavior pipeline guard inserted between ValidationBehavior and UnitOfWorkBehavior, ETag on GET responses, and 412 Precondition Failed on missing or malformed If-Match
domain: skill
type: architecture
version: 20260611
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - application
  - infrastructure
  - concurrency
  - etag
  - rowversion
  - mediatr
triggers:
  - implement concurrency control
  - optimistic concurrency
  - prevent lost updates
  - version check
  - etag if-match
  - mutable entity
  - IHasVersions
creates:
  - BuildingBlocks.Concurrency.IHasVersions.cs
  - BuildingBlocks.Concurrency.IEntityVersionResolver.cs
  - BuildingBlocks.Concurrency.ETagEncoder.cs
  - BuildingBlocks.MediatR.ConcurrencyBehavior.cs
  - App.Infrastructure.Concurrency.EntityVersionResolver.cs
extends:
  - "{Module}.Domain.csproj"
  - "{Module}.Domain.Entities.{EntityName}.cs"
  - "{Module}.Domain.Configurations.{EntityName}Config.cs"
  - BuildingBlocks.csproj
  - App.Infrastructure.csproj
  - "{Module}.Interfaces.csproj"
  - "{Module}.Api.csproj"
  - App.Host.csproj
depends_on:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/command-integration.solution.skill/command-integration.solution.skill.md|command-integration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration.solution.skill]]"
---

# Goal
- Define `Version` / `xmin` as the concurrency token on every mutable entity — the database-level last line of defence
- Define `IHasVersions` in BuildingBlocks as the interface all update commands implement to carry client-supplied version information
- Define `ETagEncoder` in BuildingBlocks to encode entity versions as base64 JSON ETags for HTTP transport
- Define `IEntityVersionResolver` in BuildingBlocks and `EntityVersionResolver` in App.Infrastructure to map stable string entity names to C# types for version checking
- Define `ConcurrencyBehavior` in BuildingBlocks as the pipeline behavior that validates all versions before the handler runs — inserted between `ValidationBehavior` and `UnitOfWorkBehavior`
- Define the full ETag flow: GET encodes version into `ETag` header, PUT/PATCH decodes `If-Match` header, missing or malformed `If-Match` returns 412 before MediatR dispatch

# Core Principles
- `Version` is a `uint` property mapped to PostgreSQL `xmin` — auto-incremented by the database on every row change, never set by application code
- `IHasVersions` and `ETagEncoder` live in BuildingBlocks — referenced by both Application and Api layers
- `IEntityVersionResolver` lives in BuildingBlocks — implementation lives in App.Infrastructure
- Entity name string keys in `IHasVersions` are stable business names — never C# type names or namespaces — decouples HTTP contract from assembly structure
- `ConcurrencyBehavior` constrained on `where TRequest : IHasVersions` — only update commands are checked, not all commands
- `ConcurrencyBehavior` runs after `ValidationBehavior` and before `UnitOfWorkBehavior` — invalid or stale commands never open a unit of work
- Missing `If-Match` returns 412 Precondition Failed — not 400 (bad input) or 409 (conflict) — 412 means precondition not supplied
- EF concurrency token is the final guard — `ConcurrencyBehavior` is the early client-friendly check that gives a meaningful 409 before EF raises `DbUpdateConcurrencyException`
- All version mismatches return `Result.Conflict` from the behavior — handler never runs for stale updates
- ETag encodes ALL entity versions involved in the operation — not only the primary entity

# Requirements
- `System.Text.Json` — provides `JsonSerializer` used in `ETagEncoder`
- `Microsoft.EntityFrameworkCore` — provides `IsConcurrencyToken()` for `Version` EF configuration
- definition of `module project structure` — [[solution-structure.solution.skill]] defines BuildingBlocks, App.Infrastructure, App.Host, and module project boundaries
- definition of `domain configuration` — [[domain-configuration.solution.skill]] provides the EF configuration pattern for `Version` mapping
- definition of `domain behaviour` — [[domain-behaviour.solution.skill]] defines mutable entities that need concurrency control
- definition of `IReadRepository<T>` — [[repository-integration.solution.skill]] used by `ConcurrencyBehavior` to load entities for version checking
- definition of `command pipeline` — [[command-integration.solution.skill]] provides update commands extended with `IHasVersions`
- definition of `validation pipeline` — [[validation-behavior.solution.skill]] runs before `ConcurrencyBehavior` — pipeline order dependency
- definition of `unit of work` — [[unit-of-work.solution.skill]] runs after `ConcurrencyBehavior` — pipeline order dependency
- definition of `API layer` — [[http-api-publication.solution.skill]] provides controller structure for ETag and If-Match handling

# Template Skill Mutations

PROJECT:
- [[./Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - extend - Add Version concurrency token to every mutable entity
  - [[./Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs]] - extend - Add uint Version property with internal set
  - [[./Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs]] - extend - Map Version to xmin with IsConcurrencyToken and ValueGeneratedOnAddOrUpdate
- [[./Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - extend - Add IHasVersions, IEntityVersionResolver, ETagEncoder, and ConcurrencyBehavior
  - [[./Implementation/BuildingBlocks.csproj.extend/IHasVersions.cs.create.md|IHasVersions.cs]] - create - Interface for update commands carrying client-supplied version information
  - [[./Implementation/BuildingBlocks.csproj.extend/IEntityVersionResolver.cs.create.md|IEntityVersionResolver.cs]] - create - Maps stable string entity names to C# types
  - [[./Implementation/BuildingBlocks.csproj.extend/ETagEncoder.cs.create.md|ETagEncoder.cs]] - create - Encodes/decodes entity versions as base64 JSON ETags
  - [[./Implementation/BuildingBlocks.csproj.extend/ConcurrencyBehavior.cs.create.md|ConcurrencyBehavior.cs]] - create - Pipeline behavior validating versions before handler runs
- [[./Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj]] - extend - Add EntityVersionResolver implementation
  - [[./Implementation/App.Infrastructure.csproj.extend/EntityVersionResolver.cs.create.md|EntityVersionResolver.cs]] - create - Maps stable business entity names to C# types
- [[./Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj]] - extend - Require update and patch commands to implement IHasVersions
  - [[./Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs]] - extend - Update/patch command implements IHasVersions
- [[./Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj]] - extend - Add ETag header to GET responses and If-Match guard to PUT/PATCH
  - [[./Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.extend.md|Single{Entity}Controller.cs]] - extend - Add ETag encoding on GET and If-Match decoding on PUT/PATCH
- [[./Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Register IEntityVersionResolver and ConcurrencyBehavior in pipeline
  - [[./Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs]] - extend - Insert ConcurrencyBehavior between ValidationBehavior and UnitOfWorkBehavior in centralized pipeline registration
  - [[./Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.extend.md|RepositoryRegistration.cs]] - extend - Register IEntityVersionResolver as Singleton

# Rules

MUST:
- Every mutable entity has `public uint Version { get; internal set; }`
- Every mutable entity configuration maps `Version` to `xmin` with `IsConcurrencyToken()` and `ValueGeneratedOnAddOrUpdate()`
- All update and patch commands implement `IHasVersions`
- `IHasVersions`, `IEntityVersionResolver`, `ETagEncoder`, and `ConcurrencyBehavior` live in BuildingBlocks
- `EntityVersionResolver` lives in App.Infrastructure and registers every mutable entity type
- `EntityVersionResolver` registered as `Singleton` in App.Host
- Entity name keys in `IHasVersions` and `EntityVersionResolver` are stable business strings — never C# type names
- `ConcurrencyBehavior` registered after `ValidationBehavior` and before `UnitOfWorkBehavior`
- GET responses for mutable entities include `ETag` header with encoded versions
- PUT/PATCH endpoints check `If-Match` presence — return 412 if missing or malformed
- DTOs returned by GET for mutable entities include `Version` field
- `ConcurrencyBehavior` returns `Result.Conflict` on version mismatch — never throws
- `ConcurrencyBehavior` returns `Result.NotFound` if entity missing during version check

MUST NOT:
- Immutable entities have `Version` property
- Create or delete commands implement `IHasVersions`
- Handler check versions manually — `ConcurrencyBehavior` owns this
- Controller return 400 for missing `If-Match` — 412 Precondition Failed is correct
- Entity name keys use C# type names — breaks on entity rename
- `ConcurrencyBehavior` call `SaveChangesAsync`

# Anti-patterns
- `Version` as plain `uint` on command property instead of `IHasVersions` — does not scale to multi-entity updates
- Handler catches `DbUpdateConcurrencyException` and returns conflict — `ConcurrencyBehavior` should catch this earlier at the application level
- ETag encoding only primary entity version — misses secondary entity conflicts when command touches multiple entities
- `ConcurrencyBehavior` registered after `UnitOfWorkBehavior` — stale commands open a unit of work unnecessarily
- `EntityVersionResolver` key using `nameof(TodoTask)` — fragile, breaks on class rename; use a stable business string constant

# Check list
- [ ] `uint Version { get; internal set; }` on every mutable entity
- [ ] `Version` mapped to `xmin` with `IsConcurrencyToken()` and `ValueGeneratedOnAddOrUpdate()` in entity configuration
- [ ] `IHasVersions` defined in `BuildingBlocks/Concurrency/IHasVersions.cs`
- [ ] `IEntityVersionResolver` defined in `BuildingBlocks/Concurrency/IEntityVersionResolver.cs`
- [ ] `ETagEncoder` defined in `BuildingBlocks/Concurrency/ETagEncoder.cs`
- [ ] `ConcurrencyBehavior` defined in `BuildingBlocks/MediatR/ConcurrencyBehavior.cs`
- [ ] `EntityVersionResolver` defined in `App.Infrastructure/Concurrency/EntityVersionResolver.cs`
- [ ] Every mutable entity registered in `EntityVersionResolver._map`
- [ ] `EntityVersionResolver` registered as `Singleton` in App.Host
- [ ] `ConcurrencyBehavior` registered between `ValidationBehavior` and `UnitOfWorkBehavior`
- [ ] All update and patch commands implement `IHasVersions`
- [ ] GET for mutable entity sets `Response.Headers.ETag`
- [ ] DTO for mutable entity includes `Version` field
- [ ] PUT/PATCH checks `If-Match` — returns 412 if missing or malformed
- [ ] 412 added to `[ProducesResponseType]` on all PUT/PATCH actions
- [ ] `switch` default arm throws `InvalidOperationException` in PUT/PATCH actions

# Unittest TestCases
- [ ] When entity saved Then `Version` (xmin) is non-zero
- [ ] When entity updated Then `Version` changes
- [ ] When two DbContexts load same entity, first saves, second saves Then `DbUpdateConcurrencyException` thrown
- [ ] When `ETagEncoder.Encode` called Then produces valid base64 string
- [ ] When `ETagEncoder.Decode` called with valid ETag Then returns correct versions dictionary
- [ ] When `ETagEncoder.Decode` called with malformed string Then returns null
- [ ] When `If-Match` header missing Then controller returns 412 before MediatR dispatch
- [ ] When `If-Match` header malformed Then controller returns 412 before MediatR dispatch
- [ ] When version matches Then `ConcurrencyBehavior` calls next — handler runs
- [ ] When version mismatches Then `ConcurrencyBehavior` returns `Result.Conflict` — handler does not run
- [ ] When entity not found during version check Then `ConcurrencyBehavior` returns `Result.NotFound`
- [ ] When command has multiple entities and one mismatches Then `Result.Conflict` without updating any
- [ ] When unknown entity name in `IHasVersions` Then `ConcurrencyBehavior` returns `Result.Error`
