---
name: plateau-core--class-dto-validator
description: Class {Dto}Validator in the plateau-core plateau — a reusable AbstractValidator<{Dto}> composing property validators plus the DTO's own local cross-field conditions
whenToUse: when creating or editing a RequestDto validator in {Module}.Application/Validators/Model, or wiring cross-field checks over a DTO's own fields
domain: skill
type: template
plateau: core
version: 20260902000000
tags:
  - skill/template/class
  - plateau/core
created_by:
  - "[[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]"
---

# Goal
- Provide a reusable validator for every public RequestDto declared in `{Module}.Interfaces`, resolvable by other modules as `IValidator<{Dto}>`.

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- Extends `AbstractValidator<{Dto}>`; uses `SetValidator(IValidator<Soft{ValueObject}>)` for every value-concept property — never an inline restatement.
- A cross-field condition across two or more of the DTO's own fields is written locally with `.Must(...)` — this validator owns it, no shared rules abstraction needed.
- Stateless, declarative, no I/O — a validator that needs preloaded data is a `{Feature}Check`, not a `{Dto}Validator`.
- Lives in `/{Module}.Application/Validators/Model`. ResponseDto validators are created only when explicitly required.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| RequestDto validator | `{Dto}Validator` | `TaskDtoValidator` | `{Dto}.Validator.cs` | `TaskDto.Validator.cs` |

# Implementation
```csharp
// Skill: plateau-core--class-dto-validator
// Plateau: core
// Version: 20260902000000
using FluentValidation;
using {Module}.Interfaces.DTOs;
using {Module}.Interfaces.ValueObjects;

namespace {Module}.Application.Validators.Model;

public sealed class TaskPostRequestDtoValidator : AbstractValidator<TaskPostRequestDto>
{
    public TaskPostRequestDtoValidator(IValidator<SoftComplexity> complexityValidator)
    {
        RuleFor(x => x.Complexity).SetValidator(complexityValidator);

        RuleFor(dto => dto)
            .Must(dto => dto.StartDateTime is null || dto.DueDateTime is null || dto.DueDateTime >= dto.StartDateTime)
            .WithMessage("Due date must not be earlier than start date.");
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

# Rules
MUST:
- Extend `AbstractValidator<{Dto}>`, named `{Dto}Validator`, in `/Validators/Model`.
- Use `SetValidator(IValidator<Soft{ValueObject}>)` for every value-concept property.
- Assemble any cross-field condition only from the DTO's own already-available fields — never perform I/O.
- Never inject a repository, `DbContext`, or service.
- Never validate a value-concept property inline instead of composing its property validator.
- Never apply several plateau templates per class.

# Check list
- [ ] `AbstractValidator<{Dto}>`, named `{Dto}Validator`, in `/Validators/Model`.
- [ ] Every value-concept property uses `SetValidator`.
- [ ] Cross-field conditions are local `.Must(...)`, no I/O.

# Unittest TestCases
- [ ] WHEN a valid RequestDto is validated THEN no errors are returned.
- [ ] WHEN a value-concept property is invalid THEN the composed property validator's error surfaces.
- [ ] WHEN the DTO's own fields form an invalid combination THEN the local cross-field rule fails.
- [ ] WHEN resolved from DI as `IValidator<{Dto}>` THEN this validator is returned.
