---
name: solution-validation-behavior
description: Defines the cross-cutting validation pipeline behavior — ValidationBehavior in BuildingBlocks intercepts any MediatR IRequest<TResponse>, collects all validation errors from registered FluentValidation validators, and short-circuits with Result.Invalid before the handler runs
domain: skill
type: architecture
version: 20260611
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

triggers:
  - add validation pipeline
  - implement validation behavior
  - transport validation
  - fluent validation pipeline
  - validate request input
  - define validator
  - validate mediatr request
creates:
  - BuildingBlocks.MediatR.ValidationBehavior.cs
extends:
  - BuildingBlocks.csproj
depends_on:
built_on_plateau: "[[skills/dotnet/architecture/draft/plateau/plateau-stateless-non-interactive-service/plateau-stateless-non-interactive-service.skill/plateau-stateless-non-interactive-service.skill.md|plateau-stateless-non-interactive-service]]"
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
- [[skills/dotnet/architecture/draft/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/draft/solutions/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create|BuildingBlocks.csproj]] - hosts `ValidationBehavior`
- [[skills/dotnet/architecture/draft/solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill|solution-pipeline-registration]]
  - [[skills/dotnet/architecture/draft/solutions/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend|App.Host.csproj]] - provides centralized `PipelineRegistration` where `ValidationBehavior` is registered

NUGET:
- `FluentValidation` {version} - provides `IValidator<T>` injected into `ValidationBehavior`
- `MediatR` {version} - provides `IPipelineBehavior<TRequest, TResponse>` and `IRequest<T>` marker
- `Ardalis.Result` {version} - provides `Result.Invalid`, `ValidationError`, and `IResult` marker

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/draft/solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend|BuildingBlocks.csproj]] - extend - Add FluentValidation, MediatR, and Ardalis.Result packages for the ValidationBehavior
  - [[skills/dotnet/architecture/draft/solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create|ValidationBehavior.cs]] - create - Pipeline behavior that validates any `IRequest<TResponse>`

# Rules

## MUST:
- [[skills/dotnet/architecture/draft/solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend#MUST|BuildingBlocks.csproj]]
	- [[skills/dotnet/architecture/draft/solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create#MUST|ValidationBehavior.cs]]

## MUST NOT:
- [[skills/dotnet/architecture/draft/solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend#MUST NOT|BuildingBlocks.csproj]]
	- [[skills/dotnet/architecture/draft/solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create#MUST NOT|ValidationBehavior.cs]]

# Anti-patterns
- Implementing per-request validation logic inside `ValidationBehavior`
- Returning exceptions instead of `Result.Invalid`
- Business invariant in validator: `RuleFor(x => x.Status).Must(s => s != TaskStatus.Closed)` — belongs in domain entity
- `RuleFor(x => x.AssigneeId).MustAsync(async (id, ct) => await _repo.AnyAsync(...))` — entity existence is a handler guard, not transport validation

# Check list
- [ ] `ValidationBehavior` defined in `BuildingBlocks/MediatR/ValidationBehavior.cs`
- [ ] `ValidationBehavior` constrained to `where TRequest : IRequest<TResponse>` and `where TResponse : IResult`
- [ ] `ValidationBehavior` collects all errors — not fail-fast on first error
- [ ] `ValidationBehavior` returns `Result.Invalid(errors)` — not exception
- [ ] `ValidationBehavior` passes through when no validators registered
