---
description: Middleware that catches ConflictException and writes 409
name: ConflictExceptionMiddleware.cs
change_kind: create
---

# Goals
- Catch any `ConflictException` thrown during an HTTP request
- Write a 409 Conflict response with the existing entity body extracted via `ex.GetValue()`
- Centralize 409 handling so controllers do not need per-action try/catch blocks

# Core Principles
- Catches non-generic `ConflictException` base class — handles any `ConflictException<T>` without knowing `T`
- Writes `application/json` response body from `ex.GetValue()`
- Does not catch non-Conflict exceptions — those propagate to the global exception handler
- Registered early in the HTTP pipeline, after routing and authentication but before endpoints

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Conflict exception middleware | `ConflictExceptionMiddleware` | `ConflictExceptionMiddleware` | `ConflictExceptionMiddleware.cs` | `ConflictExceptionMiddleware.cs` |

# Implementation changes

```csharp
// BuildingBlocks/Middleware/ConflictExceptionMiddleware.cs
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Shared.Exceptions;

public class ConflictExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ConflictExceptionMiddleware(RequestDelegate next)
        => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ConflictException ex)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = MediaTypeNames.Application.Json;

            var value = ex.GetValue();
            if (value is not null)
            {
                await JsonSerializer.SerializeAsync(
                    context.Response.Body,
                    value,
                    value.GetType(),
                    context.RequestAborted);
            }
        }
    }
}
```

# Rules

MUST:
- Catch non-generic `ConflictException` base class
- Set response status code to 409
- Set response content type to `application/json`
- Write `ex.GetValue()` as the response body

MUST NOT:
- Catch generic `Exception` — only `ConflictException`
- Write `ProblemDetails` — the body must be the existing entity so the client can recover
- Return empty body when `GetValue()` returns non-null

# Anti-patterns
- Catching `Exception` instead of `ConflictException` — would swallow unrelated errors
- Writing `ProblemDetails` instead of the existing entity — client forced to make a second GET to recover
- Registering middleware after endpoint routing without catching pipeline exceptions — middleware must wrap the endpoint invocation

# Check list
- [ ] `ConflictExceptionMiddleware` defined in `BuildingBlocks/Middleware/ConflictExceptionMiddleware.cs`
- [ ] Catches non-generic `ConflictException`
- [ ] Writes 409 with body from `ex.GetValue()`
- [ ] Registered in App.Host HTTP pipeline
