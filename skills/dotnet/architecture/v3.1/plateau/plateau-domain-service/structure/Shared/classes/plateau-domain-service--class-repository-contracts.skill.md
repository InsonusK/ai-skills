---
name: plateau-domain-service--class-repository-contracts
description: Classes IReadRepository<T> / IRepository<T> in the plateau-domain-service plateau — the Shared/Repositories contracts that keep Application handlers off EF Core and DbContext
whenToUse: when creating or editing the repository contracts in Shared/Repositories, or deciding whether a handler needs the read or the write contract
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
---

# Goal
- Give command handlers (`IRepository<T>`) and query handlers / checks (`IReadRepository<T>`) an org-named data-access contract that does not change if the concrete implementation does, and a place to add an org-specific method later without forking Ardalis.

__Applied solutions:__
- [[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../../solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IReadRepository.cs.create.md|IReadRepository.cs.create]]

# Core Principles
- Apply ONE plateau template per class (this covers a two-interface family in `Shared/Repositories`).
- `IReadRepository<T> : IReadRepositoryBase<T> where T : class` — no write or commit methods, ever.
- `IRepository<T> : IRepositoryBase<T>, IReadRepository<T> where T : class` — stages changes in the EF tracker; **no `SaveChangesAsync`** (committing is `IUnitOfWork`'s job).
- All read methods are inherited from the Ardalis base — no custom signatures.
- Command handlers inject `IRepository<T>`; query handlers and `{Feature}Check`s inject `IReadRepository<T>`.

# Implementation
```csharp
// Skill: plateau-domain-service--class-repository-contracts
// Plateau: domain-service
// Version: 20260902000000
using Ardalis.Specification;

namespace Shared.Repositories;

public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class;

public interface IRepository<T> : IRepositoryBase<T>, IReadRepository<T> where T : class;
```

__Applied solutions:__
- [[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../../solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IRepository.cs.create.md|IRepository.cs.create]]

# Rules
MUST:
- Both in `Shared/Repositories`, `where T : class`; `IRepository<T>` inherits `IReadRepository<T>` and the Ardalis write base.
- Never declare `SaveChangesAsync` on either; never add a write method to `IReadRepository<T>`.
- Never apply several plateau templates per class.

# Check list
- [ ] `IReadRepository<T>` inherits `IReadRepositoryBase<T>`, no write methods.
- [ ] `IRepository<T>` inherits `IRepositoryBase<T>` + `IReadRepository<T>`, no `SaveChangesAsync`.

# Unittest TestCases
- [ ] WHEN `IRepository<T>` is reflected THEN it exposes no `SaveChangesAsync` member.
