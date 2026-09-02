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
- Logs with the stable `LogEvents.UnhandledException` event id from [[skills/dotnet/architecture/v3.1/solutions/solution-app-logging.skill/solution-app-logging.skill.md|solution-app-logging]] — an unhandled exception is the single most alert-worthy line, so it carries a fixed id

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
using Shared.Logging;

namespace BuildingBlocks.MediatR;

public sealed class ExceptionHandlingBehavior<TRequest, TResponse>(
    ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResult
{
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
            logger.Log(
                LogLevel.Critical,
                LogEvents.UnhandledException,
                ex,
                "Unhandled exception while handling request {RequestType}. Correlation details will be available in the logs.",
                typeof(TRequest).Name);

            // TResponse is Result or Result<T>; invoke the closed type's own static Error(string).
            // A plain (TResponse)Result.Error(...) cast throws at runtime for Result<T>.
            var error = typeof(TResponse).GetMethod("Error", [typeof(string)])!;
            return (TResponse)error.Invoke(null, ["An unexpected error occurred. Please try again later."])!;
        }
    }
}
```

`LogEvents` lives in `Shared.Logging` (from [[skills/dotnet/architecture/v3.1/solutions/solution-app-logging.skill/solution-app-logging.skill.md|solution-app-logging]]); `BuildingBlocks` already references `Shared`.

# Rule changes

## MUST
- Wrap `await next()` in a `try/catch (Exception ex)` block
- Log the caught exception at `LogLevel.Critical` with the `LogEvents.UnhandledException` event id
- Return a fixed, non-detailed message by invoking the closed response type's own static `Error(string)` (reflection), never `(TResponse)Result.Error(...)` — a generic-parameter cast throws for `Result<T>`
- Define the behavior in `BuildingBlocks/MediatR/ExceptionHandlingBehavior.cs`
- Constrain the behavior to `where TRequest : notnull` and `where TResponse : IResult`
- Never re-throw the exception or throw a new exception from the catch block
- Never return `Result.Error` with the original `ex.Message` or `ex.StackTrace`
- Never catch only specific exception types

## SHOULD
- Include the request type name in the log message for correlation
- Keep the user-facing error message in a constant if it is reused elsewhere
- Avoid logging at Warning or Error level for unhandled exceptions
  - Risk: unhandled exceptions are not escalated appropriately and may be missed in alerting
  - Fix: always log at `LogLevel.Critical`
- Avoid returning `Result.CriticalError` without project convention
  - Risk: the API may map `CriticalError` to an unexpected status code or handling path
  - Fix: use `Result.Error` as the default; use `Result.CriticalError` only when the project explicitly defines it

# Check list
- [ ] `ExceptionHandlingBehavior` defined in `BuildingBlocks/MediatR/ExceptionHandlingBehavior.cs`
- [ ] `ExceptionHandlingBehavior` constrained to `notnull` and `IResult`
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
