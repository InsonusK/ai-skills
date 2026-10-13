---
description: Wire the gRPC layer into the composition root — service registration and HTTP/2 Kestrel support, independent of whatever solution-http-api-publication may or may not have already wired
project_name: "App.Host"
name: "App.Host.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/grpc-integration
  - element/app-host-csproj
---

# Goals
- Give App.Host one extension pair, `AddGrpcApi()`/`UseGrpcApi()`, wiring every module's gRPC services — the same centralized-registration discipline `AddModules()`/`AddPipeline()`/`AddApi()` already establish
- Ensure App.Host runs an ASP.NET Core web host, since `Grpc.AspNetCore` requires one — already true if `solution-http-api-publication` is also applied, established independently here otherwise

# Rule changes

## MUST
- `Program.cs` use `WebApplication.CreateBuilder` (shared with `solution-http-api-publication` if both are applied — this is one requirement, not two)
- `builder.Services.AddGrpcApi()` and `app.UseGrpcApi()` called from `Program.cs`, alongside `AddApi()`/`UseApi()` when both solutions are applied
- Never reference `{Module}.Application`/`{Module}.Domain` from `App.Host` on account of this solution

# Check list
- [ ] `Program.cs` uses `WebApplication.CreateBuilder`
- [ ] `AddGrpcApi()`/`UseGrpcApi()` called once each, from `Program.cs`
