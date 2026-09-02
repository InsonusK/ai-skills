---
name: plateau-offline-sync-service--class-entity-not-loaded-exception
description: Class EntityNotLoadedException in the plateau-offline-sync-service plateau — thrown when an entity method needs a navigation the handler did not load; distinct from DomainException
whenToUse: when a Domain-classified rule or an entity method requires a navigation property that was not preloaded, or checking the exception is mapped to 500 not confused with DomainException
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Signal a programming error — an entity method reached for a navigation the caller failed to load — as a distinct type from `DomainException` (an expected invariant violation), so the exception behavior maps it to a generic 500, never to a client-facing domain outcome.

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/Shared.csproj.extend/EntityNotLoadedException.cs.create.md|EntityNotLoadedException.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `sealed class EntityNotLoadedException : Exception` in `Shared.Exceptions`.
- Thrown only from an entity method / domain service when a required navigation is `null` because it was not `Include`d — never for an expected business condition.
- Not caught specially: `ExceptionHandlingBehavior` logs it `Critical` and returns a generic `Result.Error` like any unhandled exception.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-entity-not-loaded-exception
// Plateau: offline-sync-service
// Version: 20260902000000
namespace Shared.Exceptions;

public sealed class EntityNotLoadedException(string entity, string navigation)
    : Exception($"{entity}.{navigation} was required but not loaded.");
```

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/Shared.csproj.extend/EntityNotLoadedException.cs.create.md|EntityNotLoadedException.cs.create]]

# Rules
MUST:
- Be `sealed`, in `Shared.Exceptions`, distinct from `DomainException`.
- Be thrown only for a "required navigation not loaded" case, from the entity / domain-service layer.
- Never be used for an expected business condition (that is `DomainException`); never be caught specially.
- Never apply several plateau templates per class.

# Check list
- [ ] `sealed class EntityNotLoadedException : Exception` in `Shared.Exceptions`.
- [ ] Thrown only for an unloaded-navigation case; the Cecil exception-scoping test enforces the layer.

# Unittest TestCases
- [ ] WHEN an entity method reads an unloaded navigation THEN `EntityNotLoadedException` is thrown, and the exception behavior returns a generic `Result.Error`.
