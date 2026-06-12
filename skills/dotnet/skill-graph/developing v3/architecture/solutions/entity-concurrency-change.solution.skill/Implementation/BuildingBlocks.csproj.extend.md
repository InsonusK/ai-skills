---
description: Add IHasVersions, IEntityVersionResolver, ETagEncoder, and ConcurrencyBehavior
name: BuildingBlocks.csproj
element_kind: project
change_kind: extend
---

# Goals
- Own `IHasVersions`, `IEntityVersionResolver`, `ETagEncoder`, and `ConcurrencyBehavior` — the full client-facing concurrency contract and pipeline enforcement
- Reference `IReadRepository<T>` from Shared for version loading in `ConcurrencyBehavior`

# Core Principles
- `IHasVersions` and `ETagEncoder` live in BuildingBlocks — referenced by both Application and Api layers
- `IEntityVersionResolver` lives in BuildingBlocks — implementation lives in App.Infrastructure
- `ConcurrencyBehavior` constrained on `where TRequest : IHasVersions` — only update commands are checked

# Structure

## Project Structure
```
/BuildingBlocks
  /Concurrency
    IHasVersions.cs
    IEntityVersionResolver.cs
    ETagEncoder.cs
  /MediatR
    ConcurrencyBehavior.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Concurrency/IHasVersions.cs | Interface carried by all update commands |
| /Concurrency/IEntityVersionResolver.cs | Maps string entity name to C# Type |
| /Concurrency/ETagEncoder.cs | Encodes/decodes entity versions as base64 JSON ETag |
| /MediatR/ConcurrencyBehavior.cs | Pipeline behavior validating versions before handler runs |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `System.Text.Json` | latest stable | `JsonSerializer` used in `ETagEncoder` |
| `MediatR` | latest stable | `IPipelineBehavior<TRequest, TResponse>` |

# Allowed Dependencies
- Shared

# Rules

MUST:
- All four components defined in BuildingBlocks
- `ConcurrencyBehavior` constrained on `where TRequest : IHasVersions`
- `ETagEncoder` and `IHasVersions` available to both Application and Api layers via BuildingBlocks reference

MUST NOT:
- Add EF Core dependency to BuildingBlocks

# Anti-patterns
- `ConcurrencyBehavior` constrained on `IRequest<T>` instead of `IHasVersions` — would check all commands including queries

# Check list
- [ ] `IHasVersions` defined in `BuildingBlocks/Concurrency/IHasVersions.cs`
- [ ] `IEntityVersionResolver` defined in `BuildingBlocks/Concurrency/IEntityVersionResolver.cs`
- [ ] `ETagEncoder` defined in `BuildingBlocks/Concurrency/ETagEncoder.cs`
- [ ] `ConcurrencyBehavior` defined in `BuildingBlocks/MediatR/ConcurrencyBehavior.cs`
- [ ] No EF Core reference in BuildingBlocks
