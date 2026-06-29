---
name: class-dto-validator
description: FluentValidation validator for a public DTO declared in {Module}.Interfaces
domain: skill
type: template
version: 20260629210700
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators.skill]]"
---

# Goal
- Provide a reusable validator for every public DTO declared in `{Module}.Interfaces`
- Allow other modules to validate DTO values they receive through `IValidator<{Dto}>`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

# Core Principles
- Apply ONE plateau template per class
- Extends `AbstractValidator<{Dto}>`
- Uses `SetValidator(IValidator<Soft{ValueObject}>)` for every value-concept property
- DTO properties that carry business meaning are `Soft{ValueObject}` types, not primitives
- Stateless and declarative
- Registered by FluentValidation's assembly scan of `{Module}.Application`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| DTO validator | `{Dto}Validator` | `TaskDtoValidator` | `{Dto}.Validator.cs` | `TaskDto.Validator.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-dto-validator
//Plateau: default
//Version: 20260629210700
```

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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

# Rules
MUST:
	- Extend `AbstractValidator<{Dto}>`
	- Be named `{Dto}Validator`
	- Live in `/{Module}.Application/Validators/Model`
	- Use `SetValidator(IValidator<Soft{ValueObject}>)` for every value-concept property
MUST NOT:
	- Inject repositories or services
	- Contain business rules
	- Use inline FluentValidation predicates instead of property validators
	- Validate primitive properties directly — every value-concept must be a `Soft{ValueObject}` with its own property validator

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- Validating DTOs inside handlers instead of using the published `IValidator<{Dto}>`
- Duplicating property validation rules already covered by property validators
- Using inline FluentValidation predicates instead of property validators

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Provide a reusable validator for every public DTO declared in {Module}.Interfaces
- [ ] WHEN applied THEN Allow other modules to validate DTO values they receive through IValidator<{Dto}>
- [ ] WHEN applied THEN Extends AbstractValidator<{Dto}>
- [ ] WHEN applied THEN Uses SetValidator(IValidator<Soft{ValueObject}>) for Soft VO properties
- [ ] WHEN naming 'DTO validator' THEN pattern matches convention
- [ ] When a valid DTO is validated THEN no errors are returned
- [ ] When a DTO with an invalid Soft{ValueObject} property is validated THEN validation errors are returned
- [ ] When resolved from DI as IValidator<{Dto}> THEN the DTO validator is returned
- [ ] WHEN applied THEN every value-concept property is validated by its property validator

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]
