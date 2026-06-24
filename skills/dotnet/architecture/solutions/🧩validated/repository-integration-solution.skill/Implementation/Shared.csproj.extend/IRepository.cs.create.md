---
description: Read-write repository contract inheriting Ardalis IRepositoryBase<T>
project_name: Shared
name: IRepository.cs
element_kind: class
change_kind: create
---

# Goals
- Provide a write-staging contract for command handlers
- Extend `IReadRepository<T>` so command handlers have both read and write access through one injection

# Core Principals
- All write methods are inherited from `IRepositoryBase<T>` — no custom signatures needed
- `SaveChangesAsync` is intentionally absent — committing is the responsibility of the Unit of Work

# Structure

## Project Structure
```
/Shared
  /Repositories
    IRepository.cs
```

# Implementation changes

```csharp
// Shared/Repositories/IRepository.cs
using Ardalis.Specification;

namespace Shared.Repositories;

public interface IRepository<T> : IRepositoryBase<T>, IReadRepository<T>
    where T : class
{
}
```

# Rules

MUST:
- Inherit `IRepositoryBase<T>` from `Ardalis.Specification`
- Inherit `IReadRepository<T>` from Shared
- Generic constraint `where T : class`
- Live in `/Shared/Repositories`

MUST NOT:
- Expose `SaveChangesAsync`

# Check list
- [ ] Inherits `IRepositoryBase<T>`
- [ ] Inherits `IReadRepository<T>`
- [ ] `where T : class` constraint present
- [ ] No `SaveChangesAsync` declared

# Unittest TestCases
- [ ] WHEN component is requested THEN it provide a write-staging contract for command handlers
- [ ] WHEN applied THEN Extend IReadRepository<T> so command handlers have both read and write access through one injection
- [ ] WHEN applied THEN All write methods are inherited from IRepositoryBase<T> — no custom signatures needed
- [ ] WHEN applied THEN SaveChangesAsync is intentionally absent — committing is the responsibility of the Unit of Work
- [ ] WHEN verified THEN Inherits IRepositoryBase<T>
- [ ] WHEN verified THEN Inherits IReadRepository<T>
- [ ] WHEN verified THEN where T : class constraint present
- [ ] WHEN verified THEN No SaveChangesAsync declared
