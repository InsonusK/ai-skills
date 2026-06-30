---
name: class-dto-validator
description: FluentValidation validator for a public RequestDto declared in {Module}.Interfaces, or for a ResponseDto only when explicitly required
domain: skill
type: template
version: 20260701011400
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators.skill]]"
---

# Goal
- Provide a reusable validator for every public RequestDto declared in `{Module}.Interfaces`
- Allow other modules to validate RequestDto values they receive through `IValidator<{Dto}>`
- Provide a validator for a ResponseDto only when a concrete requirement explicitly demands it

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
| RequestDto validator | `{Dto}Validator` | `TaskDtoValidator` | `{Dto}.Validator.cs` | `TaskDto.Validator.cs` |
| ResponseDto validator (explicit only) | `{Dto}Validator` | `TaskDtoValidator` | `{Dto}.Validator.cs` | `TaskDto.Validator.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-dto-validator
//Plateau: default
//Version: 20260701011400
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
	- Exist for every public RequestDto in `{Module}.Interfaces`
MUST NOT:
	- Inject repositories or services
	- Contain business rules
	- Use inline FluentValidation predicates instead of property validators
	- Validate primitive properties directly — every value-concept must be a `Soft{ValueObject}` with its own property validator
	- Be created for ResponseDto without an explicit requirement

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
- [ ] WHEN applied THEN Provide a reusable validator for every public RequestDto declared in {Module}.Interfaces
- [ ] WHEN applied THEN Allow other modules to validate RequestDto values they receive through IValidator<{Dto}>
- [ ] WHEN applied for an explicitly required ResponseDto THEN provide a reusable validator for that ResponseDto
- [ ] WHEN applied THEN Extends AbstractValidator<{Dto}>
- [ ] WHEN applied THEN Uses SetValidator(IValidator<Soft{ValueObject}>) for Soft VO properties
- [ ] WHEN naming 'RequestDto validator' THEN pattern matches convention
- [ ] When a valid RequestDto is validated THEN no errors are returned
- [ ] When a RequestDto with an invalid Soft{ValueObject} property is validated THEN validation errors are returned
- [ ] When resolved from DI as IValidator<{Dto}> THEN the DTO validator is returned
- [ ] WHEN applied THEN every value-concept property is validated by its property validator

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs.create]]
