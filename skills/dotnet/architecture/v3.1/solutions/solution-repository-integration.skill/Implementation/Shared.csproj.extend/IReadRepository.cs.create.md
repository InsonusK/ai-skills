---
description: Read-only repository contract inheriting Ardalis IReadRepositoryBase<T>
project_name: Shared
name: IReadRepository.cs
element_kind: class
change_kind: create
tags:
  - solution/repository-integration
  - element/ireadrepository-cs
---

# Goals
- Provide a read-only data access contract for query handlers and idempotency checks
- Signal read intent at the type level

# Core Principles
- All read methods are inherited from `IReadRepositoryBase<T>` — no custom signatures needed
- This does not isolate the codebase from `Ardalis.Specification` — every `{Entity}ByIdSpec` still inherits Ardalis's own `Specification<T>` directly, so `{Module}.Application` already has a hard dependency on the package regardless of this interface. The actual value is narrower: a stable, org-named type (`IRepository<T>`, matching `IUnitOfWork` and the rest of `Shared`'s naming, instead of Ardalis's own `IRepositoryBase<T>`) that gives constructor injection sites one name that doesn't change if the concrete implementation behind it ever does, and one place to add an org-specific method later without forking Ardalis's interface — not a swappable abstraction over the query mechanism itself

# Structure

## Project Structure
```
/Shared
  /Repositories
    IReadRepository.cs
```

# Implementation changes

```csharp
// Shared/Repositories/IReadRepository.cs
using Ardalis.Specification;

namespace Shared.Repositories;

public interface IReadRepository<T> : IReadRepositoryBase<T>
    where T : class
{
}
```

# Rule changes

## MUST
- Inherit `IReadRepositoryBase<T>` from `Ardalis.Specification`
- Generic constraint `where T : class`
- Live in `/Shared/Repositories`
- Never add write or commit methods

# Check list
- [ ] Inherits `IReadRepositoryBase<T>`
- [ ] `where T : class` constraint present
- [ ] No write methods declared

# Unittest TestCases
- [ ] WHEN component is requested THEN it provide a read-only data access contract for query handlers and idempotency checks
- [ ] WHEN applied THEN Signal read intent at the type level
- [ ] WHEN applied THEN All read methods are inherited from IReadRepositoryBase<T> — no custom signatures needed
- [ ] WHEN applied THEN The interface exists so that Application layers depend on our contract, not directly on Ardalis
- [ ] WHEN verified THEN Inherits IReadRepositoryBase<T>
- [ ] WHEN verified THEN where T : class constraint present
- [ ] WHEN verified THEN No write methods declared
