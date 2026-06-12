---
description: Register ConflictExceptionMiddleware in the centralized HTTP middleware pipeline
project_name: App.Host
name: MiddlewareRegistration.cs
element_kind: class
change_kind: extend
---

# Goals
- Register `ConflictExceptionMiddleware` in the centralized HTTP middleware pipeline
- Ensure the middleware wraps endpoint execution so it can catch `ConflictException` thrown by MediatR pipeline behaviors

# Core Principles
- `ConflictExceptionMiddleware` registered inside `UseMiddlewarePipeline()` from [[middleware-registration.solution.skill]]
- Registered before `MapControllers()` so it wraps controller action execution
- Extension method lives in `App.Host/DependencyInjection` for consistency with other registrations

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    MiddlewareRegistration.cs      ← extended with ConflictExceptionMiddleware
```

# Implementation changes

Extend `MiddlewareRegistration` from [[middleware-registration.solution.skill]] with `ConflictExceptionMiddleware`:

```csharp
// App.Host/DependencyInjection/MiddlewareRegistration.cs
using BuildingBlocks.Middleware;

public static class MiddlewareRegistration
{
    public static IApplicationBuilder UseMiddlewarePipeline(
        this IApplicationBuilder app)
    {
        // custom middleware registered in execution order
        app.UseMiddleware<ConflictExceptionMiddleware>();

        return app;
    }
}
```

Usage in `App.Host/Program.cs` (provided by [[middleware-registration.solution.skill]]):

```csharp
var app = builder.Build();

app.UseRouting();

// centralized custom middleware pipeline — must be registered after routing,
// but before MapControllers / MapEndpoints
app.UseMiddlewarePipeline();

app.MapControllers();

app.Run();
```

# Rules

MUST:
- `ConflictExceptionMiddleware` registered inside `UseMiddlewarePipeline()`
- `UseMiddlewarePipeline()` called before `MapControllers()` or `MapEndpoints()`

MUST NOT:
- Register `ConflictExceptionMiddleware` after `MapControllers()` — it won't wrap endpoint execution

# Anti-patterns
- Calling `UseMiddleware<ConflictExceptionMiddleware>()` after `MapControllers()` — exceptions thrown inside controllers will bypass it

# Check list
- [ ] `ConflictExceptionMiddleware` registered inside `UseMiddlewarePipeline()`
- [ ] `UseMiddlewarePipeline()` called in `Program.cs` before `MapControllers()`

# Unittest TestCases
- [ ] WHEN applied THEN Register ConflictExceptionMiddleware in the centralized HTTP middleware pipeline
- [ ] WHEN applied THEN Middleware wraps endpoint execution so it can catch ConflictException thrown by MediatR pipeline behaviors
- [ ] WHEN applied THEN Middleware is registered once in App.Host
- [ ] WHEN applied THEN Registered before MapControllers() so it wraps controller action execution
- [ ] WHEN applied THEN Extension method lives in App.Host/DependencyInjection for consistency with other registrations
- [ ] WHEN verified THEN ConflictExceptionMiddleware registered inside UseMiddlewarePipeline()
- [ ] WHEN verified THEN UseMiddlewarePipeline() called in Program.cs before MapControllers()
