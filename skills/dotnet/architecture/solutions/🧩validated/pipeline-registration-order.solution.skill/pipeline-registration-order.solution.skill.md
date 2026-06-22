---
uid: 7d4e8c2a-9b1f-4a6e-8d3c-5e7f9a2b4c1d
name: pipeline-registration-order
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
  - "[[skills/dotnet/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration.solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior.solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity.solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change.solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work.solution.skill]]"
---

# Goal
- Define the canonical execution order for all cross-cutting MediatR pipeline behaviors in one place
- Make `PipelineRegistration.AddPipeline()` the single source of truth for behavior order
- Ensure invalid input, duplicate external creates, stale concurrency checks, and unit-of-work commit run in the correct sequence

# Core Principles
- `PipelineRegistration.cs` is the single source of truth for pipeline behavior order
- Behaviors are registered in execution order — first registered runs first
- Pipeline order is: `ValidationBehavior` → `GuidResolvingBehavior` → `ConcurrencyBehavior` → `UnitOfWorkBehavior`
- Each behavior rejects invalid commands as early as possible so later behaviors never run unnecessarily
- Pipeline behaviors are registered centrally in App.Host — never inside a module's registration method

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration.solution.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - provides the centralized `PipelineRegistration.AddPipeline()` extension point
- [[skills/dotnet/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior.solution.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - provides `ValidationBehavior` that must run first
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity.solution.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - provides `GuidResolvingBehavior` that runs after validation and before concurrency
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change.solution.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - provides `ConcurrencyBehavior` that runs after idempotency and before unit of work
- [[skills/dotnet/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work.solution.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/unit-of-work.solution.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - provides `UnitOfWorkBehavior` that must run last

NUGET:
- None — relies only on packages already required by dependency solutions.

# Template Skill Mutations

PROJECT:
- [[./Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Add the complete ordered behavior registrations to centralized `PipelineRegistration.AddPipeline()`
  - [[./Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs]] - extend - Register all pipeline behaviors in execution order

# Rules

MUST:
- `PipelineRegistration.cs` defined in `App.Host/DependencyInjection/PipelineRegistration.cs`
- `AddPipeline()` called once from `Program.cs`
- All behaviors registered inside `AddPipeline()` using `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior<,>))`
- Behaviors registered in this exact execution order:
  1. `ValidationBehavior`
  2. `GuidResolvingBehavior`
  3. `ConcurrencyBehavior`
  4. `UnitOfWorkBehavior`
- Pipeline behaviors registered in App.Host — never inside a module's registration method

MUST NOT:
- Register behaviors inside module registration methods
- Change pipeline order in multiple files
- Create multiple pipeline registration extension methods
- Register behaviors directly in `Program.cs`

SHOULD:
- Keep `AddPipeline()` the only method that adds `IPipelineBehavior<,>` registrations
- Document the ordering with inline comments in `AddPipeline()`

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

# Unittest TestCases
- [ ] When `AddPipeline()` is called THEN all four behaviors are registered as `IPipelineBehavior<,>`
- [ ] When invalid command is sent THEN `ValidationBehavior` rejects before any other behavior runs
- [ ] When duplicate external-created Guid is sent THEN `GuidResolvingBehavior` rejects before `ConcurrencyBehavior` runs
- [ ] When stale version is sent THEN `ConcurrencyBehavior` rejects before `UnitOfWorkBehavior` runs
- [ ] When valid command is sent THEN `UnitOfWorkBehavior` commits after handler completes
- [ ] When nested sub-command is dispatched THEN `UnitOfWorkBehavior` commits only once at the outermost level
