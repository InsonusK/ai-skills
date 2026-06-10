---
description: Read-only repository contract inheriting Ardalis IReadRepositoryBase<T>
name: IReadRepository.cs
change_kind: create
---

# Goals
- Provide a read-only data access contract for query handlers and idempotency checks
- Signal read intent at the type level

# Core Principals
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

# Rules

MUST:
- Inherit `IReadRepositoryBase<T>` from `Ardalis.Specification`
- Generic constraint `where T : class`
- Live in `/Shared/Repositories`

MUST NOT:
- Add write or commit methods

# Check list
- [ ] Inherits `IReadRepositoryBase<T>`
- [ ] `where T : class` constraint present
- [ ] No write methods declared
