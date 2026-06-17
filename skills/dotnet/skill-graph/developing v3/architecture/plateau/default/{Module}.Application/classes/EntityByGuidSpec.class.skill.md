---
uid: 69ac0f34-0e68-4468-8b2f-79200a6ba6b1
name: entitybyguidspec-class
description: Specification for looking up entity by Guid
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity.solution.skill]]"
---

# Goal
- Add `{Entity}ByGuidSpec` as a required Application spec for every external-created entity type
- Used by `GuidResolver` to look up the entity by its client-generated Guid

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create.md|{Entity}ByGuidSpec.cs.create]]

# Core Principals
- Single-condition spec — filters by `Guid` property only
- Lives in `{Module}.Application/Specifications` — reusable across resolver and any feature that needs Guid lookup
- Follows naming convention from repository-integration.solution.skill: `{Entity}ByGuidSpec`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create.md|{Entity}ByGuidSpec.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Guid lookup spec | `{Entity}ByGuidSpec` | `TaskByGuidSpec` | `{Entity}ByGuidSpec.cs` | `TaskByGuidSpec.cs` |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create.md|{Entity}ByGuidSpec.cs.create]]

# Implementation
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

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create.md|{Entity}ByGuidSpec.cs.create]]

# Rules
MUST:
	- Every external-created entity type has a `{Entity}ByGuidSpec` in `/{Module}.Application/Specifications`
	- Used only by `GuidResolver` and any feature that explicitly needs Guid-based lookup
MUST NOT:
	- Placed in Domain — all specs belong in Application per repository-integration.solution.skill

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create.md|{Entity}ByGuidSpec.cs.create]]

# Anti-patterns
- Inline LINQ in resolver instead of named spec

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create.md|{Entity}ByGuidSpec.cs.create]]

# Check list
- [ ] `{Entity}ByGuidSpec` in `/{Module}.Application/Specifications`
- [ ] Filters by `Guid` property only

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create.md|{Entity}ByGuidSpec.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Add {Entity}ByGuidSpec as a required Application spec for every external-created entity type
- [ ] WHEN applied THEN Used by GuidResolver to look up the entity by its client-generated Guid
- [ ] WHEN applied THEN Single-condition spec — filters by Guid property only
- [ ] WHEN applied THEN Lives in {Module}.Application/Specifications — reusable across resolver and any feature that needs Guid lookup
- [ ] WHEN applied THEN Follows naming convention from repository-integration.solution.skill: {Entity}ByGuidSpec
- [ ] WHEN verified THEN {Entity}ByGuidSpec in /{Module}.Application/Specifications
- [ ] WHEN verified THEN Filters by Guid property only
- [ ] WHEN naming 'Guid lookup spec' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create.md|{Entity}ByGuidSpec.cs.create]]
