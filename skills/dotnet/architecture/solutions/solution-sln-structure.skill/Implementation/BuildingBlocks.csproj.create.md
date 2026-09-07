---
description: Create the BuildingBlocks project — the home for reusable technical patterns (MediatR behaviors), consuming contracts from Shared
name: BuildingBlocks.csproj
element_kind: project
change_kind: create
tags:
  - solution/sln-structure
  - element/buildingblocks-csproj
---

# Goals
- Give the family one project for reusable technical-pattern implementations (MediatR pipeline behaviors first; persistence/outbox/concurrency helpers later, from their own solutions).
- Keep it referencing only `Shared`.

# Core Principles
- `BuildingBlocks` implements patterns; it never *defines* a cross-cutting contract — those live in `Shared`.
- `BuildingBlocks` references only `Shared`.
- At the v3.1 baseline it is an almost-empty project with a `/MediatR` folder; `ValidationBehavior` and `ExceptionHandlingBehavior` are added by their own solutions, not here.

# Structure

## Project Structure
```
/src/BuildingBlocks
  /MediatR            — pipeline behavior implementations (added by their solutions)
  BuildingBlocks.csproj
```
Later solutions add: `ValidationBehavior.cs` (solution-validation-behavior), `ExceptionHandlingBehavior.cs` (solution-mediator-exception-handler), `UnitOfWorkBehavior.cs` (VP2), `ConcurrencyBehavior.cs` (VP5), `GuidResolvingBehavior.cs` (VP6), `/Outbox` (VP14).

## Directory and class skills
| `Directory\|file` | Description |
| ----------------- | ----------- |
| /MediatR | Pipeline behavior implementations, added by their owning solutions |
| BuildingBlocks.csproj | Leaf technical-patterns project, references only `Shared` |

# NuGet Packages
| Package | Version constraint | Purpose |
| ------- | ------------------ | ------- |
| MediatR | central | `IPipelineBehavior<,>` base for behaviors added later |

# What Does NOT Belong Here
- Business logic, entities — `{Module}.Domain`.
- Module-specific handlers/validators — `{Module}.Application`.
- Contract definitions — `Shared`.

# Allowed Dependencies
- `Shared`
- `MediatR` (NuGet)

# Rules

## MUST
- Reference only `Shared`.
  - Risk: a reference to a module or infrastructure project inverts the dependency direction and couples the pattern layer to a concrete consumer.
  - Fix: `BuildingBlocks -> Shared` only.
- Implement patterns here; never define a cross-cutting interface here.
  - Risk: an interface defined in `BuildingBlocks` forces every consumer to reference the pattern layer to see a contract.
  - Fix: contracts in `Shared`, implementations here.
- Never put business logic or a module-specific handler/validator in `BuildingBlocks`.
  - Risk: logic that belongs to one module leaks into a layer shared by all.
  - Fix: module logic stays in `{Module}.Application` / `{Module}.Domain`.

# Check list
- [ ] `BuildingBlocks.csproj` references only `Shared` (+ `MediatR` NuGet, versionless).
- [ ] No contract definitions in `BuildingBlocks`.
- [ ] `/MediatR` behaviors present only if their owning solution has been applied.
