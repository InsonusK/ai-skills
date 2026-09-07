---
description: Pipeline behavior that validates any MediatR request
project_name: BuildingBlocks
name: ValidationBehavior.cs
element_kind: class
change_kind: create
tags:
  - solution/validation-behavior
  - element/validationbehavior-cs
---

# Goals
- Intercept every `IRequest<TResponse>` before the handler runs
- Collect all validation errors from all registered validators for the request
- Short-circuit with `Result.Invalid(errors)` if any errors exist — handler never runs for invalid input
- Pass through to the handler if no validators are registered or all validators pass

# Structure

## Project Structure
```
/BuildingBlocks
  /MediatR
    ValidationBehavior.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Validation pipeline behavior | `ValidationBehavior<TRequest, TResponse>` | `ValidationBehavior<CreateTaskCommand, Result<CreateTaskResult>>` | `ValidationBehavior.cs` | `ValidationBehavior.cs` |

# Implementation changes

```csharp
// BuildingBlocks/MediatR/ValidationBehavior.cs
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace BuildingBlocks.MediatR;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResult
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
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

    // TResponse is Result or Result<T>; both expose a static Invalid(IEnumerable<ValidationError>)
    // that returns the right closed type. A plain (TResponse)Result.Invalid(...) cast throws at
    // runtime for Result<T>, because a generic-parameter cast never runs Ardalis.Result's
    // implicit Result -> Result<T> conversion.
    private static object MakeInvalid(Type responseType, IEnumerable<ValidationError> errors)
    {
        var invalid = responseType.GetMethod("Invalid", [typeof(IEnumerable<ValidationError>)])!;
        return invalid.Invoke(null, [errors])!;
    }
}
```
# Rule changes

## MUST
- Collect all errors from all validators before returning — full error set, not first-error-only
- Return `Result.Invalid(errors)` on failure — not throw an exception
- Pass through when no validators registered — missing validator is not a fault
- `ValidationBehavior` defined in `BuildingBlocks/MediatR/ValidationBehavior.cs`
- `ValidationBehavior` constrained to `where TRequest : notnull` and `where TResponse : IResult` — activates on any MediatR request that returns a `Result` (command or query)
- Produce the short-circuit result by invoking the closed response type's own static `Invalid(IEnumerable<ValidationError>)` (reflection), never `(TResponse)Result.Invalid(...)` — a generic-parameter cast throws for `Result<T>`
- Pipeline behaviors registered via centralized `PipelineRegistration` in App.Host
- Never contain any request-specific conditions
- Never throw `ValidationException` — always return typed `Result.Invalid`
- Never register behaviors inside module registration methods
## SHOULD
- `Transient` lifetime — new behavior instance per pipeline invocation

# Unittest TestCases
- [ ] WHEN applied THEN Intercept every IRequest<TResponse> before the handler runs
- [ ] WHEN applied THEN Collect all validation errors from all registered validators for the request
- [ ] WHEN applied THEN Short-circuit with Result.Invalid(errors) if any errors exist — handler never runs for invalid input
- [ ] WHEN applied THEN Pass through to the handler if no validators are registered or all validators pass
- [ ] WHEN applied THEN Receives IEnumerable<IValidator<TRequest>> via DI — zero, one, or multiple validators supported
- [ ] WHEN applied THEN Runs all validators and collects all errors before short-circuiting — full error list, not fail-fast per field
- [ ] WHEN applied THEN Maps FluentValidation ValidationFailure to Ardalis.Result ValidationError (property name, message, error code)
- [ ] WHEN applied THEN Constrained to where TRequest : notnull and where TResponse : IResult — activates on any MediatR request that returns a Result, including commands and queries
- [ ] WHEN a Result<T> request is invalid THEN the short-circuit value is a Result<T> (not a bare Result) — no InvalidCastException
- [ ] WHEN naming 'Validation pipeline behavior' THEN pattern matches convention
