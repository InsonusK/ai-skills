---
uid: 79b2fb8b-35f2-4930-be5d-6e76eb79af28
name: validationbehavior-class
description: Pipeline behavior that validates any MediatR request
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior.solution.skill]]"
---

# Goal
- Intercept every `IRequest<TResponse>` before the handler runs
- Collect all validation errors from all registered validators for the request
- Short-circuit with `Result.Invalid(errors)` if any errors exist — handler never runs for invalid input
- Pass through to the handler if no validators are registered or all validators pass

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs.create]]

# Core Principals
- Receives `IEnumerable<IValidator<TRequest>>` via DI — zero, one, or multiple validators supported
- Runs all validators and collects all errors before short-circuiting — full error list, not fail-fast per field
- Maps FluentValidation `ValidationFailure` to `Ardalis.Result` `ValidationError`
- Constrained to `where TRequest : IRequest<TResponse>` and `where TResponse : IResult` — activates on any MediatR request that returns a Result, including commands and queries

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Validation pipeline behavior | `ValidationBehavior<TRequest, TResponse>` | `ValidationBehavior<CreateTaskCommand, Result<CreateTaskResult>>` | `ValidationBehavior.cs` | `ValidationBehavior.cs` |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs.create]]

# Implementation
```csharp
// BuildingBlocks/MediatR/ValidationBehavior.cs
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace BuildingBlocks.MediatR;

public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any())
            return await next();

        var errors = _validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Where(e => e is not null)
            .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
            .ToList();

        if (errors.Count > 0)
            return (TResponse)Result.Invalid(errors);

        return await next();
    }
}
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs.create]]

# Rules
MUST:
	- Constrained to `where TRequest : IRequest<TResponse>` and `where TResponse : IResult`
	- Collect all errors from all validators before returning — full error set, not first-error-only
	- Return `Result.Invalid(errors)` on failure — not throw an exception
	- Pass through when no validators registered — missing validator is not a fault
MUST NOT:
	- Contain any request-specific conditions
	- Throw `ValidationException` — always return typed `Result.Invalid`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Intercept every IRequest<TResponse> before the handler runs
- [ ] WHEN applied THEN Collect all validation errors from all registered validators for the request
- [ ] WHEN applied THEN Short-circuit with Result.Invalid(errors) if any errors exist — handler never runs for invalid input
- [ ] WHEN applied THEN Pass through to the handler if no validators are registered or all validators pass
- [ ] WHEN applied THEN Receives IEnumerable<IValidator<TRequest>> via DI — zero, one, or multiple validators supported
- [ ] WHEN applied THEN Runs all validators and collects all errors before short-circuiting — full error list, not fail-fast per field
- [ ] WHEN applied THEN Maps FluentValidation ValidationFailure to Ardalis.Result ValidationError
- [ ] WHEN applied THEN Constrained to where TRequest : IRequest<TResponse> and where TResponse : IResult — activates on any MediatR request that returns a Result, including commands and queries
- [ ] WHEN naming 'Validation pipeline behavior' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs.create]]
