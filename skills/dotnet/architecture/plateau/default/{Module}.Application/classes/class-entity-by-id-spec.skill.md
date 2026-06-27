---
name: class-entity-by-id-spec
description: Simple single-condition specification to load an entity by Id
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration.skill]]"
---

# Goal
- Provide a reusable named query for loading a single entity by its internal Id

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByIdSpec.cs.create.md|{Entity}ByIdSpec.cs.create]]

# Core Principals
- Inherits `Specification<T>` from `Ardalis.Specification`
- Constructor receives filter parameter and calls `Query.Where(...)`
- Never touches the database — describes what to fetch

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByIdSpec.cs.create.md|{Entity}ByIdSpec.cs.create]]

# Implementation
```csharp
// {Module}.Application/Specifications/{Entity}ByIdSpec.cs
using Ardalis.Specification;

namespace {Module}.Application.Specifications;

public class {Entity}ByIdSpec : Specification<{Entity}>
{
    public {Entity}ByIdSpec(int id)
    {
        Query.Where(e => e.Id == id);
    }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByIdSpec.cs.create.md|{Entity}ByIdSpec.cs.create]]

# Rules
MUST:
	- Inherit `Specification<T>`
	- Live in `/{Module}.Application/Specifications`
	- Be named `{Entity}ByIdSpec`
MUST NOT:
	- Call the database or reference DbContext
	- Contain business logic — filtering only

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByIdSpec.cs.create.md|{Entity}ByIdSpec.cs.create]]

# Check list
- [ ] Inherits `Specification<{Entity}>`
- [ ] Constructor applies `Query.Where(e => e.Id == id)`
- [ ] Lives in Application Specifications folder

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByIdSpec.cs.create.md|{Entity}ByIdSpec.cs.create]]

# Unittest TestCases
- [ ] WHEN component is requested THEN it provide a reusable named query for loading a single entity by its internal Id
- [ ] WHEN applied THEN Inherits Specification<T> from Ardalis.Specification
- [ ] WHEN applied THEN Constructor receives filter parameter and calls Query.Where(...)
- [ ] WHEN applied THEN Never touches the database — describes what to fetch
- [ ] WHEN verified THEN Inherits Specification<{Entity}>
- [ ] WHEN verified THEN Constructor applies Query.Where(e => e.Id == id)
- [ ] WHEN verified THEN Lives in Application Specifications folder

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByIdSpec.cs.create.md|{Entity}ByIdSpec.cs.create]]
