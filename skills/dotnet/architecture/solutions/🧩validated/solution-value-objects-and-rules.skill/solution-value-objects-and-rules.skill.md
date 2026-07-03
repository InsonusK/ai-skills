---
name: solution-value-objects-and-rules
description: Defines the Value Object and Domain Rule patterns — immutable self-validating types that encode domain semantics, and stateless deterministic predicates that encode reusable business conditions as static extension methods. Also governs extraction of reusable VOs and rules into Shared.csproj.
domain: skill
type: architecture
version: 20260611
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - ddd
  - value-object
  - rules
triggers:
  - create value object
  - eliminate primitive obsession
  - encode domain concept as type
  - design immutable domain type
  - create domain rule
  - encode business predicate
  - extract business condition
  - reusable domain validation
creates:
  - "{Module}.Domain.ValueObjects.{ValueObject}.cs"
  - "{Module}.Domain.Rules.{Rule}.cs"
extends:
  - "{Module}.Domain.csproj"
  - "{Module}.Domain.Entities.{EntityName}.cs"
  - Shared.csproj
depends_on:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration.skill]]"
---
# Goal
- Eliminate primitive obsession by encoding domain semantics into dedicated Value Object types
- Prevent invalid domain state by making Value Objects self-validating at construction time
- Ensure equality is based on value, not reference — two VOs with same data are equal
- Define two VO shapes: single-property and multi-property, each with distinct rules
- Define a single reusable pattern for encoding business predicates that can be used across Value Objects, Entities, and Domain Services
- Prevent business condition duplication across controllers, validators, services, and entities
- Separate the predicate (does this satisfy the condition?) from the enforcement (throw if not)
- Ensure primitive and VO overloads of the same rule share one implementation — no duplication
- Ensure every Entity property is a Value Object except `Id`, `Version`, and unconstrained generic parameters; any additional validation rule forces the generic parameter to become a Value Object
- Ensure Value Objects validate values only through Rules — no inline conditions
- Extract Value Objects and Rules that are reusable across multiple modules into Shared.csproj

# Capabilities
- Elimination of primitive obsession via semantic domain types
- Self-validating immutable Value Objects
- Reusable, deterministic business predicates
- Prevention of business condition duplication
- Shared location for cross-module Value Objects and rules
- Signle point of validation rules

# Core Principles
- Semantics belong to types, not primitives — if a primitive carries business meaning, it is a VO
- Value Object is immutable — no property can change after construction
- Value Object is self-validating — invalid state cannot exist, constructor throws on violation
- Equality is structural — two instances with same values are equal
- Value Object has no identity — it is defined entirely by its value
- Entity properties are Value Object types except `Id`, `Version`, and unconstrained generic parameters; any validation rule beyond the generic contract forces a Value Object
- Value Object validates values only through Rules — never via inline conditions
- Prefer Value Object over primitive on Entity properties when the value has invariant state
- Value Object has no infrastructure or application dependencies — pure domain concept
- Multi-property VO requires a private parameterless constructor for EF Core materialization
- Single-property VO should provide implicit conversion operators for ergonomic usage
- Rule defines business meaning — not transport behavior, not framework behavior
- Rule returns `bool` — the caller decides whether to throw, not the rule
- Rules are stateless, deterministic, and side-effect free
- Rules are implemented as static extension methods — never instantiated
- Primitive rule is the single source of truth — VO overloads delegate to primitive overload
- Three rule shapes exist: primitive rules, VO-scoped rules, contextual (multi-value) rules
- Value Object validates values only through Rules; inline validation logic is forbidden
- Rules define predicates — Entities define consistency — Value Objects define correctness
- Value Objects and Rules used by two or more modules belong in Shared.csproj, not duplicated in each module
- Rules are tested comprehensively and completely with all corner cases.

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]] - hosts Value Objects, Rules, and entities
    - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create|{Entity}.cs]] - entity pattern extended with Value Object properties and rule calls
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]] - hosts cross-module reusable Value Objects and Rules
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - provides EF Core configuration pattern for multi-property Value Objects
    - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md|{Entity}Config.cs]] - configures `OwnsOne` for multi-property Value Objects

NUGET:
- None — relies only on patterns defined by dependency solutions.

# Template Skill Mutations

