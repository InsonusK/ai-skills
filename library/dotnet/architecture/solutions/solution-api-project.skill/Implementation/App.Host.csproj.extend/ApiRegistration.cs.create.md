---
description: The AddModuleApi()/UseModuleApi() extension pair transport solutions extend
project_name: "App.Host"
name: "ApiRegistration.cs"
element_kind: class
change_kind: create
tags:
  - solution/api-project
  - element/api-registration-cs
---

# Goals
- One extension pair that composes whichever inbound transports are applied.

# Implementation changes
```csharp
// App.Host/DependencyInjection/ApiRegistration.cs   (created here — the shared half)
namespace App.Host.DependencyInjection;

public static partial class ApiRegistration
{
    public static IServiceCollection AddModuleApi(this IServiceCollection services)
    {
        AddHttpApiTransport(services);   // implemented by solution-http-api-publication, or no-op
        AddGrpcApiTransport(services);   // implemented by solution-grpc-integration, or no-op
        return services;
    }

    public static WebApplication UseModuleApi(this WebApplication app)
    {
        UseHttpApiTransport(app);
        UseGrpcApiTransport(app);
        return app;
    }

    static partial void AddHttpApiTransport(IServiceCollection services);
    static partial void AddGrpcApiTransport(IServiceCollection services);
    static partial void UseHttpApiTransport(WebApplication app);
    static partial void UseGrpcApiTransport(WebApplication app);
}
```

Each transport solution *implements* its `partial void` hooks in its own file (`HttpApiRegistration.cs`, `GrpcApiRegistration.cs`). When a transport solution is not applied, its `partial void` call compiles to nothing — so a module with no API, or only one transport, needs no conditional wiring.

# Rules

## MUST
- Keep `ApiRegistration` a `partial` class; declare the transport hooks as `static partial void`; transport solutions *implement* them in separate files.
  - Risk: two transport solutions both `.create`-ing `ApiRegistration.cs` collide (a delta-conflict design error).
  - Fix: this solution `.create`s the file with the shared bodies + `partial void` declarations; transports `.create` a *different* file implementing the hooks.
- Put no HTTP/gRPC-specific code in this file — only the shared `AddModuleApi()`/`UseModuleApi()` bodies and the hook declarations.
  - Risk: this solution ends up owning transport details it should not.
  - Fix: the bodies only call the `partial void` hooks.

# Check list
- [ ] `ApiRegistration.cs` (this file) has `AddModuleApi()`/`UseModuleApi()` + four `static partial void` transport-hook declarations.
- [ ] No HTTP/gRPC-specific code here.
- [ ] Transport solutions implement the hooks in their own files, never re-`.create` `ApiRegistration.cs`.
