---
description: Permissive value-object record that can hold invalid values — the public, validation-agnostic shape a domain concept exposes to DTOs and other modules
project_name: "{Module}.Interfaces"
name: "Soft{ValueObject}.cs"
element_kind: class
change_kind: create
tags:
  - solution/value-objects
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

Worked example from a real module (`TaskModule`), single-property, no multi-property fields:

```csharp
namespace TaskUnderControl.Srv.TaskModule.Interfaces.ValueObjects;

// "Soft" значит: может держать невалидное значение. Это осознанно — DTO с плохими
// данными от клиента должен долететь досюда, чтобы его смогли провалидировать,
// а не упасть на этапе десериализации без внятной ошибки.
public record SoftComplexity(int Value);
```

# Rule changes

## MUST
- Be declared as `record`
- Not validate values in constructor or properties
- Allow invalid values
- Provide a `protected` parameterless constructor for multi-property types when EF Core materialization is needed

## SHOULD
- Name file and class `Soft{ValueObject}`

## MUST NOT
- Throw exceptions for invalid values
- Contain business logic
- Reference `{Module}.Domain` or `Domain.Rules`

# Check list
- [ ] `Soft{ValueObject}` is a `record`
- [ ] No validation in constructor
- [ ] Invalid values are allowed
- [ ] Multi-property `Soft{ValueObject}` has a `protected` parameterless constructor

# Unittest TestCases
- [ ] When `Soft{ValueObject}` is created with an invalid value Then no exception is thrown
- [ ] When `Soft{ValueObject}` is created with a valid value Then properties are set correctly
