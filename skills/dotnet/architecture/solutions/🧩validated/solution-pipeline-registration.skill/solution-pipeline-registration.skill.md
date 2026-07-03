---
name: solution-pipeline-registration
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
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
---

# Goal
- Define the centralized `PipelineRegistration` class in App.Host as the single place where all MediatR pipeline behaviors are registered
- Ensure `AddPipeline()` is called from `Program.cs` so the pipeline is wired into the composition root
- Establish a clear extension point: each behavior solution extends `AddPipeline()` to insert its behavior in the correct order

# Capabilities
- Centralized registration point for all MediatR pipeline behaviors
- Single extension method called from the composition root
- Clear extension point for new cross-cutting behaviors
- Prevention of scattered behavior registrations
- Consistent pipeline setup across modules

# Core Principles
- `PipelineRegistration.cs` is the single source of truth for pipeline behavior order
- Behaviors registered in execution order — first registered runs first
- New behaviors are added by extending `AddPipeline()`, never by creating a second registration file
- `AddPipeline()` is called once from `Program.cs`

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Host.csproj.create|App.Host.csproj]] - hosts `PipelineRegistration` and `Program.cs` composition root

NUGET:
- `MediatR` {version} - required by behaviors that extend `AddPipeline()` to register `IPipelineBehavior<,>`

# Template Skill Mutations

PROJECT:
- [[./Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Wire centralized pipeline registration in the composition root
  - [[./Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md|PipelineRegistration.cs]] - create - Centralized pipeline behavior registration extension

# Rules

## MUST:
- [[./Implementation/App.Host.csproj.extend.md#MUST|App.Host.csproj.extend]]
	- [[./Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md#MUST|PipelineRegistration.cs.create]]
- `AddPipeline()` called once from `Program.cs`
- All behaviors registered inside `AddPipeline()` using `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior<,>))`
- Behaviors registered in intended execution order

## SHOULD:
- Keep `AddPipeline()` the only method that adds `IPipelineBehavior<,>` registrations

## MUST NOT:
- [[./Implementation/App.Host.csproj.extend.md#MUST NOT|App.Host.csproj.extend]]
	- [[./Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create.md#MUST NOT|PipelineRegistration.cs.create]]
- Register behaviors inside module registration methods
- Change pipeline order in multiple files
- Create multiple pipeline registration extension methods

# Anti-patterns
- Pipeline order scattered across multiple files
- `AddPipeline()` duplicated or replaced by module-specific registration methods
- Registering behaviors directly in `Program.cs` instead of inside `PipelineRegistration`

# Check list
- [ ] `PipelineRegistration.cs` exists under `App.Host/DependencyInjection`
- [ ] `AddPipeline()` extension method defined in `PipelineRegistration.cs`
- [ ] `AddPipeline()` called from `Program.cs`
- [ ] No behavior registrations outside `PipelineRegistration.cs`
