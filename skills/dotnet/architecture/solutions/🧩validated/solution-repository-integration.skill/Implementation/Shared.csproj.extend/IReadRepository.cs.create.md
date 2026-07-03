---
description: Read-only repository contract inheriting Ardalis IReadRepositoryBase<T>
project_name: Shared
name: IReadRepository.cs
element_kind: class
change_kind: create
---

# Goals
- Provide a read-only data access contract for query handlers and idempotency checks
- Signal read intent at the type level

# Core Principles
- All read methods are inherited from `IReadRepositoryBase<T>` — no custom signatures needed
- The interface exists so that Application layers depend on our contract, not directly on Ardalis

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

## MUST NOT
- Add write or commit methods

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
