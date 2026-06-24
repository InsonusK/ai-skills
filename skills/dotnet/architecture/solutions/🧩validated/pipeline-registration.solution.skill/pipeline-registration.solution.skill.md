---
uid: 266689e4-12cc-42e9-a9ea-4aa32271e1ee
name: pipeline-registration.solution
description: Defines the centralized MediatR pipeline registration in App.Host — the single AddPipeline extension method where all cross-cutting pipeline behaviors are registered in execution order
domain: skill
type: architecture
version: 20260612
tags:
  - skill/architecture/solution
  - dotnet
  - application
  - mediatr
  - pipeline
triggers:
  - register pipeline behaviors
  - add pipeline registration
  - mediatr pipeline
  - pipeline registration
creates:
  - App.Host.DependencyInjection.PipelineRegistration.cs
extends:
  - App.Host.csproj
depends_on:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]"
---

# Goal
- Define the centralized `PipelineRegistration` class in App.Host as the single place where all MediatR pipeline behaviors are registered
- Ensure `AddPipeline()` is called from `Program.cs` so the pipeline is wired into the composition root
- Establish a clear extension point: each behavior solution extends `AddPipeline()` to insert its behavior in the correct order

# Core Principles
- `PipelineRegistration.cs` is the single source of truth for pipeline behavior order
- Behaviors registered in execution order — first registered runs first
- New behaviors are added by extending `AddPipeline()`, never by creating a second registration file
- `AddPipeline()` is called once from `Program.cs`

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj]] - hosts `PipelineRegistration` and `Program.cs` composition root

NUGET:
- `MediatR` {version} - required by behaviors that extend `AddPipeline()` to register `IPipelineBehavior<,>`

# Template Skill Mutations

PROJECT:
- [[./Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Wire centralized pipeline registration in the composition root
  - [[./Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md|PipelineRegistration.cs]] - create - Centralized pipeline behavior registration extension

# Rules

MUST:
- `PipelineRegistration.cs` defined in `App.Host/DependencyInjection/PipelineRegistration.cs`
- `AddPipeline()` called once from `Program.cs`
- All behaviors registered inside `AddPipeline()` using `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior<,>))`
- Behaviors registered in intended execution order
- Pipeline behaviors registered in App.Host — never inside a module's registration method

MUST NOT:
- Register behaviors inside module registration methods
- Change pipeline order in multiple files
- Create multiple pipeline registration extension methods

SHOULD:
- Keep `AddPipeline()` the only method that adds `IPipelineBehavior<,>` registrations

# Anti-patterns
- Pipeline order scattered across multiple files
- `AddPipeline()` duplicated or replaced by module-specific registration methods
- Registering behaviors directly in `Program.cs` instead of inside `PipelineRegistration`

# Check list
- [ ] `PipelineRegistration.cs` exists under `App.Host/DependencyInjection`
- [ ] `AddPipeline()` extension method defined in `PipelineRegistration.cs`
- [ ] `AddPipeline()` called from `Program.cs`
- [ ] No behavior registrations outside `PipelineRegistration.cs`

# Unittest TestCases
- [ ] WHEN applied THEN `PipelineRegistration.cs` exists under `App.Host/DependencyInjection`
- [ ] WHEN applied THEN `AddPipeline()` is called from `Program.cs`
- [ ] WHEN applied THEN `AddPipeline()` returns `IServiceCollection`
- [ ] WHEN extended THEN behaviors can be added inside `AddPipeline()` in execution order
