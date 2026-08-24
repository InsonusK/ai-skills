---
name: class-soft-value-object
description: Class Soft{ValueObject} in the shared-rules plateau
whenToUse: when a DTO, Command, or another module needs a value-object-shaped value without the strict Domain type's throw-on-construct semantics
domain: skill
type: template
plateau: shared-rules
version: 20260824150000
tags:
  - skill/template/class
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]]"
---

# Goal
- Provide a validation-agnostic value-object type that DTOs, Commands, and other modules can use without depending on `{Module}.Domain`
- Serve as the base type the module's strict `{ValueObject}` inherits from, when one exists

__Applied solutions:__
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs.create]]

# Core Principles
- Does not enforce invariants — allows invalid values so a DTO with bad client data can still reach the layer that validates it
- Plain record with public properties, no logic, no dependency on `{Module}.Domain`

__Applied solutions:__
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Soft single-property VO | Soft{Concept} | SoftEmail | Soft{Concept}.cs | SoftEmail.cs |
| Soft multi-property VO | Soft{Concept} | SoftMoney | Soft{Concept}.cs | SoftMoney.cs |

# Implementation
```csharp
//Skill: class-soft-value-object
//Plateau: shared-rules
//Version: 20260824150000

public record SoftEmail(string Value);

public record SoftMoney(decimal Amount, string Currency)
{
    protected SoftMoney() : this(0, string.Empty) { }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs.create]]

# Rules
MUST:
- Be declared as `record`
- Not validate values in constructor or properties — allow invalid values
- Provide a `protected` parameterless constructor for multi-property types when EF Core materialization is needed
MUST NOT:
- Throw exceptions for invalid values
- Contain business logic
- Reference `{Module}.Domain`

__Applied solutions:__
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs.create]]

# Check list
- [ ] `Soft{ValueObject}` is a `record`
- [ ] No validation in constructor; invalid values are allowed
- [ ] Multi-property `Soft{ValueObject}` has a `protected` parameterless constructor

__Applied solutions:__
- [[../../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs.create]]
