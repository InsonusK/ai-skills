---
description: Thrown when an Entity method needs a navigation the caller did not load — a Handler defect, not invalid input
project_name: "Shared"
name: "EntityNotLoadedException.cs"
element_kind: class
change_kind: create
tags:
  - solution/domain-rules
  - element/entity-not-loaded-exception-cs
---

# Goals
- Give an Entity a way to fail loudly and specifically when a required navigation is missing, without conflating it with `DomainException`

# Implementation changes

```csharp
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
    // Не DomainException: сам Transaction валиден, сам запрос мог быть валиден —
    // это вызывающий код не прогрузил то, что было нужно для операции.
    if (Account is null)
        throw new EntityNotLoadedException(nameof(Transaction), nameof(Account));

    Account.Withdraw(newAmount - Amount);
    Amount = newAmount;
}
```

# Rule changes

## MUST
- Carry the entity name and the missing navigation name in the message
- Be thrown only when a required navigation is missing, never for any other failure
- Never be caught and treated as `DomainException` anywhere in the pipeline — it must map to `500` + critical log, since it signals a caller defect, not invalid input

# Check list
- [ ] `EntityNotLoadedException` exists in `Shared.Exceptions`
- [ ] Every Entity method requiring a preloaded navigation throws it when that navigation is `null`
- [ ] It is mapped to `500` + critical log, never to the same path as `DomainException`

# Unittest TestCases
- [ ] WHEN an Entity method requires a navigation that is null THEN it throws EntityNotLoadedException, not DomainException
- [ ] WHEN EntityNotLoadedException reaches the API layer THEN it maps to 500, not a 4xx client error
