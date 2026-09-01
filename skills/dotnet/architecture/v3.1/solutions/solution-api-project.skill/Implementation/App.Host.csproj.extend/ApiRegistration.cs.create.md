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
// App.Host/DependencyInjection/ApiRegistration.cs
namespace App.Host.DependencyInjection;

public static partial class ApiRegistration
{
    public static IServiceCollection AddModuleApi(this IServiceCollection services)
    {
        // solution-http-api-publication adds:  services.AddHttpApi();
        // solution-grpc-integration adds:      services.AddGrpcApi();
        return services;
    }

    public static WebApplication UseModuleApi(this WebApplication app)
    {
        // solution-http-api-publication adds:  app.UseHttpApi();
        // solution-grpc-integration adds:      app.UseGrpcApi();
        return app;
    }
}
```

The class is `partial` so each transport solution adds its `AddHttpApi()`/`AddGrpcApi()` in its own file and calls it from the shared methods.

# Rules

## MUST
- Keep `ApiRegistration` `partial`; transport solutions add their registration in a separate partial file.
  - Risk: transport solutions editing one file conflict on every apply.
  - Fix: `partial class`, one file per transport.
- Add no transport-specific code in this file — only the shared `AddModuleApi()`/`UseModuleApi()` bodies.
  - Risk: this solution ends up owning HTTP/gRPC details it should not.
  - Fix: the bodies just call the transport partials.

# Check list
- [ ] `ApiRegistration` is a `partial` class with `AddModuleApi()` / `UseModuleApi()`.
- [ ] No HTTP/gRPC-specific code in this file.
