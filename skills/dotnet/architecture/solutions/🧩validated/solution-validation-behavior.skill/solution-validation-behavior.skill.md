---
name: solution-validation-behavior
description: Defines the cross-cutting validation pipeline behavior — ValidationBehavior in BuildingBlocks intercepts any MediatR IRequest<TResponse>, collects all validation errors from registered FluentValidation validators, and short-circuits with Result.Invalid before the handler runs
domain: skill
type: architecture
version: 20260611
tags:
  - skill/architecture/solution
  - dotnet
  - application
  - validation
  - fluent-validation
  - pipeline
  - mediatr
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
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration.skill]]"
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
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create|BuildingBlocks.csproj]] - hosts `ValidationBehavior`
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - indirectly referenced because handlers return `Result<T>` via Ardalis patterns
- [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - provides centralized `PipelineRegistration` where `ValidationBehavior` is registered

NUGET:
- `FluentValidation` {version} - provides `IValidator<T>` injected into `ValidationBehavior`
- `MediatR` {version} - provides `IPipelineBehavior<TRequest, TResponse>` and `IRequest<T>` marker
- `Ardalis.Result` {version} - provides `Result.Invalid`, `ValidationError`, and `IResult` marker

# Template Skill Mutations

PROJECT:
- [[./Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - extend - Add FluentValidation, MediatR, and Ardalis.Result packages for the ValidationBehavior
  - [[./Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs]] - create - Pipeline behavior that validates any `IRequest<TResponse>`

# Rules

## MUST:
- [[./Implementation/BuildingBlocks.csproj.extend.md#MUST|BuildingBlocks.csproj.extend]]
	- [[./Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md#MUST|ValidationBehavior.cs.create]]
- Collect all errors from all validators before returning — full error set, not first-error-only
- Return `Result.Invalid(errors)` on failure — not throw an exception
- Pass through when no validators registered — missing validator is not a fault

- Pipeline behaviors registered via centralized `PipelineRegistration` in App.Host

## SHOULD:
- `Transient` lifetime — new behavior instance per pipeline invocation

## MUST NOT:
- [[./Implementation/BuildingBlocks.csproj.extend.md#MUST NOT|BuildingBlocks.csproj.extend]]
	- [[./Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md#MUST NOT|ValidationBehavior.cs.create]]
- Throw `ValidationException` — always return typed `Result.Invalid`
- Register behaviors inside module registration methods

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
