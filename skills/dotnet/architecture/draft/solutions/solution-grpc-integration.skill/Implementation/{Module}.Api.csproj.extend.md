---
description: Add Protos, Grpc, and the RpcException extension to {Module}.Api — the module's gRPC adapter layer, thin over ISender, independent of the HTTP Controllers solution-http-api-publication adds to this same project
project_name: "{Module}.Api"
name: "{Module}.Api.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/grpc-integration
  - element/module-api-csproj
---

# Goals
- Give the module a gRPC surface — one service per entity, generated from `.proto`, dispatching the same commands/queries the HTTP layer would
- Keep the gRPC layer thin: map input, dispatch exactly one `ISender.Send()`, map output — nothing else

# Core Principles
- Lives in the same `{Module}.Api` project `solution-http-api-publication` extends — both are external adapters at the same architectural layer, differing only in protocol
- Adding this solution does not require `solution-http-api-publication` to already be applied, and vice versa

# Structure

```
/{Module}.Api
  /Protos
    {Entity}.proto
  /Grpc
    {Entity}GrpcService.cs
  /Extensions
    RpcExceptionExtensions.cs
```

`{Module}.Api.csproj` references `Grpc.AspNetCore` and `Grpc.Tools` (build-time codegen) in addition to whatever `solution-http-api-publication` already added, if it's also applied.

# Rule changes

## MUST
- `.proto` files live under `/Protos`, referenced from the `.csproj` via `<Protobuf Include="Protos/*.proto" GrpcServices="Server" />`
- `{Entity}GrpcService.cs` live under `/Grpc`

## MUST NOT
- Reference `{Module}.Application`, `{Module}.Domain`, `IRepository<T>`, `IUnitOfWork`, `DbContext`, or any domain entity type — same boundary `solution-http-api-publication` holds for Controllers

# Check list
- [ ] `.proto` files under `/Protos`, wired into the `.csproj` as `Protobuf` items with `GrpcServices="Server"`
- [ ] `{Entity}GrpcService.cs` under `/Grpc`
