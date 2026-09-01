---
name: solution-api-project
description: Introduces the {Module}.Api project and its App.Host wiring skeleton — the shared prerequisite for any inbound synchronous API (REST via solution-http-api-publication, gRPC via solution-grpc-integration). Creates the project, the AddModuleApi()/UseModuleApi() extension pair, and the reference rules; adds no endpoints of its own.
whenToUse: when a module first needs an inbound synchronous API surface (HTTP or gRPC) and {Module}.Api does not exist yet, or when reviewing where a module's API project sits in the layer graph and how it is wired into the composition root
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - concern/architecture
  - api
  - module
  - solution/api-project
  - stack/dotnet
creates:
  - "{Module}.Api.csproj"
  - "App.Host.DependencyInjection.ApiRegistration.cs"
extends:
  - "App.Host.csproj"
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]"
adr:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/adr/api-project-shared-by-transports.md|One {Module}.Api project shared by REST and gRPC transports]]"
---

# Goal
- Create `{Module}.Api` the first time a module exposes an inbound synchronous API — it does not exist at the common baseline.
- Provide one `AddModuleApi()` / `UseModuleApi()` extension pair in `App.Host` that the transport solutions extend, so `Program.cs` composes whichever transports are applied.
- Fix the reference rules for the API layer once, for every transport.

# Capabilities
- A `{Module}.Api` project referencing only `{Module}.Interfaces`, `Shared`, and `BuildingBlocks` — never `Domain` or `Application`.
- A single composition-root touch-point (`ApiRegistration`) both [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] and [[skills/dotnet/architecture/v3.1/solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] extend, so a module can serve REST, gRPC, or both over the same `ISender` dispatch.

# Core Principle
- **API is a thin adapter** - the project maps a transport request to a Command/Query, dispatches via `ISender`, maps the `Result` back. No business logic, no domain rules, no persistence.
- **One project, many transports** - REST controllers and gRPC services live in the same `{Module}.Api` project (in `/Controllers` and `/Grpc`); they are not separate projects because they share the same dispatch and the same reference rules.
- **App.Host owns composition** - `ApiRegistration` exposes `AddModuleApi()`/`UseModuleApi()`; the transport solutions add `AddHttpApi()`/`AddGrpcApi()` inside them; `Program.cs` calls only the top-level pair.
- This solution adds the project and the wiring skeleton — **no endpoints**. Endpoints come from the transport solutions.

# Boundaries
- Whether a module has an API at all is VP8/VP9; this solution is their shared prerequisite, applied once before either.
- `{Module}.Api` referencing `{Module}.Interfaces` assumes the MediatR markers exist — hence `depends_on solution-mediator-integration`.
- Outbound API clients (VP10/VP11) are a different concern and do not use this project.

# Adr
- [[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/adr/api-project-shared-by-transports.md|One {Module}.Api project shared by REST and gRPC transports]]
  - Selected variant: one `{Module}.Api` project, transports as folders within it, one `ApiRegistration` extended by each transport solution.

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj]] - hosts `ApiRegistration` and the `AddModuleApi()` call
  - [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj]] - the only module project `{Module}.Api` references
- [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs]] - the markers a controller/service dispatches

NUGET:
- `MediatR` {version} - `ISender` injected into adapters (version in `Directory.Packages.props`)

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/Implementation/{Module}.Api.csproj.create.md|{Module}.Api.csproj]] - create - the module's inbound-API adapter project
- [[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - wire `AddModuleApi()`/`UseModuleApi()`
  - [[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs]] - create - the extension pair the transport solutions extend

# Rule

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/Implementation/{Module}.Api.csproj.create.md#MUST|{Module}.Api.csproj]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-api-project.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md#MUST|ApiRegistration.cs]]
- Reference only `{Module}.Interfaces`, `Shared`, `BuildingBlocks` from `{Module}.Api` — never `Domain` or `Application`.
  - Risk: an adapter reaching into `Application` or `Domain` bypasses `ISender` and puts logic in the transport layer.
  - Fix: the adapter sees only `ICommand`/`IQuery`/DTOs from `{Module}.Interfaces`.
- Add no endpoint in this solution — only the project and the `ApiRegistration` skeleton.
  - Risk: an endpoint here has no transport solution owning its conventions.
  - Fix: `solution-http-api-publication` / `solution-grpc-integration` add endpoints.

# Check list
- [ ] `{Module}.Api.csproj` exists, referencing only `{Module}.Interfaces` + `Shared` + `BuildingBlocks`.
- [ ] `App.Host/DependencyInjection/ApiRegistration.cs` defines `AddModuleApi()` / `UseModuleApi()`.
- [ ] `Program.cs` calls the pair once.
- [ ] No controller, endpoint, or `.proto` added by this solution.
