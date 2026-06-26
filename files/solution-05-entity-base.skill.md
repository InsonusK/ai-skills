---
uid: 24ddff1c-e516-47db-af1f-1d29c07f8fa7
name: solution-05-entity-base
description: Defines the base Entity pattern — domain objects with stable identity, mutable state, encapsulated behavior, and invariant enforcement. Defines the four entity type matrix based on mutability and creation source.
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - ddd
  - entity
triggers:
  - create entity
  - design domain entity
  - choose entity type
  - define aggregate
creates:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/class-Entity.skill]]"
extends:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/csproj-{Module}.Domain.skill]]"
depends_on:
  - "[[solution-01-module-boundary.skill]]"
  - "[[solution-02-solution-layer-structure.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/solution-04-value-object.skill]]"
---
# Goal

- Define a domain entity as an object with stable identity where identity — not value — determines equality
- Define the four entity types based on two axes: mutability and creation source
- Ensure every entity is assigned to exactly one type so the correct set of patterns is applied
- Prevent invalid state by enforcing that all entity properties are accessible only through controlled access modifiers

# Core Principals

- Entity has stable identity — `int Id` is always the system primary identity
- Entity has mutable state — unlike Value Objects, state changes over time
- Entity encapsulates behavior — state changes happen through methods, not direct property assignment from outside
- Entity enforces invariants — invalid state must never be reachable
- `Id` is always `internal set` — only persistence layer assigns it, never application code
- Entity type is selected from the type matrix before implementation begins — not discovered during coding
- Four entity types exist based on two axes: mutability and creation source

# Depend on solutions

- [[solution-01-module-boundary.skill]] — entities live in {Module}.Domain defined by this solution
- [[solution-02-solution-layer-structure.skill]] — placement rules for Domain project
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/solution-04-value-object.skill]] — entities may own Value Object properties

# Implementation

## [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/csproj-{Module}.Domain.skill|{Module}.Domain (.csproj)]] (extended)

### Project extension

#### Goals
- Store all entity types for this bounded context
- Own the business logic and invariant enforcement for all entities in this module

#### Core Principals
- All entities live in /{Module}.Domain/Entities
- Domain is the only layer that contains entity definitions

#### Structure
##### Project Structure
```
/{Module}.Domain
  /Entities
    InternalImmutableEntity.cs
    InternalMutableEntity.cs
    ExternalImmutableEntity.cs
    ExternalMutableEntity.cs
```

##### Directory and class skills

|Directory \| file|Description|Pattern skill|
|---|---|---|
|/Entities|All entity types for this module|[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/class-Entity.skill]]|

#### Rules

MUST:
- All entities live in /{Module}.Domain/Entities

#### Anti-patterns
- Placing entities outside /Entities folder — breaks navigation and discoverability
- Defining entities in Application or Interfaces — entities belong in Domain only

#### Check list
- [ ]  /Entities folder exists in {Module}.Domain
- [ ]  All entity classes placed in /Entities

---

### Class extension

#### [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/class-Entity.skill|class-Entity.skill]]

##### Goals

- Represent a domain object with stable identity, mutable state, encapsulated behavior, and invariant enforcement
- Select the correct entity type from the type matrix before implementation
- Define a domain entity as an object with stable identity where identity — not value — determines equality
- Ensure every entity is assigned to exactly one type so the correct set of patterns is applied
- Prevent invalid state by enforcing that all entity properties are accessible only through controlled access modifiers

##### Core Principals

- Entity has stable identity — `int Id` is always the system primary identity
- Entity has mutable state — unlike Value Objects, state changes over time
- Entity encapsulates behavior — state changes happen through methods, not direct property assignment from outside
- Entity enforces invariants — invalid state must never be reachable
- `Id` is always `internal set` — only persistence layer assigns it, never application code
- Entity type is selected from the type matrix before implementation begins — not discovered during coding
- Four entity types exist based on two axes: mutability and creation source

Entity type matrix:

