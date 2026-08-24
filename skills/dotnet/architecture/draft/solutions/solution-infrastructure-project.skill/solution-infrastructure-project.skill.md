---
name: solution-infrastructure-project
description: Creates the empty App.Infrastructure project — the single home for outbound infrastructure integrations (persistence, cache, and other external systems) — and wires it into the composition root via a dedicated AddInfrastructure() registration extension, without assuming any specific infrastructure concern itself
whenToUse: when a service needs its first outbound infrastructure integration (a database, a cache, a message broker) and App.Infrastructure does not exist yet, or when reviewing where a new infrastructure integration's project-level home and App.Host wiring should live
domain: skill
type: architecture
version: 20260824090000
tags:
  - skill/architecture/solution
  - stack/dotnet
  - infrastructure
  - application
  - concern/architecture
  - solution/infrastructure-project

creates:
  - App.Infrastructure.csproj
  - App.Host.DependencyInjection.InfrastructureRegistration.cs
extends:
  - App.Host.csproj
depends_on:
built_on_plateau: "[[skills/dotnet/architecture/draft/plateau/plateau-stateless-non-interactive-service/plateau-stateless-non-interactive-service.skill/plateau-stateless-non-interactive-service.skill.md|plateau-stateless-non-interactive-service]]"
---

# Goal
- Create `App.Infrastructure` as the single project every outbound infrastructure integration (persistence, cache, message broker, ...) extends — never a project any single integration solution creates or redefines for itself
- Give App.Host a dedicated `AddInfrastructure()` extension point, parallel to `AddModules()`/`AddPipeline()`, that infrastructure solutions extend rather than each wiring their own registration path

# Capabilities
- One project any future outbound-integration solution (persistence, cache, ...) can extend without re-deciding where it lives or how it's wired into the composition root
- A single, centralized `AddInfrastructure()` call site in `Program.cs` — no scattered per-integration registration entry points

# Core Principles
- `App.Infrastructure` is the only project that knows the implementation details of any outbound integration — no module `Application`/`Domain` ever references it directly, only through abstractions in `Shared`
- This solution is deliberately empty of any concern — it creates the project and the registration extension point, nothing else. A specific integration (persistence, cache, ...) is added by that integration's own solution extending this project, never by broadening what this solution itself defines
- `AddInfrastructure()` lives in `App.Host/DependencyInjection/InfrastructureRegistration.cs`, called once from `Program.cs` alongside `AddModules()`/`AddPipeline()`
- Reused as-is by every future outbound-integration solution — a solution that needs `App.Infrastructure` to exist depends on this solution, it never creates the project itself

# Boundaries
- Which outbound systems (a database, a cache, a message broker) actually get integrated, and what code that takes, is not this solution's concern — see whichever concern-specific solution extends `App.Infrastructure` (e.g. `solution-repository-integration` for persistence).

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/draft/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/draft/solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create|App.Host.csproj]] - hosts the composition root `InfrastructureRegistration.cs` extends

NUGET:
- None — relies only on patterns defined by dependency solutions.

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/draft/solutions/solution-infrastructure-project.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]] - create - Empty project, the single home for future outbound infrastructure integrations
- [[skills/dotnet/architecture/draft/solutions/solution-infrastructure-project.skill/Implementation/App.Host.csproj.extend|App.Host.csproj]] - extend - Add `InfrastructureRegistration.cs` and wire `AddInfrastructure()` into `Program.cs`

# Rules

## MUST
- [[skills/dotnet/architecture/draft/solutions/solution-infrastructure-project.skill/Implementation/App.Infrastructure.csproj.create#MUST|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/draft/solutions/solution-infrastructure-project.skill/Implementation/App.Host.csproj.extend#MUST|App.Host.csproj]]

## MUST NOT
- [[skills/dotnet/architecture/draft/solutions/solution-infrastructure-project.skill/Implementation/App.Infrastructure.csproj.create#MUST NOT|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/draft/solutions/solution-infrastructure-project.skill/Implementation/App.Host.csproj.extend#MUST NOT|App.Host.csproj]]
- A solution that needs `App.Infrastructure` create the project itself instead of depending on this solution.
  - Risk: two solutions independently creating/redefining `App.Infrastructure.csproj` produces conflicting project files the moment both are applied to the same service.
  - Fix: every solution that extends `App.Infrastructure` declares `depends_on: solution-infrastructure-project` and only ever extends the project this solution created.

# Check list
- [ ] `App.Infrastructure.csproj` exists, empty of any concern-specific code
- [ ] `InfrastructureRegistration.cs` exists under `App.Host/DependencyInjection`, exposing `AddInfrastructure()`
- [ ] `AddInfrastructure()` called once from `Program.cs`, alongside `AddModules()`/`AddPipeline()`
- [ ] No concern-specific code (persistence, cache, ...) added by this solution itself
