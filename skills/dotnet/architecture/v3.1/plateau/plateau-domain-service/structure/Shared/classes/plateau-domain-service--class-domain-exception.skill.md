---
name: plateau-domain-service--class-domain-exception
description: Class DomainException in the plateau-domain-service plateau — the single exception type for an entity invariant violation, carrying a stable machine code
whenToUse: when creating or editing DomainException, or deciding what code an entity guard should throw
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]]"
---

# Goal
- One exception type, carrying a stable machine `Code` plus a human message, thrown whenever a guarded entity method's condition fails. `solution-mediator-exception-handler` maps it to a generic `Result.Error`.

__Applied solutions:__
- [[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../../solutions/solution-domain-behaviour.skill/Implementation/Shared.csproj.extend/DomainException.cs.create.md|DomainException.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `sealed class DomainException : Exception` in `Shared.Exceptions`, with a `string Code { get; }`.
- `Code` convention: `{Module}.{Entity}.{Reason}` (e.g. `Sample.TodoItem.RenameCompleted`).
- No subtypes — the `Code` distinguishes cases; the single type is what the exception behavior recognises.

# Implementation
```csharp
// Skill: plateau-domain-service--class-domain-exception
// Plateau: domain-service
// Version: 20260902000000
namespace Shared.Exceptions;

public sealed class DomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../../solutions/solution-domain-behaviour.skill/Implementation/Shared.csproj.extend/DomainException.cs.create.md|DomainException.cs.create]]

# Rules
MUST:
- Be `sealed` with no subtypes, in `Shared.Exceptions`, exposing a `Code` property.
- Every throw site passes both `code` (in `{Module}.{Entity}.{Reason}` form) and `message`.
- Never apply several plateau templates per class.

# Check list
- [ ] `sealed class DomainException : Exception` in `Shared.Exceptions` with `Code`.
- [ ] Every throw passes `code` + `message`.

# Unittest TestCases
- [ ] WHEN a `DomainException` is thrown THEN `Code` and `Message` are both populated.
