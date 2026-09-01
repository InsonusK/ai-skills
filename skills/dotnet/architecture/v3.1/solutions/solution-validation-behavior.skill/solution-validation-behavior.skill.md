---
name: solution-validation-behavior
description: Defines the cross-cutting validation pipeline behavior — ValidationBehavior in BuildingBlocks intercepts any MediatR IRequest<TResponse>, collects all validation errors from registered FluentValidation validators, and short-circuits with Result.Invalid before the handler runs
whenToUse: when adding cross-cutting transport validation to the MediatR pipeline — rejecting an invalid command or query with Result.Invalid before its handler runs
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - stack/dotnet
  - application
  - validation
  - framework/fluent-validation
  - pipeline
  - framework/mediatr
  - concern/architecture
  - solution/validation-behavior

creates:
  - BuildingBlocks.MediatR.ValidationBehavior.cs
extends:
  - BuildingBlocks.csproj
  - App.Host.csproj
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]]"
built_on_plateau:
---

# Goal
- Define `ValidationBehavior` in BuildingBlocks as the single pipeline interception point that rejects invalid MediatR requests before the handler runs
- Establish that validation is a cross-cutting pipeline concern — one generic behavior handles all commands and queries across all modules

# Capabilities
- Cross-cutting transport validation for all MediatR requests
- Full error collection before short-circuiting
- Consistent `Result.Invalid` response without exceptions
- Zero per-request validation boilerplate
- Works for both commands and queries

# Core Principles
- `ValidationBehavior` is generic — one implementation handles all commands and queries across all modules
- Receives `IEnumerable<IValidator<TRequest>>` via DI — zero, one, or multiple validators supported
- Runs all validators and collects all errors before short-circuiting — full error list, not fail-fast per field
- Maps FluentValidation `ValidationFailure` to `Ardalis.Result` `ValidationError`
- Constrained to `where TRequest : IRequest<TResponse>` and `where TResponse : IResult` — activates on any MediatR request that returns a Result, including commands and queries
- Returns `Result.Invalid(errors)` on failure — not an exception; requires `TResponse` to implement `IResult`
- Passes through when no validators registered — missing validator is not a fault

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj]] - hosts `ValidationBehavior`
- [[skills/dotnet/architecture/v3.1/solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - provides centralized `PipelineRegistration` where `ValidationBehavior` is registered

NUGET:
- `FluentValidation` {version} - provides `IValidator<T>` injected into `ValidationBehavior`
- `MediatR` {version} - provides `IPipelineBehavior<TRequest, TResponse>` and `IRequest<T>` marker
- `Ardalis.Result` {version} - provides `Result.Invalid`, `ValidationError`, and `IResult` marker

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - extend - Add FluentValidation, MediatR, and Ardalis.Result packages for the ValidationBehavior
  - [[skills/dotnet/architecture/v3.1/solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs]] - create - Pipeline behavior that validates any `IRequest<TResponse>`
- [[skills/dotnet/architecture/v3.1/solutions/solution-validation-behavior.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Register `ValidationBehavior` in the centralized pipeline
  - [[skills/dotnet/architecture/v3.1/solutions/solution-validation-behavior.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs]] - extend - Insert `ValidationBehavior` right after `ExceptionHandlingBehavior`

# Rule

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend.md#MUST|BuildingBlocks.csproj]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md#MUST|ValidationBehavior.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-validation-behavior.skill/Implementation/App.Host.csproj.extend.md#MUST|App.Host.csproj]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-validation-behavior.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md#MUST|PipelineRegistration.cs]]
- Keep `ValidationBehavior` a generic dispatcher only — it runs registered validators and never contains a per-request condition.
  - Risk: a condition inlined into the behavior applies to every request type and cannot be found by anyone reading a specific command's validator.
  - Fix: the behavior resolves `IEnumerable<IValidator<TRequest>>` and runs them; conditions live in per-request `AbstractValidator<T>` classes.
- Short-circuit with `Result.Invalid(errors)`, never a thrown exception.
  - Risk: a thrown validation exception turns a 400-shaped outcome into a 500 and bypasses the `Result` contract every handler returns.
  - Fix: collect failures and return `Result.Invalid`.
- Never put a business invariant or an entity-existence check in a validator run by this behavior.
  - Risk: `RuleFor(x => x.Status).Must(...)` or `MustAsync(... _repo.AnyAsync ...)` mixes domain rules and DB access into transport validation, so the rule runs before the handler's own guards and duplicates domain logic.
  - Fix: transport shape only in validators; invariants in the entity, existence checks in the handler.

# Check list
- [ ] `ValidationBehavior` defined in `BuildingBlocks/MediatR/ValidationBehavior.cs`
- [ ] `ValidationBehavior` constrained to `where TRequest : IRequest<TResponse>` and `where TResponse : IResult`
- [ ] `ValidationBehavior` collects all errors — not fail-fast on first error
- [ ] `ValidationBehavior` returns `Result.Invalid(errors)` — not exception
- [ ] `ValidationBehavior` passes through when no validators registered
