---
name: class-unit-of-work-context
description: Scoped nesting depth counter preventing premature sub-command commit
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work.skill]]"
---

# Goal
- Track the nesting depth of active command pipeline invocations within a single request scope
- Allow `UnitOfWorkBehavior` to determine whether it is the outermost command (`Depth == 1`) and therefore responsible for committing

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkContext.cs.create.md|UnitOfWorkContext.cs.create]]

# Core Principals
- Apply ONE plateau template per class
- Plain class — no interfaces, no base classes, no infrastructure dependencies
- Single mutable integer property — `Depth`
- Registered as `Scoped` — one instance shared across all nested `_mediator.Send()` calls within the same HTTP request
- Never used directly in handlers — only `UnitOfWorkBehavior` reads and writes this

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkContext.cs.create.md|UnitOfWorkContext.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Nesting depth tracker | `UnitOfWorkContext` | `UnitOfWorkContext` | `UnitOfWorkContext.cs` | `UnitOfWorkContext.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkContext.cs.create.md|UnitOfWorkContext.cs.create]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-unit-of-work-context
//Plateau: default
//Version: 20260628
```

```csharp
// BuildingBlocks/MediatR/UnitOfWorkContext.cs
namespace BuildingBlocks.MediatR;

public class UnitOfWorkContext
{
    public int Depth { get; set; }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkContext.cs.create.md|UnitOfWorkContext.cs.create]]

# Rules
MUST:
	- Registered as `Scoped` — never `Singleton` or `Transient`
	- Never injected into handlers — only `UnitOfWorkBehavior` uses it
MUST NOT:
	- Contain any business logic
	- Be used to share state between handlers beyond depth tracking

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkContext.cs.create.md|UnitOfWorkContext.cs.create]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- `UnitOfWorkContext` registered as `Singleton` — depth leaks across HTTP requests
- `UnitOfWorkContext` registered as `Transient` — nested commands get separate instances, depth never exceeds 1
- Handler directly references `UnitOfWorkContext` — breaks separation of concerns

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkContext.cs.create.md|UnitOfWorkContext.cs.create]]

# Check list
- [ ] `UnitOfWorkContext` is a plain class with single `Depth` property
- [ ] Registered as `Scoped`
- [ ] Never injected into handlers

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkContext.cs.create.md|UnitOfWorkContext.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Track the nesting depth of active command pipeline invocations within a single request scope
- [ ] WHEN applied THEN Allow UnitOfWorkBehavior to determine whether it is the outermost command (Depth == 1) and therefore responsible for committing
- [ ] WHEN applied THEN Plain class — no interfaces, no base classes, no infrastructure dependencies
- [ ] WHEN applied THEN Single mutable integer property — Depth
- [ ] WHEN applied THEN Registered as Scoped — one instance shared across all nested _mediator.Send() calls within the same HTTP request
- [ ] WHEN applied THEN Never used directly in handlers — only UnitOfWorkBehavior reads and writes this
- [ ] WHEN verified THEN UnitOfWorkContext is a plain class with single Depth property
- [ ] WHEN verified THEN Registered as Scoped
- [ ] WHEN verified THEN Never injected into handlers
- [ ] WHEN naming 'Nesting depth tracker' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill.md|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkContext.cs.create.md|UnitOfWorkContext.cs.create]]
