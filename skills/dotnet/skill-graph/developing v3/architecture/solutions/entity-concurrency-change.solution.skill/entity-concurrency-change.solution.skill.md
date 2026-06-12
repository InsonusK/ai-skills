---
uid: 8ae45d1d-feb8-44fc-b207-a97c69c45522
name: entity-concurrency-change
description: Defines the optimistic concurrency control stack for mutable entities — Version/xmin concurrency token on every mutable entity, IVersioned marker on entities, IHasVersions interface on update commands, ETagEncoder for HTTP transport, IEntityVersionResolver in Shared with App.Infrastructure implementation, ConcurrencyBehavior pipeline guard inserted between ValidationBehavior and UnitOfWorkBehavior, ETag on GET responses, and 412 Precondition Failed on missing or malformed If-Match
domain: skill
type: architecture
version: 20260612
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
  - IVersioned
  - IHasVersions
creates:
  - Shared.Concurrency.IVersioned.cs
  - Shared.Concurrency.IHasVersions.cs
  - Shared.Concurrency.IEntityVersionResolver.cs
  - BuildingBlocks.Concurrency.ETagEncoder.cs
  - BuildingBlocks.MediatR.ConcurrencyBehavior.cs
  - BuildingBlocks.Specifications.EntityByIdSpec.cs
  - App.Infrastructure.Concurrency.EntityVersionResolver.cs
extends:
  - "{Module}.Domain.csproj"
  - "{Module}.Domain.Entities.{EntityName}.cs"
  - "{Module}.Domain.Configurations.{EntityName}Config.cs"
  - Shared.csproj
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
- Define `IVersioned` in Shared as the marker all mutable entities implement
- Define `IHasVersions` in Shared as the interface all update commands implement to carry client-supplied version information
- Define `IEntityVersionResolver` in Shared and `EntityVersionResolver` in App.Infrastructure to map stable string entity names to C# types for version checking
- Define `ETagEncoder` in BuildingBlocks to encode entity versions as base64 JSON ETags for HTTP transport
- Define `ConcurrencyBehavior` in BuildingBlocks as the pipeline behavior that validates all versions before the handler runs — inserted between `ValidationBehavior` and `UnitOfWorkBehavior`
- Define the full ETag flow: GET encodes version into `ETag` header, PUT/PATCH decodes `If-Match` header, missing or malformed `If-Match` returns 412 before MediatR dispatch

# Core Principles
- `Version` is a `uint` property mapped to PostgreSQL `xmin` — auto-incremented by the database on every row change, never set by application code
- `IVersioned`, `IHasVersions`, and `IEntityVersionResolver` live in Shared — common contracts referenced by Domain, Application, Api, and Infrastructure layers
- `ETagEncoder` and `ConcurrencyBehavior` live in BuildingBlocks — concrete implementations of technical patterns
- `IEntityVersionResolver` implementation lives in App.Infrastructure
- Entity name string keys in `IHasVersions` are stable business names — never C# type names or namespaces — decouples HTTP contract from assembly structure
- `ConcurrencyBehavior` constrained on `where TRequest : IHasVersions` — only update commands are checked, not all commands
- `ConcurrencyBehavior` runs after `ValidationBehavior` and before `UnitOfWorkBehavior` — invalid or stale commands never open a unit of work
- Missing `If-Match` returns 412 Precondition Failed — not 400 (bad input) or 409 (conflict) — 412 means precondition not supplied
- EF concurrency token is the final guard — `ConcurrencyBehavior` is the early client-friendly check that gives a meaningful 409 before EF raises `DbUpdateConcurrencyException`
- All version mismatches return `Result.Conflict` from the behavior — handler never runs for stale updates
- ETag encodes ALL entity versions involved in the operation — not only the primary entity
- `EntityVersionResolver` discovers versioned entities automatically by scanning module Domain assemblies for `IVersioned` implementations

