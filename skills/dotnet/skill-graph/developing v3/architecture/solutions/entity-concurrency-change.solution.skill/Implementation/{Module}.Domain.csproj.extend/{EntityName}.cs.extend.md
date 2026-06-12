---
description: Add uint Version property with internal set
project_name: "{Module}.Domain"
name: "{EntityName}.cs"
element_kind: class
change_kind: extend
---

# Goals
- Add `Version` as a required property on all mutable entities

# Core Principles
- `Version` is `uint` with `internal set` — never set by application code, only by database
- Present on Internal Mutable and External Mutable entity types — absent on Immutable entities
- Read by `ConcurrencyBehavior` via the entity loaded from the repository — never passed as a domain parameter

# Naming convention
| use case | property name pattern | property name | type |
| --- | --- | --- | --- |
| Concurrency token | `Version` | `Version` | `uint` |

# Implementation changes

Mutable entity must declare `Version`:

```csharp
// {Module}.Domain/Entities/{EntityName}.cs
public class {EntityName}
{
    public int Id { get; internal set; }
    // ... other properties
    public uint Version { get; internal set; }   // ← added by this solution

    internal void SomeDomainMethod() { ... }
}
```

# Rules

MUST:
- All mutable entities have `public uint Version { get; internal set; }`

MUST NOT:
- Immutable entities have `Version` — they are never updated
- Application code assign `Version` — it is controlled exclusively by the database

# Anti-patterns
- `Version` with `public set` — application code must never modify it

# Check list
- [ ] `uint Version { get; internal set; }` present on mutable entity
- [ ] Immutable entities do not have `Version`

# Unittest TestCases
- [ ] WHEN applied THEN Add Version as a required property on all mutable entities
- [ ] WHEN applied THEN Version is uint with internal set — never set by application code, only by database
- [ ] WHEN applied THEN Present on Internal Mutable and External Mutable entity types — absent on Immutable entities
- [ ] WHEN applied THEN Read by ConcurrencyBehavior via the entity loaded from the repository — never passed as a domain parameter
- [ ] WHEN verified THEN uint Version { get; internal set; } present on mutable entity
- [ ] WHEN verified THEN Immutable entities do not have Version
- [ ] WHEN naming 'Concurrency token' THEN pattern matches convention
