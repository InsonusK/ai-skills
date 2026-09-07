---
description: Add IHasGuid to create commands for externally-created entities
name: "{Module}.Interfaces.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/external-created-entity
  - element/module-interfaces-csproj
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

# Rules

## MUST
- `Guid` is the first property on the command record
- Command implements both `ICommand<Result<T>>` and `IHasGuid`
- `Guid` typed as `System.Guid` — never `string` or `int`
- Never reference BuildingBlocks — commands implement interfaces from Shared only
- Never update, delete, or internal-create commands implement `IHasGuid`

## SHOULD
- Avoid `Guid` not as first property — signals external-created entity at a glance
- Avoid `{Module}.Interfaces` referencing BuildingBlocks just to implement `IHasGuid`

# Check list
- [ ] Create command implements `IHasGuid`
- [ ] `Guid` is first property
