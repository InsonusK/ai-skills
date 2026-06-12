---
uid: 7c3a9b2e-5d1f-4e8a-9c3b-2d4e6f8a1b3c
name: validation-behavior
description: Defines the cross-cutting validation pipeline behavior — ValidationBehavior in BuildingBlocks intercepts any MediatR IRequest<TResponse>, collects all validation errors from registered FluentValidation validators, and short-circuits with Result.Invalid before the handler runs; includes App.Host pipeline wiring
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
  - App.Host.csproj
  - App.Host.DependencyInjection.PipelineRegistration.cs
depends_on:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration.solution.skill]]"
---

# Goal
- Define `ValidationBehavior` in BuildingBlocks as the single pipeline interception point that rejects invalid MediatR requests before the handler runs
- Register `ValidationBehavior` in App.Host pipeline so it executes first for any `IRequest<TResponse>` that returns `IResult`
- Establish that validation is a cross-cutting pipeline concern — one generic behavior handles all commands and queries across all modules

# Core Principles
- `ValidationBehavior` is generic — one implementation handles all commands and queries across all modules
- Receives `IEnumerable<IValidator<TRequest>>` via DI — zero, one, or multiple validators supported
- Runs all validators and collects all errors before short-circuiting — full error list, not fail-fast per field
- Maps FluentValidation `ValidationFailure` to `Ardalis.Result` `ValidationError`
- Constrained to `where TRequest : IRequest<TResponse>` and `where TResponse : IResult` — activates on any MediatR request that returns a Result, including commands and queries
- `ValidationBehavior` registered first in pipeline — before any other behavior
- Returns `Result.Invalid(errors)` on failure — not an exception; requires `TResponse` to implement `IResult`
- Passes through when no validators registered — missing validator is not a fault

# Requirements
SOLUTION:
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj]] - hosts `ValidationBehavior`
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj]] - hosts centralized pipeline registration
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/repository-integration.solution.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - indirectly referenced because handlers return `Result<T>` via Ardalis patterns
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration.solution.skill]]
  - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - provides `PipelineRegistration.AddPipeline()` where `ValidationBehavior` is registered first

NUGET:
- `FluentValidation` {version} - provides `IValidator<T>` injected into `ValidationBehavior`
- `MediatR` {version} - provides `IPipelineBehavior<TRequest, TResponse>` and `IRequest<T>` marker
- `Ardalis.Result` {version} - provides `Result.Invalid`, `ValidationError`, and `IResult` marker

# Template Skill Mutations

PROJECT:
- [[./Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - extend - Add FluentValidation, MediatR, and Ardalis.Result packages for the ValidationBehavior
  - [[./Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs]] - create - Pipeline behavior that validates any `IRequest<TResponse>`
- [[./Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Add ValidationBehavior to the centralized pipeline registration
  - [[./Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs]] - extend - Add ValidationBehavior as the first pipeline behavior

# Rules

MUST:
- `ValidationBehavior` defined in `BuildingBlocks/MediatR/ValidationBehavior.cs`
- `ValidationBehavior` constrained to `where TRequest : IRequest<TResponse>` and `where TResponse : IResult`
- Collect all errors from all validators before returning — full error set, not first-error-only
- Return `Result.Invalid(errors)` on failure — not throw an exception
- Pass through when no validators registered — missing validator is not a fault
- `ValidationBehavior` registered as the first `IPipelineBehavior<,>` entry
- All behaviors registered as `AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior<,>))`
- Behaviors registered in intended execution order
- Pipeline behaviors registered in App.Host — never inside a module's registration method

MUST NOT:
- Contain any command-specific conditions in `ValidationBehavior`
- Throw `ValidationException` — always return typed `Result.Invalid`
- Register behaviors inside module registration methods
- Change pipeline order in multiple files

SHOULD:
- Behaviors registered in execution order — first registered runs first
- `Transient` lifetime — new behavior instance per pipeline invocation

# Anti-patterns
- Implementing per-request validation logic inside `ValidationBehavior`
- Returning exceptions instead of `Result.Invalid`
- `ValidationBehavior` registered after other behaviors — invalid requests must be rejected before any side-effect behavior runs
- Business invariant in validator: `RuleFor(x => x.Status).Must(s => s != TaskStatus.Closed)` — belongs in domain entity
- `RuleFor(x => x.AssigneeId).MustAsync(async (id, ct) => await _repo.AnyAsync(...))` — entity existence is a handler guard, not transport validation

# Check list
- [ ] `ValidationBehavior` defined in `BuildingBlocks/MediatR/ValidationBehavior.cs`
- [ ] `ValidationBehavior` constrained to `where TRequest : IRequest<TResponse>` and `where TResponse : IResult`
- [ ] `ValidationBehavior` collects all errors — not fail-fast on first error
- [ ] `ValidationBehavior` returns `Result.Invalid(errors)` — not exception
- [ ] `ValidationBehavior` passes through when no validators registered
- [ ] `ValidationBehavior` registered first in App.Host pipeline
- [ ] Pipeline behaviors registered in App.Host — not in module registration

# Unittest TestCases
- [ ] When request with empty required field is sent Then `ValidationBehavior` returns `Result.Invalid` before handler runs
- [ ] When request with field exceeding max length is sent Then `Result.Invalid` returned with correct property name
- [ ] When request with multiple invalid fields is sent Then all field errors returned in single `Result.Invalid` — not just first
- [ ] When request with all valid fields is sent Then handler executes normally
- [ ] When request has no registered validator Then pipeline proceeds to handler without error
- [ ] When query is dispatched Then `ValidationBehavior` activates and validates query input
- [ ] When two validators registered for same request Then both validators run and errors are merged
