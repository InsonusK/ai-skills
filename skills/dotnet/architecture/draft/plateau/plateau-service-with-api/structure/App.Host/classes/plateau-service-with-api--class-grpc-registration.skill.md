---
name: class-grpc-registration
description: Class GrpcRegistration in the service-with-api plateau
whenToUse: when wiring gRPC service registration into the composition root
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/class
  - plateau/service-with-api
created_by:
  - "[[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]]"
---

# Goal
- Give App.Host one call site pair, `AddGrpcApi()`/`UseGrpcApi()`, for everything the gRPC layer needs registered — independent of `ApiRegistration`

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../../solutions/solution-grpc-integration.skill/Implementation/App.Host.csproj.extend/GrpcRegistration.cs.create.md|GrpcRegistration.cs.create]]

# Core Principles
- `AddGrpcApi()`/`UseGrpcApi()` are the only call sites — `Program.cs` never calls `AddGrpc()`/`MapGrpcService<T>()` directly
- One `MapGrpcService<T>()` per module that publishes gRPC
- Never assumes `ApiRegistration`'s `AddApi()`/`UseApi()` were called

# Implementation
```csharp
//Skill: class-grpc-registration
//Plateau: service-with-api
//Version: 20260825120000

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
        return app;
    }
}
```

One `MapGrpcService<T>()` call added per module that publishes gRPC — see [[../../../../../solutions/solution-grpc-integration.skill/Implementation/App.Host.csproj.extend/GrpcRegistration.cs.create.md|GrpcRegistration.cs.create]] for the Kestrel HTTP/2 note.

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../../solutions/solution-grpc-integration.skill/Implementation/App.Host.csproj.extend/GrpcRegistration.cs.create.md|GrpcRegistration.cs.create]]

# Rules
MUST:
- `AddGrpc()`/`MapGrpcService<T>()` called only from `AddGrpcApi()`/`UseGrpcApi()`
MUST NOT:
- Assume `ApiRegistration`'s methods were called

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../../solutions/solution-grpc-integration.skill/Implementation/App.Host.csproj.extend/GrpcRegistration.cs.create.md|GrpcRegistration.cs.create]]

# Check list
- [ ] `AddGrpcApi()`/`UseGrpcApi()` are the only call sites for gRPC registration
- [ ] Works correctly whether or not `solution-http-api-publication` is also applied

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../../solutions/solution-grpc-integration.skill/Implementation/App.Host.csproj.extend/GrpcRegistration.cs.create.md|GrpcRegistration.cs.create]]
