---
name: class-validation-behavior
description: Class ValidationBehavior in the service-with-api plateau
whenToUse: when reviewing or changing how any MediatR request gets validated before its handler runs
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/class
  - plateau/service-with-api
created_by:
  - "[[../../../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]]"
---

# Goal
- Intercept every `IRequest<TResponse>` before the handler runs
- Collect all validation errors from all registered validators for the request, then short-circuit with `Result.Invalid(errors)`
- Pass through to the handler when no validators are registered or all pass

__Applied solutions:__
- [[../../../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]] - [[../../../../../solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs.create]]

# Core Principles
- Generic — one implementation handles all commands and queries across all modules
- Receives `IEnumerable<IValidator<TRequest>>` via DI — zero, one, or multiple validators supported
- Constrained to `where TRequest : IRequest<TResponse>` and `where TResponse : IResult`
- Registered first in `PipelineRegistration.AddPipeline()`, right after `ExceptionHandlingBehavior`

__Applied solutions:__
- [[../../../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]] - [[../../../../../solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Validation pipeline behavior | ValidationBehavior<TRequest, TResponse> | ValidationBehavior<CreateTaskCommand, Result<CreateTaskResult>> | ValidationBehavior.cs | ValidationBehavior.cs |

# Implementation
```csharp
//Skill: class-validation-behavior
//Plateau: service-with-api
//Version: 20260825120000

public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validators.Any())
            return await next();

        var errors = _validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
            .ToList();

        if (errors.Count > 0)
            return (TResponse)Result.Invalid(errors);

        return await next();
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]] - [[../../../../../solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs.create]]

# Rules
MUST:
- Collect all errors from all validators before returning — full error set, not first-error-only
- Return `Result.Invalid(errors)` on failure — never throw
- Pass through when no validators registered
- Be registered right after `ExceptionHandlingBehavior` in `PipelineRegistration.AddPipeline()`
MUST NOT:
- Contain any request-specific conditions
- Be registered inside a module registration method

__Applied solutions:__
- [[../../../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]] - [[../../../../../solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs.create]]

# Check list
- [ ] `ValidationBehavior` lives in `BuildingBlocks/MediatR/ValidationBehavior.cs`
- [ ] Registered right after `ExceptionHandlingBehavior` in `AddPipeline()`
- [ ] Collects all errors, returns `Result.Invalid`, never throws

__Applied solutions:__
- [[../../../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]] - [[../../../../../solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs.create]]
