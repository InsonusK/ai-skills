---
name: class-entity-summary-spec
description: Application-layer projection specification mapping entity to DTO
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]]"
---

# Goal
- Project entity data directly to a DTO inside the query
- Avoid loading full entity when only read data is needed

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}SummarySpec.cs.create|{Entity}SummarySpec.cs]]

# Core Principles
- Apply ONE plateau template per class
- Inherits `Specification<T, TResult>` — second type parameter is the DTO
- Uses `Query.Select(...)` to define projection
- Repository executes projection at the database level

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}SummarySpec.cs.create|{Entity}SummarySpec.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-entity-summary-spec
//Plateau: default
//Version: 20260628
```

```csharp
// {Module}.Application/Specifications/{Entity}SummarySpec.cs
using Ardalis.Specification;

namespace {Module}.Application.Specifications;

public class {Entity}SummarySpec : Specification<{Entity}, {Entity}SummaryDto>
{
    public {Entity}SummarySpec(int ownerId)
    {
        Query
            .Where(e => e.OwnerId == ownerId)
            .Select(e => new {Entity}SummaryDto(e.Id, e.Name, e.Status.ToString()));
    }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}SummarySpec.cs.create|{Entity}SummarySpec.cs]]

# Rules
MUST:
	- Inherit `Specification<T, TResult>`
	- Define projection via `Query.Select(...)`
	- Live in `/{Module}.Application/Specifications`
MUST NOT:
	- Load full entity when only DTO fields needed
	- Define DTO inline in the spec — DTO belongs in Interfaces

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}SummarySpec.cs.create|{Entity}SummarySpec.cs]]

# Check list
- [ ] Inherits `Specification<{Entity}, {Entity}SummaryDto>`
- [ ] `Query.Select(...)` defines projection
- [ ] Lives in Application Specifications folder

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}SummarySpec.cs.create|{Entity}SummarySpec.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN Project entity data directly to a DTO inside the query
- [ ] WHEN applied THEN Avoid loading full entity when only read data is needed
- [ ] WHEN applied THEN Inherits Specification<T, TResult> — second type parameter is the DTO
- [ ] WHEN applied THEN Uses Query.Select(...) to define projection
- [ ] WHEN applied THEN Repository executes projection at the database level
- [ ] WHEN verified THEN Inherits Specification<{Entity}, {Entity}SummaryDto>
- [ ] WHEN verified THEN Query.Select(...) defines projection
- [ ] WHEN verified THEN Lives in Application Specifications folder

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}SummarySpec.cs.create|{Entity}SummarySpec.cs]]
