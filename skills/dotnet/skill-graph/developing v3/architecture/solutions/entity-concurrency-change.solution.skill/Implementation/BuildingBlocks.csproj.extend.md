---
description: Add ETagEncoder and ConcurrencyBehavior to BuildingBlocks
name: BuildingBlocks.csproj
element_kind: project
change_kind: extend
---

# Goals
- Own `ETagEncoder` and `ConcurrencyBehavior` — the concrete client-facing concurrency helpers and pipeline enforcement
- Reference `IReadRepository<T>`, `IHasVersions`, `IEntityVersionResolver`, and `IVersioned` from Shared for version loading in `ConcurrencyBehavior`

# Core Principles
- `ETagEncoder` lives in BuildingBlocks — referenced by Api layers
- `ConcurrencyBehavior` lives in BuildingBlocks — consumes contracts from Shared
- `ConcurrencyBehavior` constrained on `where TRequest : IHasVersions` — only update commands are checked

# Structure

## Project Structure
```
/BuildingBlocks
  /Concurrency
    ETagEncoder.cs
  /MediatR
    ConcurrencyBehavior.cs
  /Specifications
    EntityByIdSpec.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Concurrency/ETagEncoder.cs | Encodes/decodes entity versions as base64 JSON ETag |
| /MediatR/ConcurrencyBehavior.cs | Pipeline behavior validating versions before handler runs |
| /Specifications/EntityByIdSpec.cs | Generic by-Id spec used by ConcurrencyBehavior at runtime |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `System.Text.Json` | latest stable | `JsonSerializer` used in `ETagEncoder` |
| `MediatR` | latest stable | `IPipelineBehavior<TRequest, TResponse>` |

# Allowed Dependencies
- Shared

# Rules

MUST:
- `ETagEncoder`, `EntityByIdSpec<T>`, and `ConcurrencyBehavior` defined in BuildingBlocks
- `ConcurrencyBehavior` constrained on `where TRequest : IHasVersions`
- `ETagEncoder` available to Api layers via BuildingBlocks reference

MUST NOT:
- Add EF Core dependency to BuildingBlocks
- Define `IHasVersions`, `IEntityVersionResolver`, or `IVersioned` in BuildingBlocks — they belong in Shared

# Anti-patterns
- `ConcurrencyBehavior` constrained on `IRequest<T>` instead of `IHasVersions` — would check all commands including queries
- Defining common concurrency contracts in BuildingBlocks — forces modules to reference BuildingBlocks for contracts

# Check list
- [ ] `ETagEncoder` defined in `BuildingBlocks/Concurrency/ETagEncoder.cs`
- [ ] `EntityByIdSpec<T>` defined in `BuildingBlocks/Specifications/EntityByIdSpec.cs`
- [ ] `ConcurrencyBehavior` defined in `BuildingBlocks/MediatR/ConcurrencyBehavior.cs`
- [ ] No EF Core reference in BuildingBlocks
- [ ] `IHasVersions`, `IEntityVersionResolver`, and `IVersioned` imported from Shared, not defined in BuildingBlocks

# Unittest TestCases
- [ ] WHEN applied THEN Own ETagEncoder, EntityByIdSpec<T>, and ConcurrencyBehavior — the concrete client-facing concurrency helpers and pipeline enforcement
- [ ] WHEN applied THEN Reference IReadRepository<T>, IHasVersions, IEntityVersionResolver, and IVersioned from Shared for version loading in ConcurrencyBehavior
- [ ] WHEN verified THEN ETagEncoder defined in BuildingBlocks/Concurrency/ETagEncoder.cs
- [ ] WHEN verified THEN EntityByIdSpec<T> defined in BuildingBlocks/Specifications/EntityByIdSpec.cs
- [ ] WHEN verified THEN ConcurrencyBehavior defined in BuildingBlocks/MediatR/ConcurrencyBehavior.cs
- [ ] WHEN verified THEN No EF Core reference in BuildingBlocks
