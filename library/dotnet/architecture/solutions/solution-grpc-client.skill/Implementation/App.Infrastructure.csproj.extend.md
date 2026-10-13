---
description: Add Protos and Clients folders plus Grpc.Tools client codegen to App.Infrastructure
name: "App.Infrastructure.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/grpc-client
  - element/app-infrastructure-csproj
---

# Goals
- Host the vendored dependency `.proto`s, the generated client stubs, and the `{Dependency}GrpcClient` adapters.

# Implementation changes

**TO BE**:
```
/App.Infrastructure
  /Protos
    {Dependency}.proto
  /Clients
    {Dependency}GrpcClient.cs
    GrpcStatusExtensions.cs
    {Dependency}ClientOptions.cs
```

csproj additions:
```xml
<ItemGroup>
  <PackageReference Include="Grpc.Net.ClientFactory" />
  <PackageReference Include="Google.Protobuf" />
  <PackageReference Include="Grpc.Tools" PrivateAssets="all" />
  <PackageReference Include="Ardalis.Result" />
  <PackageReference Include="Microsoft.Extensions.Http.Resilience" />
</ItemGroup>

<ItemGroup>
  <Protobuf Include="Protos\{Dependency}.proto" GrpcServices="Client" />
</ItemGroup>
```

# NuGet Packages
| Package | Version constraint | Purpose |
| ------- | ------------------ | ------- |
| Grpc.Net.ClientFactory | central | DI-managed gRPC channels |
| Google.Protobuf / Grpc.Tools | central | `.proto` → C# client stub at build |
| Microsoft.Extensions.Http.Resilience | central | standard resilience handler on the channel |
| Ardalis.Result | central | `Result<T>` in the adapters |

# Allowed Dependencies
- `Shared` (for `I{Dependency}Client`)
- the gRPC + resilience NuGet packages

# Rules

## MUST
- Add each dependency `.proto` as `<Protobuf … GrpcServices="Client" />` — never `Both` or `Server`.
  - Risk: `Server`/`Both` generates a service base class this project has no use for and pulls in `Grpc.AspNetCore`.
  - Fix: `Client` only; the inbound service base (`solution-grpc-integration`) is a different project.
- Keep `App.Infrastructure` the only project that references `Grpc.Net.ClientFactory` / a generated client type.
  - Risk: a module or `App.Host` referencing the stub bypasses the adapter.
  - Fix: `App.Host` only calls `AddGrpcClient<{Dependency}GrpcClient>()`; modules see `I{Dependency}Client`.

# Check list
- [ ] `/Protos` + `/Clients` folders exist; each `.proto` is `GrpcServices="Client"`.
- [ ] Only `App.Infrastructure` references the gRPC client packages / generated types.
