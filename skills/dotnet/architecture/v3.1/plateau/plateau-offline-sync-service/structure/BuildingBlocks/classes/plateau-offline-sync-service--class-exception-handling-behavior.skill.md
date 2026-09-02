---
name: plateau-offline-sync-service--class-exception-handling-behavior
description: Class ExceptionHandlingBehavior<TRequest, TResponse> in the plateau-offline-sync-service plateau — the first pipeline behavior; catches every unhandled exception, logs it Critical, returns a generic Result.Error
whenToUse: when creating or editing ExceptionHandlingBehavior, or changing how an unhandled exception is logged or mapped to a Result
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]]"
---

# Goal
- Guarantee no unhandled exception from a handler or an inner behavior reaches the caller — catch it, log it `Critical` with the stable `LogEvents.UnhandledException` id, and return a generic `Result.Error` with no exception detail.

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../../solutions/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend/ExceptionHandlingBehavior.cs.create.md|ExceptionHandlingBehavior.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- Generic and content-free: `where TRequest : notnull`, `where TResponse : IResult`. Catches the broad `Exception`.
- Logs through `ILogger<T>` at `LogLevel.Critical` with `LogEvents.UnhandledException` (from `Shared.Logging`); the message carries the request type name, never the exception detail.
- Returns a fixed generic message ("An unexpected error occurred. Please try again later.") by invoking the closed `TResponse`'s own static `Error(string)` via reflection — a plain `(TResponse)Result.Error(...)` cast throws for `Result<T>`.
- Registered **first** in the pipeline so it wraps every other behavior and the handler.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Global exception behavior | `ExceptionHandlingBehavior<TRequest, TResponse>` | `ExceptionHandlingBehavior<CreateTaskCommand, Result<CreateTaskResult>>` | `ExceptionHandlingBehavior.cs` | `ExceptionHandlingBehavior.cs` |

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-exception-handling-behavior
// Plateau: core
// Version: 20260902000000
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
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Critical, LogEvents.UnhandledException, ex,
                "Unhandled exception while handling request {RequestType}.", typeof(TRequest).Name);

            var error = typeof(TResponse).GetMethod("Error", [typeof(string)])!;
            return (TResponse)error.Invoke(null, ["An unexpected error occurred. Please try again later."])!;
        }
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] - [[../../../../../solutions/solution-mediator-exception-handler.skill/Implementation/BuildingBlocks.csproj.extend/ExceptionHandlingBehavior.cs.create.md|ExceptionHandlingBehavior.cs.create]]

# Rules
MUST:
- Constrain to `where TRequest : notnull` and `where TResponse : IResult`; wrap `await next()` in `try/catch (Exception ex)`.
- Log at `LogLevel.Critical` with `LogEvents.UnhandledException`; include the request type name, never the exception message/stack.
- Return the generic message through the closed `TResponse`'s own static `Error` (reflection) — never `(TResponse)Result.Error(...)`.
- Be registered first in `PipelineRegistration.AddPipeline()`.
- Never apply several plateau templates per class.
- Never re-throw, never catch only specific types, never route an expected business failure through here.

# Check list
- [ ] `where TRequest : notnull`, `where TResponse : IResult`; catches `Exception`.
- [ ] Logs `Critical` with `LogEvents.UnhandledException`; no exception detail in the returned `Result`.
- [ ] A `Result<T>` request maps to a `Result<T>` error (no `InvalidCastException`).
- [ ] Registered first in `AddPipeline()`.

# Unittest TestCases
- [ ] WHEN the inner delegate succeeds THEN the result passes through unchanged.
- [ ] WHEN the inner delegate throws THEN the result status is `Error` with the generic message.
- [ ] WHEN the inner delegate throws THEN the exception is logged once at `Critical` with `LogEvents.UnhandledException`.
- [ ] WHEN the inner delegate throws THEN no exception propagates out of `Handle`.
