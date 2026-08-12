---
name: class-entity-by-guid-spec
description: Specification for looking up entity by Guid
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
  - stack/dotnet
  - concern/architecture

created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]]"
---

# Goal
- Add `{Entity}ByGuidSpec` as a required Application spec for every external-created entity type
- Used by `GuidResolver` to look up the entity by its client-generated Guid

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create|{Entity}ByGuidSpec.cs]]

# Core Principles
- Apply ONE plateau template per class
- Single-condition spec — filters by `Guid` property only
- Lives in `{Module}.Application/Specifications` — reusable across resolver and any feature that needs Guid lookup
- Follows naming convention from solution-repository-integration.skill: `{Entity}ByGuidSpec`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create|{Entity}ByGuidSpec.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Guid lookup spec | `{Entity}ByGuidSpec` | `TaskByGuidSpec` | `{Entity}ByGuidSpec.cs` | `TaskByGuidSpec.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create|{Entity}ByGuidSpec.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-entity-by-guid-spec
//Plateau: default
//Version: 20260628
```

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
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create|{Entity}ByGuidSpec.cs]]

# Rules
MUST:
	- Every external-created entity type has a `{Entity}ByGuidSpec` in `/{Module}.Application/Specifications`
	- Used only by `GuidResolver` and any feature that explicitly needs Guid-based lookup
MUST NOT:
	- Placed in Domain — all specs belong in Application per solution-repository-integration.skill

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create|{Entity}ByGuidSpec.cs]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- Inline LINQ in resolver instead of named spec

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create|{Entity}ByGuidSpec.cs]]

# Check list
- [ ] `{Entity}ByGuidSpec` in `/{Module}.Application/Specifications`
- [ ] Filters by `Guid` property only

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create|{Entity}ByGuidSpec.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN Add {Entity}ByGuidSpec as a required Application spec for every external-created entity type
- [ ] WHEN applied THEN Used by GuidResolver to look up the entity by its client-generated Guid
- [ ] WHEN applied THEN Single-condition spec — filters by Guid property only
- [ ] WHEN applied THEN Lives in {Module}.Application/Specifications — reusable across resolver and any feature that needs Guid lookup
- [ ] WHEN applied THEN Follows naming convention from solution-repository-integration.skill: {Entity}ByGuidSpec
- [ ] WHEN verified THEN {Entity}ByGuidSpec in /{Module}.Application/Specifications
- [ ] WHEN verified THEN Filters by Guid property only
- [ ] WHEN naming 'Guid lookup spec' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create|{Entity}ByGuidSpec.cs]]
