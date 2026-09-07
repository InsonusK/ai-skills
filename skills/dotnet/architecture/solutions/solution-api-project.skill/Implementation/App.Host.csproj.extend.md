---
description: Wire the module API composition-root extension pair
name: "App.Host.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/api-project
  - element/app-host-csproj
---

# Goals
- Add `ApiRegistration` with `AddModuleApi()` / `UseModuleApi()` that the transport solutions extend.

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    ApiRegistration.cs
  Program.cs
```

# Implementation changes
```csharp
// Program.cs
builder.Services.AddModuleApi();
// ...
app.UseModuleApi();
```

# Allowed Dependencies
- `{Module}.Api` (every module that exposes an API)

# Rules

## MUST
- Call `AddModuleApi()` / `UseModuleApi()` once each in `Program.cs`.
  - Risk: transport middleware registered ad hoc runs in an undefined order.
  - Fix: the pair is the single API composition point.

# Check list
- [ ] `Program.cs` calls `AddModuleApi()` and `UseModuleApi()` once each.
