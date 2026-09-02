---
name: plateau-offline-sync-service--class-value-object
description: Class {ValueObject} in the plateau-offline-sync-service plateau — a strict, self-validating value record in {Module}.Domain that inherits Soft{ValueObject} and throws on an invalid value
whenToUse: when creating or editing a strict Value Object in {Module}.Domain/ValueObjects
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]]"
  - "[[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Encode a domain concept so an invalid instance cannot be constructed — an entity property typed `{ValueObject}` is always valid.

__Applied solutions:__
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `sealed record {ValueObject} : Soft{ValueObject}` — reuses the Soft shape, never re-declares its properties.
- **VP4:** once its condition is duplicated by a `{ValueObject}PropertyValidator` (or another consumer), the constructor is redirected to the centralized `{Module}.Domain.Rules` `{Rule}.Check()` and the local `private static` predicate is deleted. The `DomainException` thrown (code + message) now comes from the rule's blocking `ValidationFailure` (`result.Errors.First(e => e.Severity == Severity.Error)`), not a local literal.
- Immutable, no identity, no infrastructure/application/rules-project dependency.
- Multi-property VO keeps a `private` parameterless constructor for EF materialization; single-property VO gets implicit conversion operators.
- DTOs and other modules still use `Soft{ValueObject}` — the handler maps `{ValueObject}` → `Soft{ValueObject}` when projecting.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-value-object
// Plateau: domain-service
// Version: 20260902000000
using {Module}.Interfaces.ValueObjects;
using Shared.Exceptions;

namespace {Module}.Domain.ValueObjects;

public sealed record {ValueObject} : Soft{ValueObject}
{
    public {ValueObject}(string value) : base(value)
    {
        if (!IsValid(value))
            throw new DomainException("{Module}.{ValueObject}.Invalid", "…");
    }

    private static bool IsValid(string value) => !string.IsNullOrWhiteSpace(value);

    public static implicit operator string({ValueObject} v) => v.Value;
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs.create]]

# Rules
MUST:
- `sealed record`, inherits `Soft{ValueObject}`, never re-declares a base property.
- Validate in the constructor via a local `private static` predicate; throw `DomainException` on failure.
- Immutable — no public setters, no identity, no repository/`DbContext`/service/rules-project dependency.
- Multi-property VO has a `private` parameterless constructor.
- Never apply several plateau templates per class.

# Check list
- [ ] `sealed record {ValueObject} : Soft{ValueObject}` in `/ValueObjects`.
- [ ] Constructor validates locally and throws `DomainException`; no other path to an instance.
- [ ] No public setter, no infrastructure/rules dependency; `private` parameterless ctor if multi-property.

# Unittest TestCases
- [ ] WHEN constructed with an invalid value THEN `DomainException` with the expected code is thrown.
- [ ] WHEN constructed with a valid value THEN the properties are set and equality is structural.
