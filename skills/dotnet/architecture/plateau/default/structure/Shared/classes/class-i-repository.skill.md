---
name: class-i-repository
description: Read-write repository contract inheriting Ardalis IRepositoryBase<T>
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]]"
---

# Goal
- Provide a write-staging contract for command handlers
- Extend `IReadRepository<T>` so command handlers have both read and write access through one injection

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IRepository.cs.create|IRepository.cs]]

# Core Principles
- Apply ONE plateau template per class
- All write methods are inherited from `IRepositoryBase<T>` — no custom signatures needed
- `SaveChangesAsync` is intentionally absent — committing is the responsibility of the Unit of Work

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IRepository.cs.create|IRepository.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-i-repository
//Plateau: default
//Version: 20260628
```

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
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IRepository.cs.create|IRepository.cs]]

# Rules
MUST:
	- Inherit `IRepositoryBase<T>` from `Ardalis.Specification`
	- Inherit `IReadRepository<T>` from Shared
	- Generic constraint `where T : class`
	- Live in `/Shared/Repositories`
MUST NOT:
	- Expose `SaveChangesAsync`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IRepository.cs.create|IRepository.cs]]

# Check list
- [ ] Inherits `IRepositoryBase<T>`
- [ ] Inherits `IReadRepository<T>`
- [ ] `where T : class` constraint present
- [ ] No `SaveChangesAsync` declared

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IRepository.cs.create|IRepository.cs]]

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
- [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IRepository.cs.create|IRepository.cs]]
