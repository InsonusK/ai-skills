---
description: AddGrpcApi()/UseGrpcApi() — the single App.Host extension pair wiring every module's gRPC service registration, independent of AddApi()/UseApi()
project_name: "App.Host"
name: "GrpcRegistration.cs"
element_kind: class
change_kind: create
tags:
  - solution/grpc-integration
  - element/grpc-registration-cs
---

# Goals
- Give App.Host one call site pair, `AddGrpcApi()`/`UseGrpcApi()`, for everything the gRPC layer needs registered — mirroring `ApiRegistration`'s `AddApi()`/`UseApi()` discipline, kept in its own file since the two solutions are independent

# Core Principles
- `AddGrpcApi()`/`UseGrpcApi()` are the only call sites — `Program.cs` never calls `AddGrpc()`/`MapGrpcService<T>()` directly
- One `MapGrpcService<T>()` call per module's `{Entity}GrpcService`

# Implementation changes

```csharp
// App.Host/DependencyInjection/GrpcRegistration.cs
using TaskModule.Api.Grpc;

namespace App.Host.DependencyInjection;

public static class GrpcRegistration
{
    public static IServiceCollection AddGrpcApi(this IServiceCollection services)
    {
        services.AddGrpc();
        return services;
    }

    public static WebApplication UseGrpcApi(this WebApplication app)
    {
        app.MapGrpcService<TaskGrpcService>();
        // one MapGrpcService<T>() call per module's {Entity}GrpcService

        return app;
    }
}
```

`Program.cs` calls this alongside `ApiRegistration`'s pair when both solutions are applied — neither knows about the other:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterSampleModule).Assembly));
builder.Services.AddModules(builder.Configuration);
builder.Services.AddPipeline();
builder.Services.AddApi();       // from solution-http-api-publication, if applied
builder.Services.AddGrpcApi();   // from this solution, if applied

var app = builder.Build();
app.UseApi();       // from solution-http-api-publication, if applied
app.UseGrpcApi();   // from this solution, if applied
app.Run();
```

Kestrel serves HTTP/1.1 (REST/Swagger) and HTTP/2 (gRPC) from the same endpoint by default via protocol negotiation when TLS is in use; an HTTP-only development endpoint needs `Protocols = HttpProtocols.Http1AndHttp2` set explicitly in `appsettings.json`'s Kestrel endpoint configuration — a deployment detail of *how* both coexist, not something either `AddXxxApi()` extension needs to know about.

# Rule changes

## MUST
- `AddGrpc()`/`MapGrpcService<T>()` called only from `AddGrpcApi()`/`UseGrpcApi()`
- One `MapGrpcService<T>()` call per module that publishes a gRPC service
- Never call `AddGrpc()`/`MapGrpcService<T>()` directly from `Program.cs`
- Never assume `ApiRegistration`'s `AddApi()`/`UseApi()` were called — this file must work whether or not `solution-http-api-publication` is applied

# Check list
- [ ] `AddGrpcApi()`/`UseGrpcApi()` are the only call sites for gRPC registration
- [ ] One `MapGrpcService<T>()` per module publishing gRPC
- [ ] Works correctly whether or not `solution-http-api-publication` is also applied
