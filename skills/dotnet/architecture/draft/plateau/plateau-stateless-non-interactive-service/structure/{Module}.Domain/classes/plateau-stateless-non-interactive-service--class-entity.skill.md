---
name: class-entity
description: Class {Entity} in the stateless-non-interactive-service plateau
whenToUse: when creating or editing an entity in {Module}.Domain, or picking the right entity type from the classification matrix
domain: skill
type: template
plateau: stateless-non-interactive-service
version: 20260821120000
tags:
  - skill/template/class
  - plateau/stateless-non-interactive-service
created_by:
  - "[[../../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
---

# Goal
- Represent a domain object with stable identity, mutable state, encapsulated behavior, and invariant enforcement
- Select the correct entity type from the type matrix before implementation
- Ensure every entity is assigned to exactly one type so the correct set of patterns is applied
- Prevent invalid state by enforcing that all entity properties are accessible only through controlled access modifiers

__Applied solutions:__
- [[../../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]

# Core Principles
- Apply ONE plateau template per class
- Entity has stable identity — `int Id` is always the system primary identity
- Entity has mutable state — unlike Value Objects, state changes over time
- Entity encapsulates behavior — state changes happen through methods, not direct property assignment from outside
- Entity enforces invariants — invalid state must never be reachable
- `Id` is always `internal set` — only persistence layer assigns it, never application code
- Entity type is selected from the type matrix before implementation begins — not discovered during coding

__Applied solutions:__
- [[../../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Entity   | {EntityName}       | Order      | {EntityName}.cs   | Order.cs  |

# Implementation
```csharp
//Skill: class-entity
//Plateau: stateless-non-interactive-service
//Version: 20260821120000

public class Currency
{
    public int Id { get; internal set; }
    private string _code;
    public string Code
    {
        get => _code;
        set
        {
            if (value == "")
                throw new DomainException("Invalid code");
            _code = value;
        }
    }

    public int Amount { get; internal set; }
    internal void SetAmount(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Invalid amount");

        Amount = amount;
    }
}
```

__Applied solutions:__
- [[../../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]

# Rules
MUST:
- Entity has `int Id` with `internal set`
- All public property setters or methods must validate state before assigning
- `Id` used in all domain logic, persistence, relationships, and internal APIs
MUST NOT:
- Use `public` setters on any entity property

__Applied solutions:__
- [[../../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]

# Check list
- [ ] Entity type selected from the matrix
- [ ] `int Id` with `internal set` present
- [ ] All public property setters and methods validate state
- [ ] Entity placed in `/{Module}.Domain/Entities`

__Applied solutions:__
- [[../../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]

# Unittest TestCases
- [ ] WHEN entity created THEN Id is default (0) until persisted

__Applied solutions:__
- [[../../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]
