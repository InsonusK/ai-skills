---
description: Add IHasGuid to create commands for externally-created entities
name: "{Module}.Interfaces.csproj"
change_kind: extend
---

# Goals
- Extend create commands for externally-created entity types with `IHasGuid`

# Structure

## Project Structure
```
/{Module}.Interfaces
  /Commands
    Create{Entity}Command.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Commands/Create{Entity}Command.cs | Create command with Guid and IHasGuid |

# Allowed Dependencies
- Shared
- BuildingBlocks

# Rules

MUST:
- `Guid` is the first property on the command record
- Command implements both `ICommand<Result<T>>` and `IHasGuid`
- `Guid` typed as `System.Guid` — never `string` or `int`

MUST NOT:
- Update, delete, or internal-create commands implement `IHasGuid`

# Anti-patterns
- `Guid` not as first property — signals external-created entity at a glance

# Check list
- [ ] Create command implements `IHasGuid`
- [ ] `Guid` is first property