|                     | No RowVersion (Immutable) | Has RowVersion (Mutable) |
| ------------------- | ------------------------- | ------------------------ |
| Internal (no Guid)  | Internal Immutable        | Internal Mutable         |
| External (has Guid) | External Immutable        | External Mutable         |

##### Implementation changes

Entity must be a class with `int Id` as primary identity. Type matrix determines which additional properties are required.

**Internal Immutable** — system dictionary, predefined type, configuration table. Created once, never changed.

```csharp
public class Currency
{
    public int Id { get; internal set; }
    public string Code { get; internal set; }
}
```

**Internal Mutable** — state entity created internally, editable after creation.

```csharp
public class Order
{
    public int Id { get; internal set; }
    public string Comment { get; internal set; }
    public uint Version { get; internal set; }
}
```

**External Immutable** — business event registration, created by external system, never changed.

```csharp
public class PaymentEvent
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }
    public string Code { get; internal set; }
}
```

**External Mutable** — created by external system, editable after creation.

```csharp
public class ExternalOrder
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }
    public string Title { get; internal set; }
    public uint Version { get; internal set; }
}
```

##### Rule changes

MUST:
- Entity has `int Id` with `internal set`
- Entity type selected from the type matrix before coding begins
- All property setters are `internal` or `private` — never `public`
- Mutable entities have `uint Version` with `internal set`
- External entities have `Guid Guid` with `internal set`
- `Id` used in all domain logic, persistence, relationships, and internal APIs

MUST NOT:
- Use `public` setters on any entity property
- Use `Guid` as primary identity — `int Id` is always primary
- Skip type matrix selection and implement based on intuition
- Use `Guid` in domain logic after creation — it is a correlation handle only

##### Anti-patterns (extended)

- `public string Title { get; set; }` — public setter allows invalid state from any caller
- Using `Guid` as the primary key — internal `int Id` is always primary
- Skipping the type matrix — leads to missing Version on mutable or missing Guid on external
- Placing entity in Application or Interfaces project — entities belong in Domain only

##### Check list (extended)

- [ ]  Entity type selected from the matrix
- [ ]  `int Id` with `internal set` present
- [ ]  All property setters are `internal` or `private`
- [ ]  Mutable entity has `uint Version` with `internal set`
- [ ]  External entity has `Guid Guid` with `internal set`
- [ ]  Entity placed in /{Module}.Domain/Entities

##### Unittest TestCases (extended)

- [ ]  When entity created Then Id is default (0) until persisted
- [ ]  When property setter called from outside domain Then compiler prevents access
- [ ]  When Internal Immutable entity loaded Then no Version property present
- [ ]  When Internal Mutable entity loaded Then Version property present
- [ ]  When External entity created Then Guid property present and immutable after creation

---

# Rules

MUST:

- Every entity has `int Id` with `internal set`
- Every entity type is selected from the four-type matrix
- Mutable entities have `uint Version`
- External entities have `Guid Guid`
- All property setters are `internal` or `private`
- All entities live in /{Module}.Domain/Entities

MUST NOT:

- Use `public` setters on entity properties
- Use `Guid` as primary identity
- Place entities outside the Domain project
- Use `long` or `string` as primary key without explicit justification

# Anti-patterns

- `public string Title { get; set; }` — public setter allows invalid state from any caller
- Using `Guid` as the primary key — internal `int Id` is always primary
- Skipping the type matrix — leads to missing Version on mutable or missing Guid on external
- Placing entity in Application or Interfaces project — entities belong in Domain only

# Check list

- [ ]  Entity type selected from the matrix
- [ ]  `int Id` with `internal set` present
- [ ]  All property setters are `internal` or `private`
- [ ]  Mutable entity has `uint Version` with `internal set`
- [ ]  External entity has `Guid Guid` with `internal set`
- [ ]  Entity placed in /{Module}.Domain/Entities

# Unittest TestCases

- [ ]  When entity created Then Id is default (0) until persisted
- [ ]  When property setter called from outside domain Then compiler prevents access
- [ ]  When Internal Immutable entity loaded Then no Version property present
- [ ]  When Internal Mutable entity loaded Then Version property present
- [ ]  When External entity created Then Guid property present and immutable after creation