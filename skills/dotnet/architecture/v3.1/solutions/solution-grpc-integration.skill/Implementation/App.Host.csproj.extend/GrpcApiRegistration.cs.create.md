---
description: partial class ApiRegistration — the gRPC transport half, plugged into solution-api-project's ApiRegistration hooks
project_name: "App.Host"
name: "GrpcApiRegistration.cs"
element_kind: class
change_kind: create
tags:
  - solution/grpc-integration
  - element/grpc-api-registration-cs
---

# Goals
- Add the gRPC transport to the shared `ApiRegistration` partial (from [[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/solution-api-project.skill.md|solution-api-project]]) — implement `AddGrpcApiTransport`/`UseGrpcApiTransport`.
- Keep this a **separate file** for `partial class ApiRegistration` — no `.create` collision with `solution-api-project` or `solution-http-api-publication`.

# Core Principles
- `Program.cs` calls only `AddModuleApi()`/`UseModuleApi()`; those call `AddGrpcApiTransport()` when this solution is applied.
- One `MapGrpcService<T>()` per module's `{Entity}GrpcService`; Kestrel configured for HTTP/2.

# Implementation changes

```csharp
// App.Host/DependencyInjection/GrpcApiRegistration.cs
namespace App.Host.DependencyInjection;

public static partial class ApiRegistration
{
    static partial void AddGrpcApiTransport(IServiceCollection services)
    {
        services.AddGrpc();
        // configure Kestrel HTTP/2 endpoint
    }

    static partial void UseGrpcApiTransport(WebApplication app)
    {
        // one app.MapGrpcService<{Entity}GrpcService>() per published entity
    }
}
```

# Rule changes

## MUST
- Implement `AddGrpcApiTransport`/`UseGrpcApiTransport` as `static partial` methods of `ApiRegistration`, in this file only.
  - Risk: a second `.create` of `ApiRegistration.cs` collides with `solution-api-project`'s (delta-conflict design error).
  - Fix: separate file, `partial class ApiRegistration`, partial-method hooks.
- `Program.cs` never calls `AddGrpc()`/`MapGrpcService<T>()` directly.
  - Risk: gRPC wiring scattered outside the single composition point.
  - Fix: it lives in these two partial methods.

# Check list
- [ ] `GrpcApiRegistration.cs` defines `partial class ApiRegistration` with the gRPC transport hooks only.
- [ ] One `MapGrpcService<T>()` per published entity; Kestrel HTTP/2 configured.
- [ ] `Program.cs` calls only `AddModuleApi()`/`UseModuleApi()`.
