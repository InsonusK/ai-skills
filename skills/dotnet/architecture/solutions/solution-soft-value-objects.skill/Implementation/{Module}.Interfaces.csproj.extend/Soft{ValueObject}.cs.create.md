---
description: Permissive value-object record that can hold invalid values — the public, validation-agnostic shape a domain concept exposes to DTOs and other modules
project_name: "{Module}.Interfaces"
name: "Soft{ValueObject}.cs"
element_kind: class
change_kind: create
tags:
  - solution/soft-value-objects
  - element/soft-valueobject-cs
---

# Goals
- Provide a validation-agnostic value-object type that DTOs, Commands, and other modules can use without depending on `{Module}.Domain`
- Serve as the base type the module's strict `{ValueObject}` inherits from, when one exists

# Core Principles
- `Soft{ValueObject}` does not enforce invariants — it allows invalid values so a DTO with bad client data can still reach the layer that validates it
- `Soft{ValueObject}` is a plain record with public properties, no logic, no dependency on `Domain.Rules` (referencing it would create a project-reference cycle: `Domain.Rules` itself references `{Module}.Interfaces` for the type to extend)

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Soft single-property VO | `Soft{Concept}` | `SoftEmail` | `Soft{Concept}.cs` | `SoftEmail.cs` |
| Soft multi-property VO | `Soft{Concept}` | `SoftMoney` | `Soft{Concept}.cs` | `SoftMoney.cs` |

# Implementation changes

Single-property:

```csharp
// {Module}.Interfaces/ValueObjects/SoftEmail.cs
namespace {Module}.Interfaces.ValueObjects;

public record SoftEmail(string Value);
```

Multi-property, with a parameterless constructor for EF Core materialization:

```csharp
// {Module}.Interfaces/ValueObjects/SoftMoney.cs
namespace {Module}.Interfaces.ValueObjects;

public record SoftMoney(decimal Amount, string Currency)
{
    protected SoftMoney() : this(0, string.Empty) { }
}
```

Single-property example:

```csharp
namespace {Module}.Interfaces.ValueObjects;

// "Soft" means: may hold an invalid value. This is deliberate — a DTO carrying bad
// client data must reach the layer that validates it, rather than failing at
// deserialization with an opaque error.
public record SoftComplexity(int Value);
```

# Rule changes

## MUST
- Declare `Soft{ValueObject}` as a `record`.
  - Risk: a `class` loses free structural equality, so two instances with the same value compare unequal and dictionary/set semantics break.
  - Fix: `public record Soft{ValueObject}(...)`.
- Never validate, throw, or run business logic in the constructor or properties — allow invalid values.
  - Risk: a throwing boundary type makes a bad-data DTO undeserializable, so the collect-all validator never sees it.
  - Fix: keep it a plain data record; validation belongs to `solution-dto-property-validators`.
- Never reference `{Module}.Domain` or `{Module}.Domain.Rules`.
  - Risk: `{Module}.Domain.Rules` itself references `{Module}.Interfaces`, so a reference back creates a project cycle; a reference to `{Module}.Domain` leaks internals through the public contract.
  - Fix: `Soft{ValueObject}` depends on nothing but the BCL.
- Give a multi-property `Soft{ValueObject}` a `protected` parameterless constructor when a strict `{ValueObject}` over it will be EF-persisted.
  - Risk: EF Core cannot materialize an owned type with no parameterless constructor.
  - Fix: `protected Soft{ValueObject}() : this(default, ...) { }`.

## SHOULD
- Name the file and the type `Soft{ValueObject}`.

# Check list
- [ ] `Soft{ValueObject}` is a `record`
- [ ] No validation in constructor
- [ ] Invalid values are allowed
- [ ] Multi-property `Soft{ValueObject}` has a `protected` parameterless constructor

# Unittest TestCases
- [ ] When `Soft{ValueObject}` is created with an invalid value Then no exception is thrown
- [ ] When `Soft{ValueObject}` is created with a valid value Then properties are set correctly
