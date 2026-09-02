---
name: plateau-domain-service--class-api-registration
description: Class ApiRegistration in the plateau-domain-service plateau — the App.Host AddModuleApi() / UseModuleApi() extension pair with static partial void hooks that each transport solution implements
whenToUse: when wiring a module's API into the composition root, or adding an HTTP / gRPC transport hook
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[../../../../../solutions/solution-api-project.skill/solution-api-project.skill.md|solution-api-project]]"
  - "[[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]]"
---

# Goal
- Give `App.Host` one composition touch-point for module APIs: `AddModuleApi()` / `UseModuleApi()`, called once from `Program.cs`, with `static partial void` hooks that `solution-http-api-publication` (`HttpApiRegistration`) and `solution-grpc-integration` (`GrpcApiRegistration`) implement — so a module can serve REST, gRPC, or both over the same `ISender` dispatch.

__Applied solutions:__
- [[../../../../../solutions/solution-api-project.skill/solution-api-project.skill.md|solution-api-project]] - [[../../../../../solutions/solution-api-project.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `static partial class ApiRegistration` in `App.Host/DependencyInjection`. `solution-api-project` creates the pair + the partial hook declarations; it adds no endpoints.
- Each transport solution adds a file implementing one hook (`HttpApiRegistration.cs` / `GrpcApiRegistration.cs`) as `static partial void` on this class — no `.create` collision on `ApiRegistration.cs` itself.
- `Program.cs` calls only `AddModuleApi()` (services) and `UseModuleApi()` (pipeline / endpoint mapping).

# Implementation
```csharp
// Skill: plateau-domain-service--class-api-registration
// Plateau: domain-service
// Version: 20260902000000
namespace App.Host.DependencyInjection;

public static partial class ApiRegistration
{
    public static IServiceCollection AddModuleApi(this IServiceCollection services)
    {
        AddHttpApi(services);   // implemented by solution-http-api-publication
        AddGrpcApi(services);   // implemented by solution-grpc-integration
        return services;
    }

    public static WebApplication UseModuleApi(this WebApplication app)
    {
        UseHttpApi(app);
        UseGrpcApi(app);
        return app;
    }

    static partial void AddHttpApi(IServiceCollection services);
    static partial void AddGrpcApi(IServiceCollection services);
    static partial void UseHttpApi(WebApplication app);
    static partial void UseGrpcApi(WebApplication app);
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]]

# Rules
MUST:
- `static partial class ApiRegistration` in `App.Host/DependencyInjection`; declare `static partial void` hooks, one per transport concern.
- Each transport solution implements exactly one hook in its own file — never re-`create` `ApiRegistration.cs`.
- `Program.cs` calls only `AddModuleApi()` / `UseModuleApi()`.
- `solution-api-project` adds no endpoint here.
- Never apply several plateau templates per class.

# Check list
- [ ] `static partial class ApiRegistration` with `AddModuleApi()` / `UseModuleApi()` + `static partial void` hooks.
- [ ] Transport solutions implement hooks in separate files, no collision.
- [ ] `Program.cs` calls only the pair.

# Unittest TestCases
- [ ] WHEN only the HTTP transport is applied THEN `AddGrpcApi` is a no-op (unimplemented partial) and the app still starts.
