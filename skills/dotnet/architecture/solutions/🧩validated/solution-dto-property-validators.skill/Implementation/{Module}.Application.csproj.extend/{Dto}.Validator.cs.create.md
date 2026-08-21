---
description: FluentValidation validator for a public RequestDto — composes property validators and, where two of the DTO's own fields must be matched up, its own local cross-field condition
project_name: "{Module}.Application"
name: "{Dto}.Validator.cs"
element_kind: class
change_kind: create
tags:
  - solution/dto-property-validators
  - element/dto-validator-cs
---

# Goals
- Provide a reusable validator for every public RequestDto declared in `{Module}.Interfaces`
- Let other modules validate RequestDto values they receive through `IValidator<{Dto}>`
- ResponseDto validators are created only when explicitly required

# Core Principles
- Extends `AbstractValidator<{Dto}>`
- Uses `SetValidator(IValidator<Soft{ValueObject}>)` for every value-concept property — never validates one inline
- When two or more of the DTO's own fields must be matched up (a cross-field condition), the check is written locally in this validator with `.Must(...)` over the fields — this solution owns that condition and does not require a shared rules abstraction to exist
- Stateless and declarative

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| RequestDto validator | `{Dto}Validator` | `TaskDtoValidator` | `{Dto}.Validator.cs` | `TaskDto.Validator.cs` |

# Implementation changes

Per-property composition:

```csharp
// {Module}.Application/Validators/Model/TaskDtoValidator.cs
using FluentValidation;
using {Module}.Interfaces.DTOs;
using {Module}.Interfaces.ValueObjects;

namespace {Module}.Application.Validators.Model;

public class TaskDtoValidator : AbstractValidator<TaskDto>
{
    public TaskDtoValidator(
        IValidator<SoftTitle> titleValidator,
        IValidator<SoftEmail> emailValidator)
    {
        RuleFor(x => x.Title).SetValidator(titleValidator);
        RuleFor(x => x.Email).SetValidator(emailValidator);
    }
}
```

Cross-field condition across two of the DTO's own separate fields — checked locally, no property validator involved (worked example, `TaskModule`):

```csharp
public class TodoTaskPostRequestDtoValidator : AbstractValidator<TodoTaskPostRequestDto>
{
    public TodoTaskPostRequestDtoValidator(IValidator<SoftComplexity> complexityValidator)
    {
        RuleFor(x => x.Complexity).SetValidator(complexityValidator);

        // Two of THIS DTO's own fields, matched up by THIS DTO's own validator — the DTO
        // doesn't store "Schedule" as one property, so the condition is written here directly.
        RuleFor(dto => dto)
            .Must(dto => dto.StartDateTime is null || dto.DueDateTime is null || dto.DueDateTime >= dto.StartDateTime)
            .WithMessage("Due date must not be earlier than start date.");
    }
}
```

Usage from another module, through DI:

```csharp
public class SomeHandler
{
    private readonly IValidator<TaskDto> _taskDtoValidator;

    public SomeHandler(IValidator<TaskDto> taskDtoValidator)
    {
        _taskDtoValidator = taskDtoValidator;
    }

    public void Validate(TaskDto dto)
    {
        var result = _taskDtoValidator.Validate(dto);
        // handle result
    }
}
```

# Rule changes

## MUST
- Extend `AbstractValidator<{Dto}>`
- Be named `{Dto}Validator`
- Live in `/{Module}.Application/Validators/Model`
- Use `SetValidator(IValidator<Soft{ValueObject}>)` for every value-concept property
- Assemble a cross-field condition only from the DTO's own already-available fields — never perform I/O
- ResponseDto validators are created only when explicitly required — see the `dto-validators-only-for-request-dtos` ADR

## MUST NOT
- Validate a value-concept property inline instead of composing its `PropertyValidator`
- Inject a repository, `DbContext`, or any service — a validator that needs preloaded data is a `{Feature}Check`, not a `{Dto}Validator`

## SHOULD
- Name the DTO validator `{Dto}Validator`

# Check list
- [ ] Extends `AbstractValidator<{Dto}>`, named `{Dto}Validator`, lives in `/Validators/Model`
- [ ] Every value-concept property uses `SetValidator(IValidator<Soft{ValueObject}>)`
- [ ] Every cross-field condition across the DTO's own fields is checked locally with `.Must(...)`, no I/O
- [ ] ResponseDto has a validator only when an explicit requirement exists

# Unittest TestCases
- [ ] When a valid RequestDto is validated Then no errors are returned
- [ ] When a RequestDto with an invalid value-concept property is validated Then the property validator's errors are returned
- [ ] When a RequestDto with an invalid combination of its own fields is validated Then a validation error is returned
- [ ] When resolved from DI as `IValidator<{Dto}>` Then the DTO validator is returned
