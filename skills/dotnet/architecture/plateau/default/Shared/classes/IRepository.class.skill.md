---
uid: ace8c35d-4fa7-43d0-a080-0c97ca2d68cf
name: irepository-class
description: Read-write repository contract inheriting Ardalis IRepositoryBase<T>
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration-solution.skill]]"
---

# Goal
- Provide a write-staging contract for command handlers
- Extend `IReadRepository<T>` so command handlers have both read and write access through one injection

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/Implementation/Shared.csproj.extend/IRepository.cs.create.md|IRepository.cs.create]]

# Core Principals
- All write methods are inherited from `IRepositoryBase<T>` — no custom signatures needed
- `SaveChangesAsync` is intentionally absent — committing is the responsibility of the Unit of Work

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/Implementation/Shared.csproj.extend/IRepository.cs.create.md|IRepository.cs.create]]

# Implementation
```csharp
// Shared/Repositories/IRepository.cs
using Ardalis.Specification;

namespace Shared.Repositories;

public interface IRepository<T> : IRepositoryBase<T>, IReadRepository<T>
    where T : class
{
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/Implementation/Shared.csproj.extend/IRepository.cs.create.md|IRepository.cs.create]]

# Rules
MUST:
	- Inherit `IRepositoryBase<T>` from `Ardalis.Specification`
	- Inherit `IReadRepository<T>` from Shared
	- Generic constraint `where T : class`
	- Live in `/Shared/Repositories`
MUST NOT:
	- Expose `SaveChangesAsync`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/Implementation/Shared.csproj.extend/IRepository.cs.create.md|IRepository.cs.create]]

# Check list
- [ ] Inherits `IRepositoryBase<T>`
- [ ] Inherits `IReadRepository<T>`
- [ ] `where T : class` constraint present
- [ ] No `SaveChangesAsync` declared

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/Implementation/Shared.csproj.extend/IRepository.cs.create.md|IRepository.cs.create]]

# Unittest TestCases
- [ ] WHEN component is requested THEN it provide a write-staging contract for command handlers
- [ ] WHEN applied THEN Extend IReadRepository<T> so command handlers have both read and write access through one injection
- [ ] WHEN applied THEN All write methods are inherited from IRepositoryBase<T> — no custom signatures needed
- [ ] WHEN applied THEN SaveChangesAsync is intentionally absent — committing is the responsibility of the Unit of Work
- [ ] WHEN verified THEN Inherits IRepositoryBase<T>
- [ ] WHEN verified THEN Inherits IReadRepository<T>
- [ ] WHEN verified THEN where T : class constraint present
- [ ] WHEN verified THEN No SaveChangesAsync declared

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/Implementation/Shared.csproj.extend/IRepository.cs.create.md|IRepository.cs.create]]
