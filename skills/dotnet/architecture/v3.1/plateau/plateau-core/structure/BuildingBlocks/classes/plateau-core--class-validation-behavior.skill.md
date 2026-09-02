---
name: plateau-core--class-validation-behavior
description: Class ValidationBehavior<TRequest, TResponse> in the plateau-core plateau — the MediatR pipeline behavior that collects every validator's failures and short-circuits with Result.Invalid before the handler
whenToUse: when creating or editing ValidationBehavior, or changing how transport-validation failures short-circuit the pipeline
domain: skill
type: template
plateau: core
version: 20260902000000
tags:
  - skill/template/class
  - plateau/core
created_by:
  - "[[../../../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]]"
---

# Goal
- Intercept every MediatR request that returns a `Result`, run every registered `IValidator<TRequest>`, collect all failures, and short-circuit with `Result.Invalid(errors)` before the handler runs.
- Pass straight through when no validator is registered or all pass.

__Applied solutions:__
- [[../../../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]] - [[../../../../../solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- Generic and content-free: `where TRequest : notnull`, `where TResponse : IResult`. No request-specific branching.
- Collect-all, not fail-fast — every validator runs, every failure is reported in one `Result.Invalid`.
- Maps a FluentValidation `ValidationFailure` to an Ardalis `ValidationError` (property, message, error code, `Error` severity).
- Builds the short-circuit value by invoking the closed `TResponse`'s own static `Invalid(IEnumerable<ValidationError>)` via reflection — a plain `(TResponse)Result.Invalid(...)` cast throws for `Result<T>`.
- Registered second in the pipeline (right after `ExceptionHandlingBehavior`), before any behavior that assumes a validated request.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Validation pipeline behavior | `ValidationBehavior<TRequest, TResponse>` | `ValidationBehavior<CreateTaskCommand, Result<CreateTaskResult>>` | `ValidationBehavior.cs` | `ValidationBehavior.cs` |

# Implementation
```csharp
// Skill: plateau-core--class-validation-behavior
// Plateau: core
// Version: 20260902000000
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace BuildingBlocks.MediatR;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResult
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var failures = (await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, ct))))
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count != 0)
            {
                var errors = failures.Select(f =>
                    new ValidationError(f.PropertyName, f.ErrorMessage, f.ErrorCode, ValidationSeverity.Error));
                return (TResponse)MakeInvalid(typeof(TResponse), errors);
            }
        }

        return await next();
    }

    private static object MakeInvalid(Type responseType, IEnumerable<ValidationError> errors)
    {
        var invalid = responseType.GetMethod("Invalid", [typeof(IEnumerable<ValidationError>)])!;
        return invalid.Invoke(null, [errors])!;
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]] - [[../../../../../solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs.create]]

# Rules
MUST:
- Constrain to `where TRequest : notnull` and `where TResponse : IResult`.
- Run every validator and collect every failure before short-circuiting; return `Result.Invalid`, never throw `ValidationException`.
- Pass through when no validator is registered.
- Build the invalid result through the closed `TResponse`'s own static `Invalid` (reflection) — never `(TResponse)Result.Invalid(...)`.
- Live in `BuildingBlocks/MediatR/ValidationBehavior.cs`; be registered only in `PipelineRegistration.AddPipeline()`.
- Never apply several plateau templates per class.
- Never add a request-specific condition or a business rule.

# Check list
- [ ] `where TRequest : notnull`, `where TResponse : IResult`.
- [ ] All validators run; all failures collected into one `Result.Invalid`.
- [ ] A `Result<T>` request short-circuits to a `Result<T>` (no `InvalidCastException`).
- [ ] Passes through with zero validators.

# Unittest TestCases
- [ ] WHEN a validator rejects the request THEN the handler is not invoked and the result status is `Invalid`.
- [ ] WHEN two validators both fail THEN the result carries both failures.
- [ ] WHEN no validator is registered THEN the handler runs unchanged.
- [ ] WHEN the request type is `Result<T>` and invalid THEN the returned value is a `Result<T>`.