# Requirements
SOLUTION:
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/Shared.csproj.create.md|Shared.csproj]] - hosts common concurrency contracts (`IVersioned`, `IHasVersions`, `IEntityVersionResolver`)
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj]] - hosts `ETagEncoder` and `ConcurrencyBehavior`
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj]] - hosts `EntityVersionResolver` implementation
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj]] - hosts pipeline and resolver registrations
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj]] - hosts ETag and If-Match controller handling
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj]] - hosts update/patch commands implementing `IHasVersions`
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-configuration.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - provides EF Core configuration pattern for `Version` mapping
    - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-configuration.solution.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md|{Entity}Config.cs]] - maps `Version` to `xmin` with `IsConcurrencyToken`
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-behaviour.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - defines mutable entities that require concurrency control
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/repository-integration.solution.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - provides `IReadRepository<T>` used to load entities for version checking
    - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/repository-integration.solution.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md|IReadRepository.cs]] - used by `ConcurrencyBehavior` to read entity versions
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/command-integration.solution.skill/command-integration.solution.skill.md|command-integration.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/command-integration.solution.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - provides `ICommand<T>` marker extended with `IHasVersions`
    - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/command-integration.solution.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs]] - marker interface for commands that `ConcurrencyBehavior` constrains on
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - provides `ValidationBehavior` that must run before `ConcurrencyBehavior`
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/unit-of-work.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - provides `UnitOfWorkBehavior` that must run after `ConcurrencyBehavior`
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/http-api-publication.solution.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj]] - provides controller structure for ETag and If-Match handling
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - provides centralized `PipelineRegistration` where `ConcurrencyBehavior` is ordered

NUGET:
- `System.Text.Json` {version} - provides `JsonSerializer` used in `ETagEncoder`
- `Microsoft.EntityFrameworkCore` {version} - provides `IsConcurrencyToken()` for `Version` EF configuration

# Template Skill Mutations

