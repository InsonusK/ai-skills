---
description: Soft value object declaration that can hold invalid values
project_name: "{Module}.Interfaces"
name: "Soft{ValueObject}.cs"
element_kind: class
change_kind: create
---

# Goals
- Provide a validation-agnostic value object type that other modules can use in their DTOs and commands
- Serve as the base type for the module's strict domain value object

# Core Principles
- `Soft{ValueObject}` does not enforce invariants
- `Soft{ValueObject}` allows invalid values
- `Soft{ValueObject}` is a plain record with public properties

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Soft single-property VO | `Soft{Concept}` | `SoftEmail` | `Soft{Concept}.cs` | `SoftEmail.cs` |
| Soft multi-property VO | `Soft{Concept}` | `SoftMoney` | `Soft{Concept}.cs` | `SoftMoney.cs` |

# Implementation changes

Single-property soft value object:

```csharp
// {Module}.Interfaces/ValueObjects/SoftEmail.cs
namespace {Module}.Interfaces.ValueObjects;

public record SoftEmail(string Value);
```

Multi-property soft value object with a parameterless constructor for EF Core materialization:

```csharp
// {Module}.Interfaces/ValueObjects/SoftMoney.cs
namespace {Module}.Interfaces.ValueObjects;

public record SoftMoney(decimal Amount, string Currency)
{
    protected SoftMoney() : this(0, string.Empty) { }
}
```

# Rule changes
MUST:
- Be declared as `record`
- Not validate values in constructor or properties
- Allow invalid values
- Provide a `protected` parameterless constructor for multi-property VOs when EF Core materialization is needed

SHOULD:
- Name file and class `Soft{ValueObject}`

MUST NOT:
- Throw exceptions for invalid values
- Contain business logic

# Anti-patterns
- Adding validation to `Soft{ValueObject}`
- Using the domain value object as a command or DTO property directly

# Check list
- [ ] `Soft{ValueObject}` is a `record`
- [ ] No validation in constructor
- [ ] Invalid values are allowed
- [ ] Multi-property `Soft{ValueObject}` has a `protected` parameterless constructor

# Unittest TestCases
- [ ] When `Soft{ValueObject}` is created with an invalid value Then no exception is thrown
- [ ] When `Soft{ValueObject}` is created with a valid value Then properties are set correctly
