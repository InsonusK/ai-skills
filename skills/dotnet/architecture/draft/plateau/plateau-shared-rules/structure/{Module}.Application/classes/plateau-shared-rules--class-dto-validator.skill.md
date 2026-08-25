---
name: class-dto-validator
description: Class {Dto}Validator in the shared-rules plateau
whenToUse: when a public RequestDto declared in {Module}.Interfaces needs a reusable, cross-module-resolvable validator
domain: skill
type: template
plateau: shared-rules
version: 20260824163000
tags:
  - skill/template/class
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]"
  - "[[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
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
//Plateau: shared-rules
//Version: 20260824163000

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

## Once a cross-field condition is duplicated elsewhere: redirect to a centralized rule

Optional, applied only once the same condition is found duplicated between this DTO validator and an Entity method. Before (local condition, per `solution-dto-property-validators`):

```csharp
public class TodoTaskPostRequestDtoValidator : AbstractValidator<TodoTaskPostRequestDto>
{
    public TodoTaskPostRequestDtoValidator(IValidator<SoftComplexity> complexityValidator)
    {
        RuleFor(x => x.Complexity).SetValidator(complexityValidator);

        RuleFor(dto => dto)
            .Must(dto => dto.StartDateTime is null || dto.DueDateTime is null || dto.DueDateTime >= dto.StartDateTime)
            .WithMessage("Due date must not be earlier than start date.");
    }
}
```

After (redirected to a centralized Semantic rule — the wrapper `SoftSchedule` is assembled ad hoc from the DTO's own separate fields, reusing the same condition `TodoTask.UpdateSchedule` also uses):

```csharp
public class TodoTaskPostRequestDtoValidator : AbstractValidator<TodoTaskPostRequestDto>
{
    public TodoTaskPostRequestDtoValidator(IValidator<SoftComplexity> complexityValidator)
    {
        RuleFor(x => x.Complexity).SetValidator(complexityValidator);

        // SoftSchedule assembled ad hoc, purely to reuse ScheduleRules — the DTO itself
        // doesn't store this value as one property.
        RuleFor(dto => new SoftSchedule(dto.StartDateTime, dto.DueDateTime)).ScheduleIsValid();
    }
}
```

The local `.Must(...)`/`.WithMessage(...)` is deleted — `{Module}.Domain.Rules` is now the only place the condition exists. See [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] and its [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.extend.md|{Dto}.Validator.cs.extend]].

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.extend.md|{Dto}.Validator.cs.extend]]

# Rules
MUST:
- Extend `AbstractValidator<{Dto}>`, be named `{Dto}Validator`, live in `/{Module}.Application/Validators/Model`
- Use `SetValidator(IValidator<Soft{ValueObject}>)` for every value-concept property
- Assemble a cross-field condition only from the DTO's own already-available fields — never perform I/O
- Call the centralized extension for a redirected cross-field condition instead of a local `.Must(...)`, once redirected — delete the local condition, never keep both
MUST NOT:
- Validate a value-concept property inline instead of composing its `PropertyValidator`
- Inject a repository, `DbContext`, or any service — a condition that needs preloaded data is a `{Feature}Check`, not this class

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.extend.md|{Dto}.Validator.cs.extend]]

# Check list
- [ ] Extends `AbstractValidator<{Dto}>`, lives in `/Validators/Model`
- [ ] Every value-concept property uses `SetValidator`, no inline duplicate
- [ ] Cross-field conditions check only the DTO's own fields, no I/O

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]
