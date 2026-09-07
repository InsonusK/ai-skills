---
description: Specification for looking up entity by Guid
project_name: "{Module}.Application"
name: "{Entity}ByGuidSpec.cs"
element_kind: class
change_kind: create
tags:
  - solution/external-created-entity
  - element/entity-byguidspec-cs
---

# Goals
- Add `{Entity}ByGuidSpec` as a required Application spec for every external-created entity type
- Used by `GuidResolver` to look up the entity by its client-generated Guid

# Core Principles
- Single-condition spec — filters by `Guid` property only
- Lives in `{Module}.Application/Specifications` — reusable across resolver and any feature that needs Guid lookup
- Follows naming convention from solution-repository-integration.skill: `{Entity}ByGuidSpec`

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Guid lookup spec | `{Entity}ByGuidSpec` | `TaskByGuidSpec` | `{Entity}ByGuidSpec.cs` | `TaskByGuidSpec.cs` |

# Implementation changes

```csharp
// {Module}.Application/Specifications/{Entity}ByGuidSpec.cs
public class {Entity}ByGuidSpec : Specification<{Entity}>
{
    public {Entity}ByGuidSpec(Guid guid)
    {
        Query.Where(t => t.Guid == guid);
    }
}
```

# Rule changes

## MUST
- Every external-created entity type has a `{Entity}ByGuidSpec` in `/{Module}.Application/Specifications`
- Used only by `GuidResolver` and any feature that explicitly needs Guid-based lookup
- `{Entity}ByGuidSpec` defined in `/{Module}.Application/Specifications`
- Never placed in Domain — all specs belong in Application per solution-repository-integration.skill

## SHOULD
- Avoid inline LINQ in resolver instead of named spec

# Check list
- [ ] `{Entity}ByGuidSpec` in `/{Module}.Application/Specifications`
- [ ] Filters by `Guid` property only

# Unittest TestCases
- [ ] WHEN applied THEN Add {Entity}ByGuidSpec as a required Application spec for every external-created entity type
- [ ] WHEN applied THEN Used by GuidResolver to look up the entity by its client-generated Guid
- [ ] WHEN applied THEN Single-condition spec — filters by Guid property only
- [ ] WHEN applied THEN Lives in {Module}.Application/Specifications — reusable across resolver and any feature that needs Guid lookup
- [ ] WHEN applied THEN Follows naming convention from solution-repository-integration.skill: {Entity}ByGuidSpec
- [ ] WHEN verified THEN {Entity}ByGuidSpec in /{Module}.Application/Specifications
- [ ] WHEN verified THEN Filters by Guid property only
- [ ] WHEN naming 'Guid lookup spec' THEN pattern matches convention
