---
description: FluentValidation validator for a public DTO
project_name: "{Module}.Application"
name: "{Dto}.Validator.cs"
element_kind: class
change_kind: create
---

# Goals
- Provide a reusable validator for every public DTO declared in `{Module}.Interfaces`
- Allow other modules to validate DTO values they receive through `IValidator<{Dto}>`

# Core Principles
- Extends `AbstractValidator<{Dto}>`
- Uses `SetValidator(IValidator<Soft{ValueObject}>)` for Soft VO properties
- Stateless and declarative
- Registered by FluentValidation's assembly scan of `{Module}.Application`

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| DTO validator | `{Dto}Validator` | `TaskDtoValidator` | `{Dto}.Validator.cs` | `TaskDto.Validator.cs` |

# Implementation changes

```csharp
// {Module}.Application/Validators/TaskDtoValidator.cs
using FluentValidation;
using {Module}.Interfaces.DTOs;
using {Module}.Interfaces.ValueObjects;

namespace {Module}.Application.Validators;

public class TaskDtoValidator : AbstractValidator<TaskDto>
{
    public TaskDtoValidator(IValidator<SoftEmail> emailValidator)
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SoftEmail).SetValidator(emailValidator);
    }
}
```

Usage from another module through DI:

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
MUST:
- Extend `AbstractValidator<{Dto}>`
- Be named `{Dto}Validator`
- Live in `/{Module}.Application/Validators`
- Use `SetValidator(IValidator<Soft{ValueObject}>)` for Soft VO properties

MUST NOT:
- Inject repositories or services
- Contain business rules

# Anti-patterns
- Validating DTOs inside handlers instead of using the published `IValidator<{Dto}>`
- Duplicating property validation rules already covered by property validators

# Check list
- [ ] Extends `AbstractValidator<{Dto}>`
- [ ] Named `{Dto}Validator`
- [ ] Located in `/Validators`
- [ ] Uses injected property validators for Soft VO properties

# Unittest TestCases
- [ ] When a valid DTO is validated Then no errors are returned
- [ ] When a DTO with an invalid `Soft{ValueObject}` property is validated Then validation errors are returned
- [ ] When resolved from DI as `IValidator<{Dto}>` Then the DTO validator is returned
