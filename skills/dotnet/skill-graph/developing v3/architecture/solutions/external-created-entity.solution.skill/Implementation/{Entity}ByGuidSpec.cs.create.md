---
description: Specification for looking up entity by Guid
name: "{Entity}ByGuidSpec.cs"
change_kind: create
---

# Goals
- Add `{Entity}ByGuidSpec` as a required Application spec for every external-created entity type
- Used by `GuidResolver` to look up the entity by its client-generated Guid

# Core Principles
- Single-condition spec — filters by `Guid` property only
- Lives in `{Module}.Application/Specifications` — reusable across resolver and any feature that needs Guid lookup
- Follows naming convention from repository-integration.solution.skill: `{Entity}ByGuidSpec`

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Guid lookup spec | `{Entity}ByGuidSpec` | `TaskByGuidSpec` | `{Entity}ByGuidSpec.cs` | `TaskByGuidSpec.cs` |

# Implementation changes

```csharp
// {Module}.Application/Specifications/{Entity}ByGuidSpec.cs
public class {Entity}ByGuidSpec : Specification<{EntityName}>
{
    public {Entity}ByGuidSpec(Guid guid)
    {
        Query.Where(t => t.Guid == guid);
    }
}
```

# Rules

MUST:
- Every external-created entity type has a `{Entity}ByGuidSpec` in `/{Module}.Application/Specifications`
- Used only by `GuidResolver` and any feature that explicitly needs Guid-based lookup

MUST NOT:
- Placed in Domain — all specs belong in Application per repository-integration.solution.skill

# Anti-patterns
- Inline LINQ in resolver instead of named spec

# Check list
- [ ] `{Entity}ByGuidSpec` in `/{Module}.Application/Specifications`
- [ ] Filters by `Guid` property only