PROJECT:
- [[./Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - extend - Add Version concurrency token to every mutable entity and implement IVersioned
  - [[./Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs]] - extend - Add uint Version property with internal set and implement IVersioned
  - [[./Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs]] - extend - Map Version to xmin with IsConcurrencyToken and ValueGeneratedOnAddOrUpdate
- [[./Implementation/Shared.csproj.extend.md|Shared.csproj]] - extend - Add IVersioned, IHasVersions, and IEntityVersionResolver
  - [[./Implementation/Shared.csproj.extend/IVersioned.cs.create.md|IVersioned.cs]] - create - Marker interface for versioned domain entities
  - [[./Implementation/Shared.csproj.extend/IHasVersions.cs.create.md|IHasVersions.cs]] - create - Interface for update commands carrying client-supplied version information
  - [[./Implementation/Shared.csproj.extend/IEntityVersionResolver.cs.create.md|IEntityVersionResolver.cs]] - create - Maps stable string entity names to C# types
- [[./Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - extend - Add ETagEncoder, EntityByIdSpec, and ConcurrencyBehavior
  - [[./Implementation/BuildingBlocks.csproj.extend/ETagEncoder.cs.create.md|ETagEncoder.cs]] - create - Encodes/decodes entity versions as base64 JSON ETags
  - [[./Implementation/BuildingBlocks.csproj.extend/EntityByIdSpec.cs.create.md|EntityByIdSpec.cs]] - create - Generic by-Id spec used by ConcurrencyBehavior at runtime
  - [[./Implementation/BuildingBlocks.csproj.extend/ConcurrencyBehavior.cs.create.md|ConcurrencyBehavior.cs]] - create - Pipeline behavior validating versions before handler runs
- [[./Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj]] - extend - Add EntityVersionResolver implementation
  - [[./Implementation/App.Infrastructure.csproj.extend/EntityVersionResolver.cs.create.md|EntityVersionResolver.cs]] - create - Maps stable business entity names to C# types by scanning assemblies
- [[./Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj]] - extend - Require update and patch commands to implement IHasVersions
  - [[./Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs]] - extend - Update/patch command implements IHasVersions
- [[./Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj]] - extend - Add ETag header to GET responses and If-Match guard to PUT/PATCH
  - [[./Implementation/{Module}.Api.csproj.extend/Single{Entity}Controller.cs.extend.md|Single{Entity}Controller.cs]] - extend - Add ETag encoding on GET and If-Match decoding on PUT/PATCH
- [[./Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Register IEntityVersionResolver and ConcurrencyBehavior in pipeline
  - [[./Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs]] - extend - Insert ConcurrencyBehavior between ValidationBehavior and UnitOfWorkBehavior in centralized pipeline registration
  - [[./Implementation/App.Host.csproj.extend/EntityVersionResolverRegistration.cs.create.md|EntityVersionResolverRegistration.cs]] - create - Register IEntityVersionResolver as Singleton with module Domain assemblies

# Rules

MUST:
- Every mutable entity has `public uint Version { get; internal set; }`
- Every mutable entity implements `IVersioned`
- Every mutable entity declares a public `VersionedEntityName` constant with its stable business name
- Every mutable entity configuration maps `Version` to `xmin` with `IsConcurrencyToken()` and `ValueGeneratedOnAddOrUpdate()`
- All update and patch commands implement `IHasVersions`
- `IVersioned`, `IHasVersions`, and `IEntityVersionResolver` live in Shared
- `ETagEncoder` and `ConcurrencyBehavior` live in BuildingBlocks
- `EntityVersionResolver` lives in App.Infrastructure and discovers mutable entity types by scanning assemblies for `IVersioned`
- `EntityVersionResolver` registered as `Singleton` in App.Host via `EntityVersionResolverRegistration`
- `EntityVersionResolver` receives all module Domain assemblies from App.Host
- Entity name keys in `IHasVersions` and `EntityVersionResolver` are stable business strings — never C# type names
- `ConcurrencyBehavior` registered after `ValidationBehavior` and before `UnitOfWorkBehavior`
- GET responses for mutable entities include `ETag` header with encoded versions
- PUT/PATCH endpoints check `If-Match` presence — return 412 if missing or malformed
- DTOs returned by GET for mutable entities include `Version` field
- `ConcurrencyBehavior` returns `Result.Conflict` on version mismatch — never throws
- `ConcurrencyBehavior` returns `Result.NotFound` if entity missing during version check
- `ConcurrencyBehavior` reads `Version` through `IVersioned` — no reflection

MUST NOT:
- Immutable entities have `Version` property or implement `IVersioned`
- Create or delete commands implement `IHasVersions`
- Handler check versions manually — `ConcurrencyBehavior` owns this
- Controller return 400 for missing `If-Match` — 412 Precondition Failed is correct
- Entity name keys use C# type names — breaks on entity rename
- `ConcurrencyBehavior` call `SaveChangesAsync`
- `ConcurrencyBehavior` use reflection to read `Version`

# Anti-patterns
- `Version` as plain `uint` on command property instead of `IHasVersions` — does not scale to multi-entity updates
- Handler catches `DbUpdateConcurrencyException` and returns conflict — `ConcurrencyBehavior` should catch this earlier at the application level
- ETag encoding only primary entity version — misses secondary entity conflicts when command touches multiple entities
- `ConcurrencyBehavior` registered after `UnitOfWorkBehavior` — stale commands open a unit of work unnecessarily
- `EntityVersionResolver` key using `nameof(TodoTask)` — fragile, breaks on class rename; use a stable business string constant
- Hardcoded entity dictionary in `EntityVersionResolver` — duplicates the entity list and is easy to forget; scan assemblies for `IVersioned` instead
- Defining `IHasVersions` or `IEntityVersionResolver` in BuildingBlocks — violates the rule that common contracts live in Shared

# Check list
- [ ] `uint Version { get; internal set; }` on every mutable entity
- [ ] Every mutable entity implements `IVersioned`
- [ ] Every mutable entity declares a public `VersionedEntityName` constant
- [ ] `Version` mapped to `xmin` with `IsConcurrencyToken()` and `ValueGeneratedOnAddOrUpdate()` in entity configuration
- [ ] `IVersioned` defined in `Shared/Concurrency/IVersioned.cs`
- [ ] `IHasVersions` defined in `Shared/Concurrency/IHasVersions.cs`
- [ ] `IEntityVersionResolver` defined in `Shared/Concurrency/IEntityVersionResolver.cs`
- [ ] `ETagEncoder` defined in `BuildingBlocks/Concurrency/ETagEncoder.cs`
- [ ] `EntityByIdSpec<T>` defined in `BuildingBlocks/Specifications/EntityByIdSpec.cs`
- [ ] `ConcurrencyBehavior` defined in `BuildingBlocks/MediatR/ConcurrencyBehavior.cs`
- [ ] `EntityVersionResolver` defined in `App.Infrastructure/Concurrency/EntityVersionResolver.cs`
- [ ] `EntityVersionResolver` constructor accepts `IEnumerable<Assembly>`
- [ ] `EntityVersionResolver` scans supplied assemblies for `IVersioned` implementations
- [ ] `EntityVersionResolver` registered as `Singleton` in App.Host
- [ ] `EntityVersionResolver` receives module Domain assemblies from App.Host
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
- [ ] When mutable entity inspected Then it declares a public `VersionedEntityName` constant
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
