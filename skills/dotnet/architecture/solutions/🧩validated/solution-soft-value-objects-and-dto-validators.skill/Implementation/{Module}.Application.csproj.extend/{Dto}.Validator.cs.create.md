---
description: FluentValidation validator for a public DTO
project_name: "{Module}.Application"
name: "{Dto}.Validator.cs"
element_kind: class
change_kind: create
tags:
  - solution/soft-value-objects-and-dto-validators
  - element/dto-validator-cs
---

# Goals
- Provide a reusable validator for every public RequestDto declared in `{Module}.Interfaces`
- Allow other modules to validate RequestDto values they receive through `IValidator<{Dto}>`
- ResponseDto validators are created only when explicitly required

# Core Principles
- Extends `AbstractValidator<{Dto}>`
- Uses `SetValidator(IValidator<Soft{ValueObject}>)` for every value-concept property
- DTO properties that carry business meaning are `Soft{ValueObject}` types, not primitives
- Stateless and declarative

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| RequestDto validator | `{Dto}Validator` | `TaskDtoValidator` | `{Dto}.Validator.cs` | `TaskDto.Validator.cs` |

# Implementation changes

```csharp
// {Module}.Application/Validators/TaskDtoValidator.cs
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

## MUST
- Extend `AbstractValidator<{Dto}>`
- Be named `{Dto}Validator`
- Live in `/{Module}.Application/Validators`
- Use `SetValidator(IValidator<Soft{ValueObject}>)` for every value-concept property
- `{Module}.Application` must call `AddValidatorsFromAssembly` for its own assembly so that property and DTO validators are registered in DI
- ResponseDto validators are created only when explicitly required, for example external contract validation, untrusted response sources, or mandated integration boundaries
- Other modules consume validators through `IValidator<T>` resolved from DI
- Property validators and DTO validators validate values only by calling Rules
- Property validators are stateless and have no infrastructure dependencies

## MUST NOT
- Validate primitive properties directly — every value-concept must be a `Soft{ValueObject}` with its own property validator
- Validators inject repositories, `DbContext`, or services
- Validators contain business rules
- Property validators or DTO validators contain inline FluentValidation predicates that duplicate Rule logic
## SHOULD
- Name DTO validator `{Dto}Validator`

# Anti-patterns
- Validating DTOs inside handlers instead of using the published `IValidator<{Dto}>`
- Duplicating property validation rules already covered by property validators
- Using inline FluentValidation predicates instead of property validators

# Check list
- [ ] Extends `AbstractValidator<{Dto}>`
- [ ] Named `{Dto}Validator`
- [ ] Located in `/Validators/Model`
- [ ] Uses injected property validators for Soft VO properties

# Unittest TestCases
- [ ] When a valid RequestDto is validated Then no errors are returned
- [ ] When a RequestDto with an invalid `Soft{ValueObject}` property is validated Then validation errors are returned
- [ ] When resolved from DI as `IValidator<{Dto}>` Then the DTO validator is returned
- [ ] When validated Then every value-concept property is validated by its property validator
