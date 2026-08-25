---
name: class-dto-validator
description: Class {Dto}Validator in the service-with-api plateau
whenToUse: when a public RequestDto declared in {Module}.Interfaces needs a reusable, cross-module-resolvable validator
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/class
  - plateau/service-with-api
created_by:
  - "[[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]"
---

# Goal
- Provide a reusable validator for every public RequestDto declared in `{Module}.Interfaces`
- Let other modules validate RequestDto values they receive through `IValidator<{Dto}>`

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

# Core Principles
- Extends `AbstractValidator<{Dto}>`
- Uses `SetValidator(IValidator<Soft{ValueObject}>)` for every value-concept property — never validates one inline
- A cross-field condition spanning two or more of the DTO's own fields is written locally with `.Must(...)`
- ResponseDto validators are created only when explicitly required

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| RequestDto validator | {Dto}Validator | TaskDtoValidator | {Dto}.Validator.cs | TaskDto.Validator.cs |

# Implementation
```csharp
//Skill: class-dto-validator
//Plateau: service-with-api
//Version: 20260825120000

public class TaskDtoValidator : AbstractValidator<TaskDto>
{
    public TaskDtoValidator(IValidator<SoftEmail> emailValidator)
    {
        RuleFor(x => x.Email).SetValidator(emailValidator);

        RuleFor(dto => dto)
            .Must(dto => dto.StartDate is null || dto.DueDate is null || dto.DueDate >= dto.StartDate)
            .WithMessage("Due date must not be earlier than start date.");
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

# Rules
MUST:
- Extend `AbstractValidator<{Dto}>`, be named `{Dto}Validator`, live in `/{Module}.Application/Validators/Model`
- Use `SetValidator(IValidator<Soft{ValueObject}>)` for every value-concept property
- Assemble a cross-field condition only from the DTO's own already-available fields — never perform I/O
MUST NOT:
- Validate a value-concept property inline instead of composing its `PropertyValidator`
- Inject a repository, `DbContext`, or any service — a condition that needs preloaded data is a `{Feature}Check`, not this class

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

# Check list
- [ ] Extends `AbstractValidator<{Dto}>`, lives in `/Validators/Model`
- [ ] Every value-concept property uses `SetValidator`, no inline duplicate
- [ ] Cross-field conditions check only the DTO's own fields, no I/O

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]
