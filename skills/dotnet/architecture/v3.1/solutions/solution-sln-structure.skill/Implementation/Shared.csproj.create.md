---
description: Create the Shared project — cross-cutting contracts and Result primitives every layer may depend on
name: Shared.csproj
element_kind: project
change_kind: create
tags:
  - solution/sln-structure
  - element/shared-csproj
---

# Goals
- Give every layer one project it may reference for cross-cutting contracts and the `Result` primitive, with no coupling.
- Keep it a leaf: `Shared` references nothing else in the solution.

# Core Principles
- `Shared` holds interfaces, marker types, and small value primitives — no implementations, no behaviors, no entities.
- Any project at any layer may reference `Shared`; `Shared` references nothing.
- Later solutions add their own folders here (`/MediatR` markers from `solution-mediator-integration`, `/Exceptions` from `solution-domain-behaviour`, concurrency/outbox contracts from VP2/VP14) — this solution creates the empty project and the `Result` reference only.

# Structure

## Project Structure
```
/src/Shared
  Shared.csproj
```
At the v3.1 baseline `Shared` is an almost-empty project. Folders appear as their owning solution is applied:
- `/MediatR` — `ICommand.cs`, `IQuery.cs`, `INotificationEvent.cs` (solution-mediator-integration)
- `/Exceptions` — `DomainException.cs` (solution-domain-behaviour), `EntityNotLoadedException.cs` (solution-domain-rules)
- `/Repositories`, `/UnitOfWork`, `/Concurrency`, `/Timestamps` (VP2 / VP5 / VP7 solutions)

## Directory and class skills
| `Directory\|file` | Description |
| ----------------- | ----------- |
| Shared.csproj | Empty leaf project; contract folders added by later solutions |

# NuGet Packages
| Package | Version constraint | Purpose |
| ------- | ------------------ | ------- |
| Ardalis.Result | central | `Result` / `Result<T>` — the outcome type every handler returns |

Version lives in `Directory.Packages.props` per [[skills/dotnet/architecture/v3.1/solutions/solution-central-package-management.skill/solution-central-package-management.skill.md|solution-central-package-management]].

# What Does NOT Belong Here
- Business logic, domain rules, entities — `{Module}.Domain`.
- Pipeline behaviors — `BuildingBlocks`.
- Any implementation — `BuildingBlocks` or `App.Infrastructure`.

# Allowed Dependencies
- `Ardalis.Result` (NuGet).
- No project references.

# Rules

## MUST
- Keep `Shared` with zero project references.
  - Risk: one project reference from `Shared` makes it non-leaf, and every consumer transitively inherits that dependency.
  - Fix: `Shared` depends only on the BCL and `Ardalis.Result`.
- Put only interfaces, marker types, and small value primitives in `Shared` — never an implementation or a behavior.
  - Risk: an implementation in `Shared` is referenced by every layer and cannot be swapped or tested in isolation.
  - Fix: contracts here; implementations in `BuildingBlocks` / `App.Infrastructure`.
- Never reference a module, `BuildingBlocks`, or an infrastructure project from `Shared`.
  - Risk: a cycle, or an inverted dependency direction.
  - Fix: dependencies flow toward `Shared`, never out of it.

# Check list
- [ ] `Shared.csproj` has no project references.
- [ ] `Shared.csproj` references only `Ardalis.Result` (versionless).
- [ ] No implementation, behavior, or entity in any `Shared` type.
- [ ] Contract folders (`/MediatR`, `/Exceptions`, …) exist only if the solution that owns them has been applied.
