---
description: Add Guid property with internal set
project_name: "{Module}.Domain"
name: "{EntityName}.cs"
element_kind: class
change_kind: extend
---

# Goals
- Add `Guid` as a required immutable property on External Immutable and External Mutable entity types
- Keep `Guid` strictly as a correlation handle — never used in domain logic, domain events, or relationships

# Core Principles
- `Guid` declared with `internal set` — set once during entity creation factory method, never changed
- Entity creation factory method receives `Guid` as a parameter — it is the caller's responsibility to supply the client-generated value
- No domain method ever reads `Guid` after creation — only the resolver and the entity factory use it

# Naming convention
| use case | property name pattern | property name | type |
| --- | --- | --- | --- |
| Correlation Guid | `Guid` | `Guid` | `Guid` |

# Implementation changes
External-created entity must declare `Guid` with `internal set`:

```csharp
// {Module}.Domain/Entities/{EntityName}.cs
public class {EntityName}
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }    // ← added by this solution
    // ... other properties
    public uint Version { get; internal set; }  // ← entity-concurrency-change-solution.skill (if mutable)

    // factory method receives client-generated Guid
    public static {EntityName} Create(Guid guid, /* ... */)
        => new()
        {
            Guid = guid,
            // ...
        };
}
```

# Rules

MUST:
- `Guid` declared as `public Guid Guid { get; internal set; }`
- Set exactly once in the entity factory method — never reassigned
- Present on External Immutable and External Mutable entity types only

MUST NOT:
- `Guid` used in domain logic, domain events, or as a foreign key in relationships
- `Guid` reassigned after entity creation
- Internal entity types (no external creation) have `Guid`

# Anti-patterns
- `Guid` with `public set` — application code must never modify it

# Check list
- [ ] `Guid Guid { get; internal set; }` present on external-created entity
- [ ] `Guid` set in factory method
- [ ] Immutable after creation

# Unittest TestCases
- [ ] WHEN applied THEN Add Guid as a required immutable property on External Immutable and External Mutable entity types
- [ ] WHEN applied THEN Keep Guid strictly as a correlation handle — never used in domain logic, domain events, or relationships
- [ ] WHEN applied THEN Guid declared with internal set — set once during entity creation factory method, never changed
- [ ] WHEN applied THEN Entity creation factory method receives Guid as a parameter — it is the caller's responsibility to supply the client-generated value
- [ ] WHEN applied THEN No domain method ever reads Guid after creation — only the resolver and the entity factory use it
- [ ] WHEN verified THEN Guid Guid { get; internal set; } present on external-created entity
- [ ] WHEN verified THEN Guid set in factory method
- [ ] WHEN verified THEN Immutable after creation
- [ ] WHEN naming 'Correlation Guid' THEN pattern matches convention
