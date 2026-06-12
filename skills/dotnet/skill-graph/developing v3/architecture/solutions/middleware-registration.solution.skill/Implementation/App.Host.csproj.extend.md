---
description: Wire centralized HTTP middleware registration in the application pipeline
name: App.Host.csproj
element_kind: project
change_kind: extend
---

# Goals
- Register the centralized `UseMiddlewarePipeline()` extension in the application pipeline
- Ensure `Program.cs` calls `UseMiddlewarePipeline()` exactly once in the correct middleware order

# Core Principles
- `Program.cs` only calls the high-level middleware extension: `UseMiddlewarePipeline()`
- HTTP middleware order is enforced inside `MiddlewareRegistration.UseMiddlewarePipeline()` — not in `Program.cs`
- `UseMiddlewarePipeline()` called after routing and before `MapControllers()` / `MapEndpoints()`

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    MiddlewareRegistration.cs
  Program.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /DependencyInjection/MiddlewareRegistration.cs | Centralized HTTP middleware registration |

# Implementation changes

Update `Program.cs` to call the centralized middleware extension:

```csharp
// App.Host/Program.cs
var app = builder.Build();

app.UseRouting();

// centralized custom middleware pipeline — must be after routing, before endpoints
app.UseMiddlewarePipeline();

app.MapControllers();

app.Run();
```

# Allowed Dependencies
- App.Host.DependencyInjection

# Rules

MUST:
- `UseMiddlewarePipeline()` called in `Program.cs`
- `UseMiddlewarePipeline()` called exactly once
- `UseMiddlewarePipeline()` called after `UseRouting()` and before `MapControllers()` / `MapEndpoints()`

MUST NOT:
- Register custom middleware directly in `Program.cs`
- Call `UseMiddlewarePipeline()` more than once
- Call `UseMiddlewarePipeline()` after `MapControllers()` / `MapEndpoints()`

# Anti-patterns
- Registering middleware directly in `Program.cs`
- Calling `UseMiddlewarePipeline()` multiple times
- Calling `UseMiddlewarePipeline()` after endpoint mapping

# Check list
- [ ] `UseMiddlewarePipeline()` called from `Program.cs`
- [ ] Called after `UseRouting()` and before `MapControllers()` / `MapEndpoints()`
- [ ] No direct custom middleware registration in `Program.cs`
