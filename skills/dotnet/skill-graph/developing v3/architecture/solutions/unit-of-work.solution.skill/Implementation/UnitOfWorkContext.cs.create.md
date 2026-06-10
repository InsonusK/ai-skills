---
description: Scoped nesting depth counter preventing premature sub-command commit
project_name: BuildingBlocks
name: UnitOfWorkContext.cs
change_kind: create
---

# Goals
- Track the nesting depth of active command pipeline invocations within a single request scope
- Allow `UnitOfWorkBehavior` to determine whether it is the outermost command (`Depth == 1`) and therefore responsible for committing

# Core Principles
- Plain class — no interfaces, no base classes, no infrastructure dependencies
- Single mutable integer property — `Depth`
- Registered as `Scoped` — one instance shared across all nested `_mediator.Send()` calls within the same HTTP request
- Never used directly in handlers — only `UnitOfWorkBehavior` reads and writes this

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Nesting depth tracker | `UnitOfWorkContext` | `UnitOfWorkContext` | `UnitOfWorkContext.cs` | `UnitOfWorkContext.cs` |

# Implementation changes

```csharp
// BuildingBlocks/MediatR/UnitOfWorkContext.cs
namespace BuildingBlocks.MediatR;

public class UnitOfWorkContext
{
    public int Depth { get; set; }
}
```

# Rules

MUST:
- Registered as `Scoped` — never `Singleton` or `Transient`
- Never injected into handlers — only `UnitOfWorkBehavior` uses it

MUST NOT:
- Contain any business logic
- Be used to share state between handlers beyond depth tracking

# Anti-patterns
- `UnitOfWorkContext` registered as `Singleton` — depth leaks across HTTP requests
- `UnitOfWorkContext` registered as `Transient` — nested commands get separate instances, depth never exceeds 1
- Handler directly references `UnitOfWorkContext` — breaks separation of concerns

# Check list
- [ ] `UnitOfWorkContext` is a plain class with single `Depth` property
- [ ] Registered as `Scoped`
- [ ] Never injected into handlers
