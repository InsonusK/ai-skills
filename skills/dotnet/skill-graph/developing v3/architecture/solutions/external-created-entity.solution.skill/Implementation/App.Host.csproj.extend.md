---
description: Extend MiddlewareRegistration with ConflictExceptionMiddleware
name: App.Host.csproj
element_kind: project
change_kind: extend
---

# Goals
- Register `ConflictExceptionMiddleware` in the centralized HTTP middleware pipeline so it wraps all controller invocations

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    MiddlewareRegistration.cs    ← extended with ConflictExceptionMiddleware
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /DependencyInjection/MiddlewareRegistration.cs | Register ConflictExceptionMiddleware in the centralized HTTP middleware pipeline |

# Allowed Dependencies
- Shared
- BuildingBlocks
- App.Infrastructure
- All module Application and Api projects

# Rules

MUST:
- `ConflictExceptionMiddleware` registered inside `UseMiddlewarePipeline()`
- `UseMiddlewarePipeline()` called before `MapControllers()` or endpoint routing so it wraps endpoint execution

MUST NOT:
- Register `ConflictExceptionMiddleware` after `MapControllers()` without wrapping endpoints — it must catch pipeline exceptions

# Anti-patterns
- Registering `ConflictExceptionMiddleware` only after `MapControllers()` — won't catch exceptions thrown inside endpoint handlers

# Check list
- [ ] `ConflictExceptionMiddleware` registered inside `UseMiddlewarePipeline()`
