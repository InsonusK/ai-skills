---
name: class-i-repository
description: Classes IRepository/IReadRepository in the statefull-service plateau
whenToUse: when a handler needs to load, add, or stage a persisted entity
domain: skill
type: template
plateau: statefull-service
version: 20260824100000
tags:
  - skill/template/class
  - plateau/statefull-service
created_by:
  - "[[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
---

# Goal
- Decouple Application handlers from EF Core/`DbContext` behind thin wrappers over Ardalis's `IReadRepositoryBase<T>`/`IRepositoryBase<T>`

# Core Principles
- `IReadRepository<T>` is strictly read-only; `IRepository<T>` stages changes but never commits — `SaveChangesAsync` is intentionally absent from both
- All read methods accept `ISpecification<T>` — no raw LINQ parameters

# Implementation
```csharp
//Skill: class-i-repository
//Plateau: statefull-service
//Version: 20260824100000

public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class { }

public interface IRepository<T> : IRepositoryBase<T>, IReadRepository<T> where T : class { }
```

# Rules
MUST:
- Live in `Shared/Repositories`
- `IRepository<T>` never exposes a commit method
MUST NOT:
- Add methods beyond what the Ardalis base interfaces already provide

# Check list
- [ ] `IReadRepository<T>`/`IRepository<T>` defined in `Shared/Repositories`
- [ ] Neither interface exposes `SaveChangesAsync`

__Applied solutions:__
- [[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../../solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md|IReadRepository.cs.create]], [[../../../../../solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IRepository.cs.create.md|IRepository.cs.create]]
