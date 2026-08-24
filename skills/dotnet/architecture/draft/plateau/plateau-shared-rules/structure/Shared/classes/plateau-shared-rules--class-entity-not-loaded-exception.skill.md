---
name: class-entity-not-loaded-exception
description: Class EntityNotLoadedException in the shared-rules plateau
whenToUse: when an Entity method needs a navigation the caller did not load — a Handler defect, not invalid input
domain: skill
type: template
plateau: shared-rules
version: 20260824150000
tags:
  - skill/template/class
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Give an Entity a way to fail loudly and specifically when a required navigation is missing, without conflating it with `DomainException`

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/Shared.csproj.extend/EntityNotLoadedException.cs.create.md|EntityNotLoadedException.cs.create]]

# Core Principles
- The Handler forgot to load a required navigation (a Handler defect) is a fundamentally different failure than "the request itself is invalid" (`DomainException`) — the two never share an HTTP status or a test expectation

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/Shared.csproj.extend/EntityNotLoadedException.cs.create.md|EntityNotLoadedException.cs.create]]

# Implementation
```csharp
//Skill: class-entity-not-loaded-exception
//Plateau: shared-rules
//Version: 20260824150000

namespace Shared.Exceptions;

public sealed class EntityNotLoadedException : Exception
{
    public EntityNotLoadedException(string entityName, string navigationName)
        : base($"{entityName} required navigation '{navigationName}' was not loaded before this operation.")
    {
    }
}
```

Usage — an Entity method that needs a preloaded navigation:

```csharp
public void UpdateAmount(decimal newAmount)
{
    if (Account is null)
        throw new EntityNotLoadedException(nameof(Transaction), nameof(Account));

    Account.Withdraw(newAmount - Amount);
    Amount = newAmount;
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/Shared.csproj.extend/EntityNotLoadedException.cs.create.md|EntityNotLoadedException.cs.create]]

# Rules
MUST:
- Carry the entity name and the missing navigation name in the message
- Be thrown only when a required navigation is missing, never for any other failure
MUST NOT:
- Be caught and treated as `DomainException` anywhere in the pipeline — maps to `500` + critical log

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/Shared.csproj.extend/EntityNotLoadedException.cs.create.md|EntityNotLoadedException.cs.create]]

# Check list
- [ ] Exists in `Shared.Exceptions`
- [ ] Every Entity method requiring a preloaded navigation throws it when that navigation is `null`
- [ ] Mapped to `500` + critical log, never to the same path as `DomainException`

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/Shared.csproj.extend/EntityNotLoadedException.cs.create.md|EntityNotLoadedException.cs.create]]
