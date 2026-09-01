---
name: solution-pipeline-registration
description: Defines the centralized MediatR pipeline registration in App.Host — the single AddPipeline extension method where all cross-cutting pipeline behaviors are registered in execution order
whenToUse: when registering a new MediatR pipeline behavior, or deciding where cross-cutting pipeline behaviors are centrally wired in App.Host
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - stack/dotnet
  - application
  - framework/mediatr
  - pipeline
  - concern/architecture
  - solution/pipeline-registration

creates:
  - App.Host.DependencyInjection.PipelineRegistration.cs
extends:
  - App.Host.csproj
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
built_on_plateau:
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
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create|App.Host.csproj]] - hosts `PipelineRegistration` and `Program.cs` composition root

NUGET:
- `MediatR` {version} - required by behaviors that extend `AddPipeline()` to register `IPipelineBehavior<,>`

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend|App.Host.csproj]] - extend - Wire centralized pipeline registration in the composition root
  - [[skills/dotnet/architecture/v3.1/solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create|PipelineRegistration.cs]] - create - Centralized pipeline behavior registration extension

# Rules

## MUST:
- [[skills/dotnet/architecture/v3.1/solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend#MUST|App.Host.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create#MUST|PipelineRegistration.cs]]
- `AddPipeline()` called once from `Program.cs`

## MUST NOT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend#MUST NOT|App.Host.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.create#MUST NOT|PipelineRegistration.cs]]
- Change pipeline order in multiple files

# Anti-patterns
- Pipeline order scattered across multiple files
- `AddPipeline()` duplicated or replaced by module-specific registration methods
- Registering behaviors directly in `Program.cs` instead of inside `PipelineRegistration`

# Check list
- [ ] `PipelineRegistration.cs` exists under `App.Host/DependencyInjection`
- [ ] `AddPipeline()` extension method defined in `PipelineRegistration.cs`
- [ ] `AddPipeline()` called from `Program.cs`
- [ ] No behavior registrations outside `PipelineRegistration.cs`
