---
description: Add a Clients folder to Shared for per-dependency outbound-call contracts
name: "Shared.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/grpc-client
  - element/shared-csproj
---

# Goals
- Give the module one place for the narrow `I{Dependency}Client` contracts a handler injects — decoupled from the transport (gRPC here) and from `App.Infrastructure`.

# Implementation changes

**AS IS** (after `solution-sln-structure` + earlier solutions): `Shared` holds `/MediatR`, `/Exceptions`, and (with persistence) `/Repositories` etc.

**TO BE**: add `/Clients` — one `I{Dependency}Client.cs` per external dependency the module calls.
```
/Shared
  /Clients
    I{Dependency}Client.cs
```
Allowed Dependencies: `Ardalis.Result` (already referenced) — the contract methods return `Result<T>`. No new project reference.

# NuGet Packages
| Package | Version constraint | Purpose |
| ------- | ------------------ | ------- |
| Ardalis.Result | central (already present) | `Result<T>` return type |

# Rules

## MUST
- Keep `/Clients` interfaces free of any gRPC / `Grpc.*` / generated type.
  - Risk: a `Grpc.Core` type on the contract forces every consumer (and `Shared` itself) to reference the gRPC stack.
  - Fix: the interface speaks only DTOs + `Result<T>`; the gRPC-ness lives in `App.Infrastructure/Clients`.

# Check list
- [ ] `Shared/Clients/I{Dependency}Client.cs` exists, no `Grpc.*` reference.
