---
description: Add timestamp contracts to Shared
name: Shared.csproj
element_kind: project
change_kind: extend
---

# Goals
- Host all timestamp-related contracts in one place so every layer can reference them without coupling to BuildingBlocks.
- Provide both read-only and mutable timestamp interfaces so entities can keep setters internal while read models expose getters only.
- Provide a command marker that carries the user-supplied action time.

# Core Principles
- Shared contains only contracts and markers — no implementations.
- Timestamp interfaces are independent of MediatR and EF Core.
- Read-only interfaces are for projections, DTOs, and consumers that must not mutate timestamps.
- Mutable interfaces are implemented by domain entities and are used by handlers and `AppDbContext`.

# Structure

## Project Structure
```
/Shared
  /Timestamps
    ICreationInfoModelReadOnly.cs
    ICreationInfoModel.cs
    IUpdateInfoModelReadOnly.cs
    IUpdateInfoModel.cs
    ICommandWithTimestamp.cs
```

## Directory and class skills
| Directory \ file | Description |
| ----------------- | ----------- |
| /Timestamps | Timestamp contracts |
| ICreationInfoModelReadOnly.cs | Read-only creation timestamp contract |
| ICreationInfoModel.cs | Mutable creation timestamp contract implemented by entities |
| IUpdateInfoModelReadOnly.cs | Read-only update timestamp contract |
| IUpdateInfoModel.cs | Mutable update timestamp contract implemented by entities |
| ICommandWithTimestamp.cs | Command marker carrying `ActionTimeStamp` |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| None | - | Timestamp contracts use only BCL types. |

# Allowed Dependencies
- None — Shared has no project dependencies.

# Rules

## MUST
- All timestamp interfaces and the command marker are placed in `/Shared/Timestamps`.
- Mutable interfaces declare getters and setters.
- Read-only interfaces declare only getters.
- `ICommandWithTimestamp` declares a single `DateTimeOffset ActionTimeStamp { get; }` member.

## MUST NOT
- Add behavior logic or dependencies to Shared timestamp contracts.
- Define timestamp markers in BuildingBlocks or module projects.

## SHOULD
- Place the timestamp command marker in `Shared.Timestamps` to keep it independent of MediatR internals.

# Anti-patterns
- Putting timestamp contracts in `{Module}.Interfaces` — would force cross-module consumers to reference a specific module.
- Adding validation logic to `ICommandWithTimestamp` — interfaces are contracts, not behaviors.

# Check list
- [ ] `/Shared/Timestamps/ICreationInfoModelReadOnly.cs` exists.
- [ ] `/Shared/Timestamps/ICreationInfoModel.cs` exists.
- [ ] `/Shared/Timestamps/IUpdateInfoModelReadOnly.cs` exists.
- [ ] `/Shared/Timestamps/IUpdateInfoModel.cs` exists.
- [ ] `/Shared/Timestamps/ICommandWithTimestamp.cs` exists.
- [ ] Shared references no new packages for this solution.
