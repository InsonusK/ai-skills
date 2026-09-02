---
description: partial class ApiRegistration — the HTTP transport half (AddHttpApi/UseHttpApi) plugged into solution-api-project's ApiRegistration
project_name: "App.Host"
name: "HttpApiRegistration.cs"
element_kind: class
change_kind: create
tags:
  - solution/http-api-publication
  - element/http-api-registration-cs
---

# Goals
- Add the REST transport to the shared `ApiRegistration` partial (from [[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/solution-api-project.skill.md|solution-api-project]]) — `AddHttpApi()` registers controllers, ProblemDetails, and one Swagger doc per module; `UseHttpApi()` maps them.
- Keep this a **separate file** for the same `partial class ApiRegistration` — `solution-api-project` owns the shared `AddModuleApi()`/`UseModuleApi()` bodies, this file only adds `AddHttpApi()`/`UseHttpApi()` and the shared methods call it. No `.create` collision on `ApiRegistration.cs`.

# Core Principles
- `Program.cs` calls only `AddModuleApi()`/`UseModuleApi()` (the shared pair); those call `AddHttpApi()` when this solution is applied.
- One `SwaggerDoc` + `SwaggerEndpoint` + `DocInclusionPredicate` arm per module that publishes a REST API, matched by its own `{Module}ApiSwaggerRegistration.MatchesRoute`.

# Implementation changes

```csharp
// App.Host/DependencyInjection/HttpApiRegistration.cs
using Microsoft.OpenApi.Models;

namespace App.Host.DependencyInjection;

public static partial class ApiRegistration
{
    static partial void AddHttpApiTransport(IServiceCollection services)
    {
        services.AddControllers();
        services.AddProblemDetails();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // one options.SwaggerDoc(...) + DocInclusionPredicate arm per module's {Module}ApiSwaggerRegistration
        });
    }

    static partial void UseHttpApiTransport(WebApplication app)
    {
        app.UseExceptionHandler();
        app.MapControllers();
        // one app.Map{System}Endpoints() per module's Minimal API
        app.UseSwagger();
        app.UseSwaggerUI(/* one SwaggerEndpoint per module */);
    }
}
```

`solution-api-project`'s `ApiRegistration.cs` declares the `partial void AddHttpApiTransport`/`UseHttpApiTransport` hooks and calls them from `AddModuleApi()`/`UseModuleApi()`; this file implements them. A module without VP8 simply has no implementation — the `partial void` call compiles to nothing.

# Rule changes

## MUST
- Implement `AddHttpApiTransport`/`UseHttpApiTransport` as `static partial` methods of `ApiRegistration`, in this file only.
  - Risk: a second `.create` of `ApiRegistration.cs` collides with `solution-api-project`'s.
  - Fix: separate file, `partial class ApiRegistration`, partial-method hooks.
- One `SwaggerDoc`/`SwaggerEndpoint`/`DocInclusionPredicate` arm per module publishing a REST API; never a single solution-wide `v1` document.
  - Risk: a `v1`-everything document makes a multi-module Swagger UI unnavigable and leaks routes across modules.
  - Fix: per-module doc keyed on `{Module}ApiSwaggerRegistration`.
- `UseExceptionHandler()` before `MapControllers()`.
  - Risk: exceptions in the pipeline reach the client as raw 500s.
  - Fix: order it first in `UseHttpApiTransport`.

# Check list
- [ ] `HttpApiRegistration.cs` defines `partial class ApiRegistration` with the HTTP transport hooks only.
- [ ] `Program.cs` still calls only `AddModuleApi()`/`UseModuleApi()`.
- [ ] One Swagger doc per REST module; `UseExceptionHandler()` precedes `MapControllers()`.
