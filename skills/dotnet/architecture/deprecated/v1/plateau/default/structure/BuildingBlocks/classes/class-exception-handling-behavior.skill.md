---
name: class-exception-handling-behavior
description: Pipeline behavior that catches unhandled exceptions and returns a generic Result error
domain: skill
type: template
version: 20260704153836
plateau: default
tags:
  - skill/template/class
  - plateau/default
  - stack/dotnet
  - concern/architecture

created_by:
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]]"
---

# Goal
- Intercept every unhandled exception thrown by a MediatR request handler or inner pipeline behavior
- Log the exception as a critical error with request context
- Return a safe, generic `Result.Error` so the API never receives a raw exception

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend/ExceptionHandlingBehavior.cs.create|ExceptionHandlingBehavior.cs]]

# Core Principles
- Apply ONE plateau template per class
- Generic behavior activated for any `IRequest<TResponse>` where `TResponse` implements `IResult`
- Catches the broad `Exception` base type to guarantee no unhandled exception escapes
- Logs at `LogLevel.Critical` because an unhandled exception indicates a programming or infrastructure defect
- Returns a constant user-facing message; internal diagnostic details stay in logs

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend/ExceptionHandlingBehavior.cs.create|ExceptionHandlingBehavior.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Global exception handling pipeline behavior | `ExceptionHandlingBehavior<TRequest, TResponse>` | `ExceptionHandlingBehavior<CreateTaskCommand, Result<CreateTaskResult>>` | `ExceptionHandlingBehavior.cs` | `ExceptionHandlingBehavior.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend/ExceptionHandlingBehavior.cs.create|ExceptionHandlingBehavior.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-exception-handling-behavior
//Plateau: default
//Version: 20260704153836
```

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

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend/ExceptionHandlingBehavior.cs.create|ExceptionHandlingBehavior.cs]]

# Rules
MUST:
	- Constrained to `where TRequest : IRequest<TResponse>` and `where TResponse : IResult`
	- Wrap `await next()` in a `try/catch (Exception ex)` block
	- Log the caught exception at `LogLevel.Critical` with `ILogger.LogCritical`
	- Return `(TResponse)Result.Error(...)` with a fixed, non-detailed message
MUST NOT:
	- Re-throw the exception or throw a new exception from the catch block
	- Return `Result.Error` with the original `ex.Message` or `ex.StackTrace`
	- Catch only specific exception types

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend/ExceptionHandlingBehavior.cs.create|ExceptionHandlingBehavior.cs]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- Logging at Warning or Error level for unhandled exceptions
- Returning `Result.CriticalError` without project convention
- Returning exception details instead of a generic `Result.Error`

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend/ExceptionHandlingBehavior.cs.create|ExceptionHandlingBehavior.cs]]

# Check list
- [ ] `ExceptionHandlingBehavior` defined in `BuildingBlocks/MediatR/ExceptionHandlingBehavior.cs`
- [ ] `ExceptionHandlingBehavior` constrained to `IRequest<TResponse>` and `IResult`
- [ ] `try/catch (Exception ex)` wraps `await next()`
- [ ] Exception is logged at `LogLevel.Critical`
- [ ] `Result.Error` with a generic message is returned
- [ ] No exception details are returned to the caller

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend/ExceptionHandlingBehavior.cs.create|ExceptionHandlingBehavior.cs]]

# Unittest TestCases
- [ ] WHEN handler succeeds THEN behavior returns the handler result unchanged
- [ ] WHEN handler throws THEN behavior logs the exception at Critical level
- [ ] WHEN handler throws THEN behavior returns Result.Error with a generic message
- [ ] WHEN handler throws THEN behavior does not re-throw the exception
- [ ] WHEN naming 'Global exception handling pipeline behavior' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill|solution-mediator-exception-handler]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend/ExceptionHandlingBehavior.cs.create|ExceptionHandlingBehavior.cs]]
