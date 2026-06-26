---
name: class-domain-exception
description: defines DomainException — the base exception type thrown when a domain invariant is violated
domain: skill
type: class
tags:
  - skill/pattern/class
  - dotnet
  - domain
  - exceptions
triggers:
  - DomainException
  - domain invariant violation
  - throw domain exception
---
# Goal
Define `DomainException`. Thrown by entities, value objects, and domain services when an invariant is violated. Lives in Shared so Domain, Application, and Infrastructure can all catch it without circular dependencies.

# Governed by
- solution-rule-usage.skill.md — domain consumers throw this when a rule returns false
- solution-entity-behavior.skill.md — behavior methods throw this on invariant violation

# Structure
## Place in csproj
Defined in `csproj-shared.skill.md`
```
/Shared
  /Exceptions
    DomainException.cs
```

## Naming convention
```
class name: DomainException
file name: DomainException.cs
```

# Contracts
```csharp
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
```

# Rules
MUST:
- Thrown only by Domain layer — entities, value objects, domain services
- Message describes the violated invariant in business terms
MUST NOT:
- Be thrown by Application, Infrastructure, or Api layers
- Be caught and swallowed silently — let it propagate to exception middleware

# Anti-patterns
- Throwing generic `Exception` from domain — use `DomainException` so middleware can handle it correctly
- Catching `DomainException` in handler — validator should have prevented invalid input reaching the domain

# Relations
- csproj-shared.skill.md — lives here
- solution-rule-usage.skill.md — thrown when domain rule returns false
- solution-entity-behavior.skill.md — thrown in behavior methods
- class-value-object.skill.md — thrown in VO constructors
