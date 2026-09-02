---
name: plateau-core--class-soft-value-object
description: Class Soft{ValueObject} in the plateau-core plateau — a permissive value record in {Module}.Interfaces/ValueObjects that may hold an invalid value on purpose
whenToUse: when creating or editing a Soft{ValueObject} record, or deciding whether a value concept on a DTO/command should be a Soft Value Object
domain: skill
type: template
plateau: core
version: 20260902000000
tags:
  - skill/template/class
  - plateau/core
created_by:
  - "[[../../../../../solutions/solution-soft-value-objects.skill/solution-soft-value-objects.skill.md|solution-soft-value-objects]]"
---

# Goal
- Provide a validation-agnostic value type that DTOs, commands, queries, and other modules can use without depending on `{Module}.Domain`.
- Serve as the base type a strict `{ValueObject}` (VP3) inherits from, when one exists.

__Applied solutions:__
- [[../../../../../solutions/solution-soft-value-objects.skill/solution-soft-value-objects.skill.md|solution-soft-value-objects]] - [[../../../../../solutions/solution-soft-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- "Soft" = may hold an invalid value on purpose, so a bad DTO still reaches the collect-all validator instead of failing at deserialization.
- A plain `record` with public properties — no validation, no throwing, no logic, no dependency beyond the BCL.
- One record per file in `/{Module}.Interfaces/ValueObjects`.
- A multi-property soft VO gets a `protected` parameterless constructor when a strict VP3 `{ValueObject}` over it will be EF-persisted.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Single-property soft VO | `Soft{Concept}` | `SoftEmail` | `Soft{Concept}.cs` | `SoftEmail.cs` |
| Multi-property soft VO | `Soft{Concept}` | `SoftMoney` | `Soft{Concept}.cs` | `SoftMoney.cs` |

# Implementation
```csharp
// Skill: plateau-core--class-soft-value-object
// Plateau: core
// Version: 20260902000000
namespace {Module}.Interfaces.ValueObjects;

// "Soft" means: may hold an invalid value. Deliberate — a DTO carrying bad client
// data must reach the layer that validates it, not fail at deserialization.
public record SoftComplexity(int Value);
```
Multi-property, EF-materializable:
```csharp
public record SoftMoney(decimal Amount, string Currency)
{
    protected SoftMoney() : this(0, string.Empty) { }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-soft-value-objects.skill/solution-soft-value-objects.skill.md|solution-soft-value-objects]] - [[../../../../../solutions/solution-soft-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs.create]]

# Rules
MUST:
- Declare it a `record` in `/{Module}.Interfaces/ValueObjects`.
- Never validate, throw, or run business logic in the constructor or a property.
- Never reference `{Module}.Domain` or `{Module}.Domain.Rules` — depend on nothing but the BCL.
- Give a multi-property soft VO a `protected` parameterless constructor when a strict VO over it will be EF-persisted.
- Never apply several plateau templates per class.

# Check list
- [ ] `Soft{ValueObject}` is a `record`, one per file, in `/ValueObjects`.
- [ ] No validation / throw in the constructor; invalid values allowed.
- [ ] Multi-property soft VO has a `protected` parameterless constructor.

# Unittest TestCases
- [ ] WHEN constructed with an invalid value THEN no exception is thrown.
- [ ] WHEN two soft VOs with equal values are compared THEN they are equal.