PROJECT:
- [[./Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - extend - Add ValueObjects and Rules folders
  - [[./Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs]] - create - Value Object type (single or multi-property)
  - [[./Implementation/{Module}.Domain.csproj.extend/{Rule}.cs.create.md|{Rule}.cs]] - create - Domain rule static class
  - [[./Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs]] - extend - Use Value Objects on entity properties and rules in entity methods
- [[./Implementation/Shared.csproj.extend.md|Shared.csproj]] - extend - Add cross-module reusable Value Objects and rules
  - [[./Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs]] - create - Shared Value Object type when cross-module
  - [[./Implementation/{Module}.Domain.csproj.extend/{Rule}.cs.create.md|{Rule}.cs]] - create - Shared domain rule when cross-module

# Rules

## MUST:
- [[./Implementation/Shared.csproj.extend.md#MUST|Shared.csproj.extend]]
- [[./Implementation/{Module}.Domain.csproj.extend.md#MUST|{Module}.Domain.csproj.extend]]
	- [[./Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md#MUST|{Entity}.cs.extend]]
	- [[./Implementation/{Module}.Domain.csproj.extend/{Rule}.cs.create.md#MUST|{Rule}.cs.create]]
	- [[./Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md#MUST|{ValueObject}.cs.create]]

## SHOULD
- [[./Implementation/{Module}.Domain.csproj.extend.md#SHOULD|{Module}.Domain.csproj.extend]]
	- [[./Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md#SHOULD|{ValueObject}.cs.create]]
## MUST NOT:
- [[./Implementation/Shared.csproj.extend.md#MUST NOT|Shared.csproj.extend]]
- [[./Implementation/{Module}.Domain.csproj.extend.md#MUST NOT|{Module}.Domain.csproj.extend]]
	- [[./Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md#MUST NOT|{Entity}.cs.extend]]
	- [[./Implementation/{Module}.Domain.csproj.extend/{Rule}.cs.create.md#MUST NOT|{Rule}.cs.create]]
	- [[./Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md#MUST NOT|{ValueObject}.cs.create]]

# Anti-patterns
- Primitive on Entity instead of Value Object when the value has invariant state — loses invariant enforcement
- Entity property that is not `Id`, `Version`, or an unconstrained generic is a primitive type
- Value Object constructor with inline validation instead of calling a Rule
- VO with public setter — allows post-construction mutation, invalidates immutability guarantee
- VO that throws on `ToString()` when null internal state — private constructor must not leave fields unset for EF
- Multi-property VO without private parameterless constructor — EF materialization fails silently
- Multi-property VO without `OwnsOne` config — EF creates a shadow table or fails mapping
- VO with infrastructure dependency — couples domain to persistence layer
- Reusing same VO type across modules via project reference — each module should reference Shared, not another module's Domain
- Rule throws `DomainException` itself — rule returns `bool`, the VO or entity caller throws
- `new CanDriveCarRule().IsSatisfied()` — rules are static, never instantiated
- VO rule reimplements primitive rule logic — always delegate to primitive overload
- Same business condition checked in controller, validator, entity, and service separately — define once as rule
- Rule depends on DbContext or HttpContext — pure domain predicates only
- Rule has instance state — all rules must be stateless
- Duplicating identical Value Object or Rule across multiple modules instead of placing it in Shared
- Putting module-specific VO or Rule in Shared — Shared must contain only cross-cutting primitives

# Check list
- [ ] Declared as `sealed record`
- [ ] All invariants validated in constructor
- [ ] `DomainException` thrown on violation — not null, not bool return
- [ ] No public setters
- [ ] No infrastructure or service dependencies
- [ ] Single-property VO has implicit conversion operators
- [ ] Multi-property VO has private parameterless constructor
- [ ] Multi-property VO has `OwnsOne` EF configuration on owning entity
- [ ] `ToString()` implemented when used in logs or UI
- [ ] Lives in `/{Module}.Domain/ValueObjects` or `/Shared/ValueObjects`
- [ ] Rule is a static class with static extension methods
- [ ] Rule returns `bool` — never throws
- [ ] Rule is stateless and deterministic
- [ ] Primitive rule exists as source of truth where applicable
- [ ] VO rule delegates to primitive rule — no logic duplication
- [ ] ContextualRule has primitive tuple overload as source of truth
- [ ] Named correctly: `{Type}Rules` or `{Condition}Rule`
- [ ] Lives in `/{Module}.Domain/Rules` or `/Shared/Rules`
- [ ] No infrastructure dependencies
- [ ] Cross-module VOs live in `/Shared/ValueObjects`
- [ ] Cross-module rules live in `/Shared/Rules`
- [ ] No duplicate VO/rule logic exists in multiple modules
- [ ] Entity uses Value Object for every property except `Id`, `Version`, and unconstrained generic parameters
- [ ] Generic properties have no additional validation rules; otherwise they are replaced with Value Objects
- [ ] Value Object validates values only through Rules
- [ ] Entity uses VO for properties with invariant state
- [ ] Entity calls rules before mutating state
- [ ] Entity throws `DomainException` when rule returns false
- [ ] Unittest covers all cases in `Rules` included corner cases
