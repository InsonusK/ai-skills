---
description: Create the module's inbound-API adapter project
name: "{Module}.Api.csproj"
element_kind: project
change_kind: create
tags:
  - solution/api-project
  - element/module-api-csproj
---

# Goals
- Host every inbound synchronous adapter for the module (REST controllers, gRPC services) as thin MediatR adapters.

# Structure

## Project Structure
```
/src/Modules/{ModuleName}/{ModuleName}.Api
  /Controllers        — added by solution-http-api-publication
  /Grpc               — added by solution-grpc-integration
  /Protos             — added by solution-grpc-integration
  {ModuleName}.Api.csproj
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Controllers | REST endpoints (transport solution) |
| /Grpc, /Protos | gRPC services + contracts (transport solution) |

# NuGet Packages
| Package | Version constraint | Purpose |
| ------- | ------------------ | ------- |
| MediatR | central | `ISender` in adapters |

# What Does NOT Belong Here
- Business logic, domain rules — `{Module}.Domain`.
- Handlers — `{Module}.Application`.
- Persistence — `App.Infrastructure`.

# Allowed Dependencies
- `{Module}.Interfaces` (own module only)
- `Shared`
- `BuildingBlocks`

# Rules

## MUST
- Reference only `{Module}.Interfaces`, `Shared`, `BuildingBlocks`.
  - Risk: a reference to `Domain`/`Application` lets a transport adapter bypass `ISender`.
  - Fix: adapters see only `{Module}.Interfaces` contracts.
- Never inject a repository, `DbContext`, or domain type into an adapter.
  - Risk: persistence and domain logic leak into the transport layer.
  - Fix: dispatch a Command/Query via `ISender`; the handler touches persistence.

# Check list
- [ ] `{Module}.Api.csproj` references only `{Module}.Interfaces` + `Shared` + `BuildingBlocks`.
- [ ] No `Domain`/`Application` reference.
