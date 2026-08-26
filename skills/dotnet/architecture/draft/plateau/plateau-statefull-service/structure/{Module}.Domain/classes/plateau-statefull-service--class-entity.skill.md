---
name: plateau-statefull-service--class-entity
description: Class {Entity} in the statefull-service plateau
whenToUse: when creating or editing an entity in {Module}.Domain, or picking the right entity type from the classification matrix
domain: skill
type: template
plateau: statefull-service
version: 20260824100000
tags:
  - skill/template/class
  - plateau/statefull-service
created_by:
  - "[[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]]"
  - "[[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]]"
  - "[[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
  - "[[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]]"
  - "[[../../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]]"
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
Which of `IVersioned`/`IHasGuid`/`ICreationInfoModel`/`IUpdateInfoModel` an entity implements is decided by its classification — see `solution-entity-classification`'s Entity Type Matrix and `solution-entity-edit-timestamp`'s Timestamp Matrix. Example: an **Internal Mutable, user-initiated** entity implements `IVersioned` + `ICreationInfoModel` + `IUpdateInfoModel`, not `IHasGuid` (internal identity, not external).

```csharp
//Skill: class-entity
//Plateau: statefull-service
//Version: 20260824100000

public class Order : IVersioned, ICreationInfoModel, IUpdateInfoModel
{
    public int Id { get; internal set; }
    public Money Total { get; internal set; }
    public Email CustomerEmail { get; internal set; }
    public string Comment { get; internal set; }

    // solution-entity-concurrency-change: Version is internal-set, database-assigned (xmin) — never set by application code
    public uint Version { get; internal set; }

    // solution-entity-edit-timestamp: user timestamps assigned by the handler (SetCreationInfo/SetUpdateInfo below),
    // server timestamps assigned only by AppDbContext.OnBeforeSaving — never both from the same code path
    public DateTimeOffset UserCreatedDateTime { get; private set; }
    public DateTimeOffset ServerCreatedDateTime { get; internal set; }
    public DateTimeOffset UserUpdatedDateTime { get; private set; }
    public DateTimeOffset ServerUpdatedDateTime { get; internal set; }

    public void SetCreationInfo(DateTimeOffset userCreatedDateTime) => UserCreatedDateTime = userCreatedDateTime;
    public void SetUpdateInfo(DateTimeOffset userUpdatedDateTime) => UserUpdatedDateTime = userUpdatedDateTime;

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

A separate, **External Immutable** entity (client-generated correlation id, never updated) implements `IHasGuid`/`ICreationInfoModel` only — no `Version`, no `IUpdateInfoModel`:

```csharp
public class Attachment : IHasGuid, ICreationInfoModel
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; } // set once, in the factory method — never reassigned
    public DateTimeOffset UserCreatedDateTime { get; private set; }
    public DateTimeOffset ServerCreatedDateTime { get; internal set; }

    public void SetCreationInfo(DateTimeOffset userCreatedDateTime) => UserCreatedDateTime = userCreatedDateTime;
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
- Implement exactly the classification/timestamp interfaces its type requires — see `solution-entity-classification`'s Entity Type Matrix and `solution-entity-edit-timestamp`'s Timestamp Matrix
- `Guid` (when present) set once, in the factory method, never reassigned; `Version`/server timestamps assigned only by the persistence layer, never application code
MUST NOT:
- Use `public` setters on any entity property
- Use a primitive type on an entity property when the value carries business meaning or invariant constraints a Value Object could enforce
- Mutate state before validating, or let a service extension bypass entity methods and write directly to properties
- Implement `IVersioned`/`IHasGuid` on a type its classification forbids (e.g. `Version` on an immutable entity, `Guid` on an internal entity)

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
