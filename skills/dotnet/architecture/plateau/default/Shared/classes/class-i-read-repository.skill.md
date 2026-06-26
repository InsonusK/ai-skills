---
uid: f04c4d22-1b0e-41db-9a42-28c3502ac03f
name: class-i-read-repository
description: Read-only repository contract inheriting Ardalis IReadRepositoryBase<T>
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration.skill]]"
---

# Goal
- Provide a read-only data access contract for query handlers and idempotency checks
- Signal read intent at the type level

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md|IReadRepository.cs.create]]

# Core Principals
- All read methods are inherited from `IReadRepositoryBase<T>` — no custom signatures needed
- The interface exists so that Application layers depend on our contract, not directly on Ardalis

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md|IReadRepository.cs.create]]

# Implementation
```csharp
// Shared/Repositories/IReadRepository.cs
using Ardalis.Specification;

namespace Shared.Repositories;

public interface IReadRepository<T> : IReadRepositoryBase<T>
    where T : class
{
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md|IReadRepository.cs.create]]

# Rules
MUST:
	- Inherit `IReadRepositoryBase<T>` from `Ardalis.Specification`
	- Generic constraint `where T : class`
	- Live in `/Shared/Repositories`
MUST NOT:
	- Add write or commit methods

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md|IReadRepository.cs.create]]

# Check list
- [ ] Inherits `IReadRepositoryBase<T>`
- [ ] `where T : class` constraint present
- [ ] No write methods declared

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md|IReadRepository.cs.create]]

# Unittest TestCases
- [ ] WHEN component is requested THEN it provide a read-only data access contract for query handlers and idempotency checks
- [ ] WHEN applied THEN Signal read intent at the type level
- [ ] WHEN applied THEN All read methods are inherited from IReadRepositoryBase<T> — no custom signatures needed
- [ ] WHEN applied THEN The interface exists so that Application layers depend on our contract, not directly on Ardalis
- [ ] WHEN verified THEN Inherits IReadRepositoryBase<T>
- [ ] WHEN verified THEN where T : class constraint present
- [ ] WHEN verified THEN No write methods declared

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md|IReadRepository.cs.create]]
