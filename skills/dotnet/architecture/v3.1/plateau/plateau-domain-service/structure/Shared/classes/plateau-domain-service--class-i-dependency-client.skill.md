---
name: plateau-domain-service--class-i-dependency-client
description: Class I{Dependency}Client in the plateau-domain-service plateau — the narrow Shared/Clients contract a handler injects to call an external service, one method per operation, each returning Result<T>
whenToUse: when declaring the contract for an outbound service dependency a handler needs to call, or editing an existing one
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[../../../../../solutions/solution-grpc-client.skill/solution-grpc-client.skill.md|solution-grpc-client]]"
---

# Goal
- Expose exactly the operations a module calls on `{Dependency}`, transport-agnostic, each returning `Result<T>` — so a handler injects `I{Dependency}Client` and branches on `Result` like any other operation.

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-client.skill/solution-grpc-client.skill.md|solution-grpc-client]] - [[../../../../../solutions/solution-grpc-client.skill/Implementation/Shared.csproj.extend/I{Dependency}Client.cs.create.md|I{Dependency}Client.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `interface I{Dependency}Client` in `Shared/Clients`; one method per operation the module actually calls — not a mirror of the dependency's whole `.proto`.
- Every method returns `Result<T>` (or `Result`) — never a raw DTO, never a `Task<T>` that throws for a remote failure.
- DTOs are plain records next to the interface (or reused from `{Module}.Interfaces` when already public).
- References nothing but the BCL and `Ardalis.Result` — no `Grpc.*`, no infrastructure type.

# Implementation
```csharp
// Skill: plateau-domain-service--class-i-dependency-client
// Plateau: domain-service
// Version: 20260902000000
using Ardalis.Result;

namespace Shared.Clients;

public interface IPricingClient
{
    Task<Result<PriceDto>> GetPrice(string sku, CancellationToken ct);
}

public sealed record PriceDto(string Sku, decimal Amount, string Currency);
```

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-client.skill/solution-grpc-client.skill.md|solution-grpc-client]] - [[../../../../../solutions/solution-grpc-client.skill/Implementation/Shared.csproj.extend/I{Dependency}Client.cs.create.md|I{Dependency}Client.cs.create]]

# Rules
MUST:
- In `Shared/Clients`; one method per called operation; every method returns `Result<T>` / `Result`.
- Reference only the BCL and `Ardalis.Result` — no `Grpc.Core`, no `App.Infrastructure` type.
- Never apply several plateau templates per class.

# Check list
- [ ] `I{Dependency}Client` in `Shared/Clients`; every method returns `Result<T>` / `Result`.
- [ ] Only operations the module calls are declared.
- [ ] No `Grpc.*` / infrastructure reference.

# Unittest TestCases
- [ ] WHEN the interface is reflected THEN every method's return type is assignable to `IResult`.
