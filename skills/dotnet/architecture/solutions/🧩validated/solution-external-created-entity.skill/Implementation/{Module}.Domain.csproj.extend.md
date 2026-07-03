---
description: Add Guid property and unique index to externally-created entities
name: "{Module}.Domain.csproj"
element_kind: project
change_kind: extend
---

# Goals
- Add `Guid` as an immutable property on externally-created entities
- Add unique database index on `Guid` as the final idempotency guard

# Core Principles
- `Guid` declared with `internal set` — set once during entity creation factory method, never changed
- Entity creation factory method receives `Guid` as a parameter
- No domain method ever reads `Guid` after creation — only the resolver and the entity factory use it

# Structure

## Project Structure
```
/{Module}.Domain
  /Entities
    {EntityName}.cs
  /Configurations
    {EntityName}Config.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Entities/{EntityName}.cs | External-created entity with Guid property |
| /Configurations/{EntityName}Config.cs | Unique index on Guid configuration |

# Allowed Dependencies
- Shared
- EF Core configuration packages

# Rules

## MUST
- `Guid` declared as `public Guid Guid { get; internal set; }`
- Set exactly once in the entity factory method — never reassigned
- Present on External Immutable and External Mutable entity types only

## MUST NOT
- `Guid` used in domain logic, domain events, or as a foreign key in relationships
- `Guid` reassigned after entity creation
- Internal entity types (no external creation) have `Guid`

# Anti-patterns
- `Guid` with `public set` — application code must never modify it
- `Guid` used in domain method logic — it is a correlation handle only

# Check list
- [ ] `Guid Guid { get; internal set; }` on every external-created entity
- [ ] `Guid` set in entity factory method — never reassigned
