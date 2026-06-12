---
description: Register ConflictExceptionMiddleware in the HTTP pipeline
project_name: App.Host
name: MiddlewareRegistration.cs
element_kind: class
change_kind: create
---

# Goals
- Add an extension method that registers `ConflictExceptionMiddleware` in the HTTP pipeline
- Ensure the middleware wraps endpoint execution so it can catch `ConflictException` thrown by MediatR pipeline behaviors

# Core Principles
- Middleware is registered once in App.Host
- Registered before `MapControllers()` so it wraps controller action execution
- Extension method lives in App.Host/DependencyInjection for consistency with other registrations

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Middleware registration extension | `MiddlewareRegistration` | `MiddlewareRegistration` | `MiddlewareRegistration.cs` | `MiddlewareRegistration.cs` |

# Implementation changes

```csharp
// App.Host/DependencyInjection/MiddlewareRegistration.cs
using BuildingBlocks.Middleware;

public static class MiddlewareRegistration
{
    public static IApplicationBuilder UseConflictExceptionMiddleware(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<ConflictExceptionMiddleware>();
    }
}
```

Usage in `App.Host/Program.cs`:

```csharp
var app = builder.Build();

app.UseRouting();

// must be registered after routing and authentication/authorization,
// but before MapControllers / MapEndpoints
app.UseConflictExceptionMiddleware();

app.MapControllers();

app.Run();
```

# Rules

MUST:
- Extension method calls `app.UseMiddleware<ConflictExceptionMiddleware>()`
- Middleware registered before `MapControllers()` or `MapEndpoints()`

MUST NOT:
- Register middleware after `MapControllers()` — it won't wrap endpoint execution

# Anti-patterns
- Calling `UseMiddleware<ConflictExceptionMiddleware>()` after `MapControllers()` — exceptions thrown inside controllers will bypass it

# Check list
- [ ] `UseConflictExceptionMiddleware` extension defined in App.Host
- [ ] Called in `Program.cs` before `MapControllers()`

# Unittest TestCases
- [ ] WHEN applied THEN Add an extension method that registers ConflictExceptionMiddleware in the HTTP pipeline
- [ ] THEN it ensure the middleware wraps endpoint execution so it can catch ConflictException thrown by MediatR pipeline behaviors
- [ ] WHEN applied THEN Middleware is registered once in App.Host
- [ ] WHEN applied THEN Registered before MapControllers() so it wraps controller action execution
- [ ] WHEN applied THEN Extension method lives in App.Host/DependencyInjection for consistency with other registrations
- [ ] WHEN verified THEN UseConflictExceptionMiddleware extension defined in App.Host
- [ ] WHEN verified THEN Called in Program.cs before MapControllers()
- [ ] WHEN naming 'Middleware registration extension' THEN pattern matches convention
