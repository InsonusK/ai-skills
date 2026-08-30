---
name: plateau-service-with-api--class-exception-handling-behavior
description: Class ExceptionHandlingBehavior in the service-with-api plateau
whenToUse: when creating or editing ExceptionHandlingBehavior, or creating another pipeline behavior that plays the same role
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/class
  - plateau/service-with-api
created_by:
  - "[[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]]"
---

# Goal
- Intercept every unhandled exception thrown by a MediatR request handler or inner pipeline behavior
- Log the exception as a critical error with request context
- Return a safe, generic `Result.Error` so the API never receives a raw exception

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../../solutions/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend/ExceptionHandlingBehavior.cs.create.md|ExceptionHandlingBehavior.cs.create]]

# Core Principles
- Apply ONE plateau template per class
- Catches the broad `Exception` base type to guarantee no unhandled exception escapes
- Logs at `LogLevel.Critical` because an unhandled exception indicates a programming or infrastructure defect

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../../solutions/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend/ExceptionHandlingBehavior.cs.create.md|ExceptionHandlingBehavior.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Global exception handling pipeline behavior | `ExceptionHandlingBehavior<TRequest, TResponse>` | `ExceptionHandlingBehavior<CreateTaskCommand, Result<CreateTaskResult>>` | `ExceptionHandlingBehavior.cs` | `ExceptionHandlingBehavior.cs` |

# Implementation
```csharp
//Skill: class-exception-handling-behavior
//Plateau: service-with-api
//Version: 20260825120000

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
- [[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../../solutions/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend/ExceptionHandlingBehavior.cs.create.md|ExceptionHandlingBehavior.cs.create]]

# Rules
MUST:
- Wrap `await next()` in a `try/catch (Exception ex)` block
- Log the caught exception at `LogLevel.Critical` with `ILogger.LogCritical`
- Return `(TResponse)Result.Error(...)` with a fixed, non-detailed message
- Constrain the behavior to `where TRequest : IRequest<TResponse>` and `where TResponse : IResult`
MUST NOT:
- Re-throw the exception or throw a new exception from the catch block
- Return `Result.Error` with the original `ex.Message` or `ex.StackTrace`
- Catch only specific exception types

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../../solutions/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend/ExceptionHandlingBehavior.cs.create.md|ExceptionHandlingBehavior.cs.create]]

# Check list
- [ ] `ExceptionHandlingBehavior` defined in `BuildingBlocks/MediatR/ExceptionHandlingBehavior.cs`
- [ ] `try/catch (Exception ex)` wraps `await next()`
- [ ] Exception is logged at `LogLevel.Critical`
- [ ] No exception details are returned to the caller

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../../solutions/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend/ExceptionHandlingBehavior.cs.create.md|ExceptionHandlingBehavior.cs.create]]

# Unittest TestCases
- [ ] WHEN handler succeeds THEN behavior returns the handler result unchanged
- [ ] WHEN handler throws THEN behavior logs the exception at Critical level and returns `Result.Error` with a generic message, without re-throwing

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../../solutions/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend/ExceptionHandlingBehavior.cs.create.md|ExceptionHandlingBehavior.cs.create]]
