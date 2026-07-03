---
description: Centralized HTTP middleware registration extension
project_name: App.Host
name: MiddlewareRegistration.cs
element_kind: class
change_kind: create
---

# Goals
- Provide the single `UseMiddlewarePipeline()` extension method where all custom HTTP middleware are registered
- Be the authoritative record of custom HTTP middleware order

# Core Principles
- `MiddlewareRegistration` is a static class with one public extension method
- `UseMiddlewarePipeline()` returns `IApplicationBuilder` so it can be chained in `Program.cs`
- Individual middleware solutions extend this method to insert their middleware in order

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    MiddlewareRegistration.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Middleware registration | `MiddlewareRegistration` | `MiddlewareRegistration` | `MiddlewareRegistration.cs` | `MiddlewareRegistration.cs` |

# Implementation changes

```csharp
// App.Host/DependencyInjection/MiddlewareRegistration.cs
using Microsoft.AspNetCore.Builder;

namespace App.Host.DependencyInjection;

public static class MiddlewareRegistration
{
    public static IApplicationBuilder UseMiddlewarePipeline(
        this IApplicationBuilder app)
    {
        // Custom HTTP middleware are registered here in execution order.
        // Each middleware solution extends this method to add its own middleware.

        return app;
    }
}
```
# Rule changes

## MUST
- `MiddlewareRegistration` defined as a static class in `App.Host/DependencyInjection/MiddlewareRegistration.cs`
- `UseMiddlewarePipeline()` is an extension method on `IApplicationBuilder`
- `UseMiddlewarePipeline()` returns `IApplicationBuilder`
- `MiddlewareRegistration.cs` defined in `App.Host/DependencyInjection/MiddlewareRegistration.cs`
- `UseMiddlewarePipeline()` called once from `Program.cs`
- All custom middleware registered inside `UseMiddlewarePipeline()` using `app.UseMiddleware<TMiddleware>()`
- Middleware registered in intended execution order
- `UseMiddlewarePipeline()` called after routing and before `MapControllers()` / `MapEndpoints()`

## SHOULD
- Keep `UseMiddlewarePipeline()` the only method that adds custom HTTP middleware registrations

## MUST NOT
- Register middleware inside module registration methods
- Change middleware order in multiple files
- Register custom middleware inside module registration methods
- Register custom middleware directly in `Program.cs`
- Create multiple middleware registration extension methods

# Anti-patterns
- Middleware order scattered across multiple files
- Registering middleware in `Program.cs` instead of inside `MiddlewareRegistration`

# Check list
- [ ] `MiddlewareRegistration.cs` exists under `App.Host/DependencyInjection`
- [ ] `UseMiddlewarePipeline()` extension method on `IApplicationBuilder`
- [ ] `UseMiddlewarePipeline()` returns `IApplicationBuilder`

# Unittest TestCases
- [ ] WHEN applied THEN `MiddlewareRegistration` class exists in `App.Host/DependencyInjection`
- [ ] WHEN applied THEN `UseMiddlewarePipeline()` extends `IApplicationBuilder`
- [ ] WHEN applied THEN `UseMiddlewarePipeline()` returns the same `IApplicationBuilder` instance
