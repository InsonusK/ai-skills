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
# Rule changes

## MUST
- Collect all errors from all validators before returning — full error set, not first-error-only
- Return `Result.Invalid(errors)` on failure — not throw an exception
- Pass through when no validators registered — missing validator is not a fault
- `ValidationBehavior` defined in `BuildingBlocks/MediatR/ValidationBehavior.cs`
- `ValidationBehavior` constrained to `where TRequest : IRequest<TResponse>` and `where TResponse : IResult`
- Pipeline behaviors registered via centralized `PipelineRegistration` in App.Host
## MUST NOT
- Contain any request-specific conditions
- Throw `ValidationException` — always return typed `Result.Invalid`
- Register behaviors inside module registration methods

## SHOULD
- `Transient` lifetime — new behavior instance per pipeline invocation

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
