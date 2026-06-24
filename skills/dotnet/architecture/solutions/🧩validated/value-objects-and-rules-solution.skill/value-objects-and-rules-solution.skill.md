---
uid: 4f8b2c1e-9d3a-4e7f-b8a1-2c4d6e8f0a2b
name: value-objects-and-rules-solution
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
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration.solution.skill]]"
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
- Extract Value Objects and Rules that are reusable across multiple modules into Shared.csproj

# Core Principals
- Semantics belong to types, not primitives — if a primitive carries business meaning, it is a VO
- Value Object is immutable — no property can change after construction
- Value Object is self-validating — invalid state cannot exist, constructor throws on violation
- Equality is structural — two instances with same values are equal
- Value Object has no identity — it is defined entirely by its value
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
- Rules define predicates — Entities define consistency — Value Objects define correctness
- Value Objects and Rules used by two or more modules belong in Shared.csproj, not duplicated in each module

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj]] - hosts Value Objects, Rules, and entities
    - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs]] - entity pattern extended with Value Object properties and rule calls
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/Shared.csproj.create.md|Shared.csproj]] - hosts cross-module reusable Value Objects and Rules
- [[skills/dotnet/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration.solution.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - provides EF Core configuration pattern for multi-property Value Objects
    - [[skills/dotnet/architecture/solutions/🧩validated/domain-configuration.solution.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md|{Entity}Config.cs]] - configures `OwnsOne` for multi-property Value Objects

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

MUST:
- All Value Objects declared as `sealed record`
- All Value Objects immutable — no public setters
- All Value Objects self-validating — throw `DomainException` on invalid construction
- Value Objects live in `/{Module}.Domain/ValueObjects` or `/Shared/ValueObjects` when cross-module
- Multi-property VO has private parameterless constructor
- Multi-property VO has `OwnsOne` EF configuration on owning entity
- All rules implemented as static extension methods
- Rules return `bool` — caller decides whether to throw
- Rules are stateless and deterministic
- Primitive rule is single source of truth — VO rules delegate to it
- All rules live in `/{Module}.Domain/Rules` or `/Shared/Rules` when cross-module
- Named `{Type}Rules` for primitive/VO rules, `{Condition}Rule` for contextual rules
- Extract reusable VO/rule to Shared.csproj when used by two or more modules
- Use Value Object on Entity property when the value has invariant state or carries business semantics
- Call domain rules inside entity methods before mutating state
- Throw `DomainException` when a rule returns `false` — the entity enforces, the rule only predicates
- Configure multi-property Value Objects with `OwnsOne` in the entity's EF configuration

SHOULD:
- Single-property VO has implicit conversion operators
- All VOs override `ToString()` when used in logs or UI
- Complex invariant logic extracted to domain rule
- Rules be synchronous
- Rules avoid allocations
- Use the most specific rule available (primitive, VO, or contextual) for the condition being checked

MUST NOT:
- Value Object depend on infrastructure, repositories, or application services
- Value Object expose public setters
- Value Object be used to carry identity — use entity Id for that
- Primitive used in place of VO when the primitive carries business meaning
- Rule throw exceptions internally
- Rule depend on EF Core, FluentValidation, ASP.NET, HttpContext, or any infrastructure
- Rule mutate any object
- Rule duplicate logic that already exists in another rule
- Rule be instantiated with `new` — always static
- Reimplement rule logic inline inside entity methods — always delegate to existing rules
- Mutate state before validating with rules
- Allow invalid state to persist silently
- Duplicate the same VO/rule logic across multiple module Domain projects
- Use primitive type on Entity property when the value carries business meaning or invariant constraints

# Anti-patterns
- Primitive on Entity instead of Value Object when the value has invariant state — loses invariant enforcement
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
- [ ] Entity uses VO for properties with invariant state
- [ ] Entity calls rules before mutating state
- [ ] Entity throws `DomainException` when rule returns false

# Unittest TestCases
- [ ] When value is below lower bound Then constructor throws DomainException
- [ ] When value is above upper bound Then constructor throws DomainException
- [ ] When value is at lower boundary (min valid) Then object created successfully
- [ ] When value is at upper boundary (max valid) Then object created successfully
- [ ] When valid value provided Then object created with correct property value
- [ ] When two VOs have same value Then they are equal
- [ ] When two VOs have different values Then they are not equal
- [ ] When implicit operator used Then value round-trips losslessly (single-property only)
- [ ] When multi-property VO persisted and loaded Then all properties materialize correctly
- [ ] When value satisfies rule Then returns true
- [ ] When value violates rule Then returns false
- [ ] When boundary value at minimum Then returns expected result
- [ ] When boundary value at maximum Then returns expected result
- [ ] When VO overload called Then delegates to primitive overload — same result
- [ ] When contextual VO overload called Then same result as primitive tuple overload
- [ ] Rule is pure — same input always produces same output
- [ ] Rule has no side effects — calling it twice produces no observable difference
