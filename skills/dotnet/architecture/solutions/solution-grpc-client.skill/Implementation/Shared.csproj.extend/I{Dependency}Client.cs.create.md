---
description: Narrow per-dependency contract a handler injects to call an external service — one method per operation, each returning Result<T>
project_name: "Shared"
name: "I{Dependency}Client.cs"
element_kind: class
change_kind: create
tags:
  - solution/grpc-client
  - element/i-dependency-client-cs
---

# Goals
- Expose exactly the operations this module calls on `{Dependency}`, transport-agnostic, each returning `Result<T>`.

# Naming convention
| use case | interface | file |
| --- | --- | --- |
| Outbound dependency contract | `I{Dependency}Client` (e.g. `IPricingClient`) | `I{Dependency}Client.cs` |

# Implementation changes

```csharp
// Shared/Clients/IPricingClient.cs
using Ardalis.Result;

namespace Shared.Clients;

public interface IPricingClient
{
    Task<Result<PriceDto>> GetPrice(string sku, CancellationToken ct);
    Task<Result<IReadOnlyList<PriceDto>>> ListPrices(IReadOnlyList<string> skus, CancellationToken ct);
}

public sealed record PriceDto(string Sku, decimal Amount, string Currency);
```

- One method per operation the module actually calls — not a 1:1 mirror of the dependency's whole `.proto`.
- DTOs are plain records defined next to the interface (or reused from `{Module}.Interfaces` if the shape is already public).

# Rules

## MUST
- Return `Result<T>` (or `Result`) from every method — never a raw DTO, never `Task<T>` that can throw for a remote failure.
  - Risk: a throwing contract makes every call site wrap a try/catch and the handler cannot branch cleanly on "not found" vs "unavailable".
  - Fix: the adapter converts every outcome to a `Result`.
- Reference nothing but the BCL and `Ardalis.Result`.
  - Risk: a `Grpc.Core` or `App.Infrastructure` type here couples `Shared` to the transport.
  - Fix: DTOs + `Result<T>` only.

# Check list
- [ ] Every method returns `Result<T>` / `Result`.
- [ ] Only the operations the module calls are declared.
- [ ] No `Grpc.*` / infrastructure reference.
