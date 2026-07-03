---
name: solution-pipeline-registration-order
description: Defines the canonical execution order of all MediatR pipeline behaviors in App.Host.PipelineRegistration — ValidationBehavior first, then GuidResolvingBehavior, then ConcurrencyBehavior, and UnitOfWorkBehavior last
domain: skill
type: architecture
version: 20260615
tags:
  - skill/architecture/solution
  - dotnet
  - application
  - mediatr
  - pipeline
  - pipeline-registration
  - ordering
  - sorting
triggers:
  - pipeline order
  - pipeline sorting
  - register pipeline behaviors
  - mediatr pipeline order
  - behavior order
  - behavior sequence
creates:
extends:
  - App.Host.csproj
  - App.Host.DependencyInjection.PipelineRegistration.cs
depends_on:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work.skill]]"
---

# Goal
- Define the canonical execution order for all cross-cutting MediatR pipeline behaviors in one place
- Make `PipelineRegistration.AddPipeline()` the single source of truth for behavior order
- Ensure invalid input, duplicate external creates, stale concurrency checks, and unit-of-work commit run in the correct sequence

# Capabilities
- Canonical, documented order for all cross-cutting pipeline behaviors
- Early rejection of invalid or dangerous commands before expensive checks
- Correct interaction between validation, idempotency, concurrency, and commit
- Single source of truth for pipeline ordering
- Prevention of subtle bugs from behavior misordering

# Core Principles
- `PipelineRegistration.cs` is the single source of truth for pipeline behavior order
- Behaviors are registered in execution order — first registered runs first
- Pipeline order is: `ValidationBehavior` → `GuidResolvingBehavior` → `ConcurrencyBehavior` → `UnitOfWorkBehavior`
- Each behavior rejects invalid commands as early as possible so later behaviors never run unnecessarily
- Pipeline behaviors are registered centrally in App.Host — never inside a module's registration method

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - provides the centralized `PipelineRegistration.AddPipeline()` extension point
- [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - provides `ValidationBehavior` that must run first
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - provides `GuidResolvingBehavior` that runs after validation and before concurrency
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - provides `ConcurrencyBehavior` that runs after idempotency and before unit of work
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - provides `UnitOfWorkBehavior` that must run last

NUGET:
- None — relies only on packages already required by dependency solutions.

# Template Skill Mutations

PROJECT:
- [[./Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Add the complete ordered behavior registrations to centralized `PipelineRegistration.AddPipeline()`
  - [[./Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs]] - extend - Register all pipeline behaviors in execution order

# Rules

## MUST:
- [[./Implementation/App.Host.csproj.extend.md#MUST|App.Host.csproj.extend]]
	- [[./Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md#MUST|PipelineRegistration.cs.extend]]
- `AddPipeline()` called once from `Program.cs`
  1. `ValidationBehavior`
  2. `GuidResolvingBehavior`
  3. `ConcurrencyBehavior`
  4. `UnitOfWorkBehavior`

## SHOULD:
- Document the ordering with inline comments in `AddPipeline()`

## MUST NOT:
- [[./Implementation/App.Host.csproj.extend.md#MUST NOT|App.Host.csproj.extend]]
	- [[./Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md#MUST NOT|PipelineRegistration.cs.extend]]
- Change pipeline order in multiple files

# Anti-patterns
- Pipeline order scattered across multiple files
- `AddPipeline()` duplicated or replaced by module-specific registration methods
- Registering behaviors directly in `Program.cs` instead of inside `PipelineRegistration`
- `UnitOfWorkBehavior` registered before `ValidationBehavior` — invalid commands would open a unit of work
- `ConcurrencyBehavior` registered before `GuidResolvingBehavior` — duplicate external creates would run a version check against a non-existent entity
- `GuidResolvingBehavior` registered before `ValidationBehavior` — invalid commands would hit the database lookup

# Check list
- [ ] `PipelineRegistration.cs` exists under `App.Host/DependencyInjection`
- [ ] `AddPipeline()` called from `Program.cs`
- [ ] `ValidationBehavior` registered first
- [ ] `GuidResolvingBehavior` registered after `ValidationBehavior`
- [ ] `ConcurrencyBehavior` registered after `GuidResolvingBehavior`
- [ ] `UnitOfWorkBehavior` registered last
- [ ] No behavior registrations outside `PipelineRegistration.cs`
- [ ] Inline comments document the execution order
