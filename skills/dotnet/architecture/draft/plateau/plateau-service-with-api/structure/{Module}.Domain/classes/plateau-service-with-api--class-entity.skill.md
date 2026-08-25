---
name: class-entity
description: Class {Entity} in the service-with-api plateau
whenToUse: when creating or editing an entity in {Module}.Domain, or picking the right entity type from the classification matrix
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/class
  - plateau/service-with-api
created_by:
  - "[[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]]"
  - "[[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]]"
---

# Goal
- Represent a domain object with stable identity, mutable state, encapsulated behavior, and invariant enforcement
- Select the correct entity type from the type matrix before implementation
- Ensure every entity is assigned to exactly one type so the correct set of patterns is applied
- Prevent invalid state by enforcing that all entity properties are accessible only through controlled access modifiers
- Encapsulate invariant state on properties into dedicated Value Object types instead of validating primitives inline
- Extract bulky or multi-step behavior into static domain services while the entity stays the gatekeeper of its own state

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]

# Core Principles
- Apply ONE plateau template per class
- Entity has stable identity — `int Id` is always the system primary identity
- Entity has mutable state — unlike Value Objects, state changes over time
- Entity encapsulates behavior — state changes happen through methods, not direct property assignment from outside
- Entity enforces invariants — invalid state must never be reachable
- `Id` is always `internal set` — only persistence layer assigns it, never application code
- Entity type is selected from the type matrix before implementation begins — not discovered during coding
- A property that carries invariant state or business semantics is typed as a Value Object, not a primitive — the entity's own code stays free of inline validation for that property
- Every behavior method validates via a condition it owns — inline or a `private static` helper on the same class — before mutating state; bulky or multi-step behavior delegates to a static domain service extension in `{Module}.Domain/Services`, through a guarded `internal` method
- A single entity property must not have multiple uncoordinated public mutation points

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Entity   | {EntityName}       | Order      | {EntityName}.cs   | Order.cs  |
| Entity behavior method | {Verb}{Noun} or {Verb} | UpdateComment | {EntityName}.cs | Order.cs |

# Implementation
```csharp
//Skill: class-entity
//Plateau: service-with-api
//Version: 20260825120000

public class Order
{
    public int Id { get; internal set; }
    public Money Total { get; internal set; }
    public Email CustomerEmail { get; internal set; }
    public string Comment { get; internal set; }
    public uint Version { get; internal set; }

    public void UpdateComment(string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            throw new DomainException("Order.CommentRequired", "Comment must not be empty.");

        if (comment.Length > 500)
            throw new DomainException("Order.CommentTooLong", "Comment must not exceed 500 characters.");

        Comment = comment;
    }

    // Bulky/multi-step recalculation delegated to a static domain service extension
    // (see class-behavior-service); the entity exposes a guarded internal setter for it to call.
    internal void SetTotal(Money total)
    {
        Total = total;
    }
}
```

Properties with invariant state or business semantics (`Total`, `CustomerEmail`) are Value Object types (see `class-value-object`/`class-soft-value-object`) — their constructors enforce the invariant, so the entity itself needs no inline `if`-check for them. Properties without a dedicated Value Object yet (`Comment`) still validate locally inside their own behavior method.

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]

# Rules
MUST:
- Entity has `int Id` with `internal set`
- All public property setters or methods must validate state before assigning
- `Id` used in all domain logic, persistence, relationships, and internal APIs
- Use a Value Object type on an entity property when the value has invariant state or carries business semantics; entity properties other than `Id`/`Version`/unconstrained generics are Value Object types
- Validate via a locally-owned condition (inline or a `private static` helper) inside entity methods before mutating state that has no dedicated Value Object yet; throw `DomainException` when that condition fails
- A single entity property must not have multiple uncoordinated public mutation points
MUST NOT:
- Use `public` setters on any entity property
- Use a primitive type on an entity property when the value carries business meaning or invariant constraints a Value Object could enforce
- Mutate state before validating, or let a service extension bypass entity methods and write directly to properties

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]

# Check list
- [ ] Entity type selected from the matrix
- [ ] `int Id` with `internal set` present
- [ ] All public property setters and methods validate state
- [ ] Entity placed in `/{Module}.Domain/Entities`
- [ ] All properties except `Id`/`Version`/unconstrained generics that carry invariant state use a Value Object type
- [ ] Every behavior method validates via a locally-owned condition before mutating state that has no dedicated Value Object yet
- [ ] No property has multiple uncoordinated mutation points

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]

# Unittest TestCases
- [ ] WHEN entity created THEN Id is default (0) until persisted
- [ ] WHEN a property carries invariant state THEN its type is a Value Object, not a primitive
- [ ] WHEN a behavior method's condition fails THEN it throws `DomainException` before mutating state

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs.create]]
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs.extend]]
- [[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs.extend]]
