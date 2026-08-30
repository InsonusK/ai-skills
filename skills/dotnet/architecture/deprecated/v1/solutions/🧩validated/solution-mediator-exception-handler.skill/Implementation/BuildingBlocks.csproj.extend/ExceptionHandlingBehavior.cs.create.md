---
description: Pipeline behavior that catches unhandled exceptions and returns a generic Result error
project_name: BuildingBlocks
name: ExceptionHandlingBehavior.cs
element_kind: class
change_kind: create
tags:
  - solution/mediator-exception-handler
  - element/exceptionhandlingbehavior-cs
---

# Goals
- Intercept every unhandled exception thrown by a MediatR request handler or inner pipeline behavior
- Log the exception as a critical error with request context
- Return a safe, generic `Result.Error` so the API never receives a raw exception

# Core Principles
- Catches the broad `Exception` base type to guarantee no unhandled exception escapes
- Logs at `LogLevel.Critical` because an unhandled exception indicates a programming or infrastructure defect

# Structure

## Project Structure
```
/BuildingBlocks
  /MediatR
    ExceptionHandlingBehavior.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Global exception handling pipeline behavior | `ExceptionHandlingBehavior<TRequest, TResponse>` | `ExceptionHandlingBehavior<CreateTaskCommand, Result<CreateTaskResult>>` | `ExceptionHandlingBehavior.cs` | `ExceptionHandlingBehavior.cs` |

# Implementation changes

```csharp
// BuildingBlocks/MediatR/ExceptionHandlingBehavior.cs
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.MediatR;

public class ExceptionHandlingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

    public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(
                ex,
                "Unhandled exception while handling request {RequestType}. Correlation details will be available in the logs.",
                typeof(TRequest).Name);

            return (TResponse)Result.Error("An unexpected error occurred. Please try again later.");
        }
    }
}
```

# Rule changes

## MUST
- Wrap `await next()` in a `try/catch (Exception ex)` block
- Log the caught exception at `LogLevel.Critical` with `ILogger.LogCritical`
- Return `(TResponse)Result.Error(...)` with a fixed, non-detailed message
- Define the behavior in `BuildingBlocks/MediatR/ExceptionHandlingBehavior.cs`
- Constrain the behavior to `where TRequest : IRequest<TResponse>` and `where TResponse : IResult`

## SHOULD
- Include the request type name in the log message for correlation
- Keep the user-facing error message in a constant if it is reused elsewhere

## MUST NOT
- Re-throw the exception or throw a new exception from the catch block
- Return `Result.Error` with the original `ex.Message` or `ex.StackTrace`
- Catch only specific exception types

# Anti-patterns
- **Logging at Warning or Error level for unhandled exceptions**
  - Consequence: unhandled exceptions are not escalated appropriately and may be missed in alerting
  - Instead: always log at `LogLevel.Critical`

- **Returning `Result.CriticalError` without project convention**
  - Consequence: the API may map `CriticalError` to an unexpected status code or handling path
  - Instead: use `Result.Error` as the default; use `Result.CriticalError` only when the project explicitly defines it

# Check list
- [ ] `ExceptionHandlingBehavior` defined in `BuildingBlocks/MediatR/ExceptionHandlingBehavior.cs`
- [ ] `ExceptionHandlingBehavior` constrained to `IRequest<TResponse>` and `IResult`
- [ ] `try/catch (Exception ex)` wraps `await next()`
- [ ] Exception is logged at `LogLevel.Critical`
- [ ] `Result.Error` with a generic message is returned
- [ ] No exception details are returned to the caller

# Unittest TestCases
- [ ] WHEN handler succeeds THEN behavior returns the handler result unchanged
- [ ] WHEN handler throws THEN behavior logs the exception at Critical level
- [ ] WHEN handler throws THEN behavior returns Result.Error with a generic message
- [ ] WHEN handler throws THEN behavior does not re-throw the exception
- [ ] WHEN naming 'Global exception handling pipeline behavior' THEN pattern matches convention
