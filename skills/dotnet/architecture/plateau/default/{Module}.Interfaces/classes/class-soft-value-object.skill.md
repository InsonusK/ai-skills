---
name: class-soft-value-object
description: Soft value object declaration that can hold invalid values and is shared through {Module}.Interfaces
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators.skill]]"
---

# Goal
- Provide a validation-agnostic value object type that other modules can use in their DTOs and commands
- Serve as the base type for the module's strict domain value object

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs.create]]

# Core Principals
- Apply ONE plateau template per class
- `Soft{ValueObject}` does not enforce invariants
- `Soft{ValueObject}` allows invalid values
- `Soft{ValueObject}` is a plain record with public properties

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Soft single-property VO | `Soft{Concept}` | `SoftEmail` | `Soft{Concept}.cs` | `SoftEmail.cs` |
| Soft multi-property VO | `Soft{Concept}` | `SoftMoney` | `Soft{Concept}.cs` | `SoftMoney.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs.create]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-soft-value-object
//Plateau: default
//Version: 20260628
```

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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs.create]]

# Rules
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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs.create]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- Adding validation to `Soft{ValueObject}`
- Using the domain value object as a command or DTO property directly

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Provide a validation-agnostic value object type that other modules can use in their DTOs and commands
- [ ] WHEN applied THEN Serve as the base type for the module's strict domain value object
- [ ] WHEN applied THEN Soft{ValueObject} does not enforce invariants
- [ ] WHEN applied THEN Soft{ValueObject} allows invalid values
- [ ] WHEN applied THEN Soft{ValueObject} is a plain record with public properties
- [ ] WHEN naming 'Soft single-property VO' THEN pattern matches convention
- [ ] WHEN naming 'Soft multi-property VO' THEN pattern matches convention
- [ ] WHEN Soft{ValueObject} is created with an invalid value THEN no exception is thrown
- [ ] WHEN Soft{ValueObject} is created with a valid value THEN properties are set correctly

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs.create]]
