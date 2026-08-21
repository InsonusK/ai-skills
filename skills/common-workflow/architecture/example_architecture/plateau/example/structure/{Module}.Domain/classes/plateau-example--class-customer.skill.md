---
name: class-customer
description: Class Customer in the example plateau
whenToUse: when creating or editing Customer, or creating another entity that plays the same role in a different module
domain: skill
type: template
plateau: example
version: 20260821120000
tags:
  - skill/template/class
  - plateau/example
created_by:
  - "[[../../../../../solutions/solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]]"
---

# Goal
- Make an invalid `Customer` state unreachable: every method that changes state validates first and throws instead of assigning a bad value.

__Applied solutions:__
- [[../../../../../solutions/solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]] - [[../../../../../solutions/solution-entity-invariant.skill/Implementation/{Module}.Domain.csproj.extend/Customer.cs.create.md|Customer.cs.create]]

# Core Principles
- Apply ONE plateau template per class
- A property has exactly one mutation point
- `ChangeEmail` accepts an already-valid `Email` — it re-validates only its own invariant, not the email's format

__Applied solutions:__
- [[../../../../../solutions/solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]] - [[../../../../../solutions/solution-entity-invariant.skill/Implementation/{Module}.Domain.csproj.extend/Customer.cs.create.md|Customer.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Entity | {Entity} | Customer | {Entity}.cs | Customer.cs |

# Implementation
```csharp
//Skill: class-customer
//Plateau: example
//Version: 20260821120000

// {Module}.Domain/Entities/Customer.cs
public class Customer
{
    public int Id { get; internal set; }
    public Email Email { get; private set; }

    public void ChangeEmail(Email newEmail)
    {
        if (newEmail == Email)
            throw new DomainException("{ModuleName}.Customer.EmailUnchanged", "New email must differ from the current one.");

        Email = newEmail;
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]] - [[../../../../../solutions/solution-entity-invariant.skill/Implementation/{Module}.Domain.csproj.extend/Customer.cs.create.md|Customer.cs.create]]

# Rules
MUST:
- Accept `Email`, not `string`, as `ChangeEmail`'s parameter
- Validate before assigning, in every method that changes `Customer` state
MUST NOT:
- Add a second method that assigns `Email`

__Applied solutions:__
- [[../../../../../solutions/solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]] - [[../../../../../solutions/solution-entity-invariant.skill/Implementation/{Module}.Domain.csproj.extend/Customer.cs.create.md|Customer.cs.create]]

# Check list
- [ ] `Customer.Email` has exactly one method that assigns it
- [ ] Calling `ChangeEmail` with the current email throws instead of silently succeeding

__Applied solutions:__
- [[../../../../../solutions/solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]] - [[../../../../../solutions/solution-entity-invariant.skill/Implementation/{Module}.Domain.csproj.extend/Customer.cs.create.md|Customer.cs.create]]
