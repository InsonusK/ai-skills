---
name: class-feature-validator
description: Transport correctness validator
domain: skill
type: template
version: 20260629223200
plateau: default
tags:
  - skill/template/class
  - plateau/default
  - stack/dotnet
  - concern/architecture

created_by:
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill|solution-command-integration]]"
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-dto-property-validators.skill/solution-dto-property-validators.skill|solution-dto-property-validators]]"
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]"
---

# Goal
- Validate transport correctness of one command's input before it reaches the handler
- Express validation rules as a declarative FluentValidation rule set — not imperative checks
- This is the per-command validator; for `Soft{ValueObject}` property validators and public RequestDto validators see [[skills/dotnet/architecture/deprecated/v1/plateau/default/structure/{Module}.Application/classes/class-property-validator.skill|class-PropertyValidator]] and [[skills/dotnet/architecture/deprecated/v1/plateau/default/structure/{Module}.Application/classes/class-dto-validator.skill|class-DtoValidator]]

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill|solution-command-integration]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create|{FeatureName}.Validator.cs]]
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.extend|{FeatureName}.Validator.cs]]

# Core Principles
- Apply ONE plateau template per class
- Extends `AbstractValidator<TCommand>`
- Rules defined in constructor via `RuleFor(...)`
- Transport correctness only: `NotEmpty`, `NotNull`, `MaximumLength`, `GreaterThan`, `InclusiveBetween`, email format, regex format
- Commands implementing `ICommandWithTimestamp` validate `ActionTimeStamp` with `NotEmpty()` and a `Must` rule ensuring it is not in the future
- Uses `SetValidator(IValidator<Soft{ValueObject}>)` for Soft VO properties
- No database access, no repository injection — purely declarative on the command's properties
- No business logic — existence and state checks belong in handler guard or domain
- Distinct from `{ValueObject}PropertyValidator` (validates a `Soft{ValueObject}`) and `{Dto}Validator` (validates a public RequestDto, or a ResponseDto only when explicitly required)

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill|solution-command-integration]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create|{FeatureName}.Validator.cs]]
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.extend|{FeatureName}.Validator.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Command validator | `{FeatureName}Validator` | `CreateTaskValidator` | `{FeatureName}.Validator.cs` | `CreateTask.Validator.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill|solution-command-integration]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create|{FeatureName}.Validator.cs]]
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.extend|{FeatureName}.Validator.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-feature-validator
//Plateau: default
//Version: 20260628
```

Validator declares rules for each command property in the constructor:

```csharp
// {Module}.Application/Features/CreateTask/CreateTask.Validator.cs
using FluentValidation;

namespace {Module}.Application.Features.CreateTask;

public class CreateTaskValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.AssigneeId)
            .GreaterThan(0);
    }
}
```

```csharp
// {Module}.Application/Features/AssignTask/AssignTask.Validator.cs
using FluentValidation;

namespace {Module}.Application.Features.AssignTask;

public class AssignTaskValidator : AbstractValidator<AssignTaskCommand>
{
    public AssignTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .GreaterThan(0);

        RuleFor(x => x.AssigneeId)
            .GreaterThan(0);
    }
}
```

Validator for a command with `ICommandWithTimestamp`:

```csharp
// {Module}.Application/Features/CreateTask/CreateTask.Validator.cs
using FluentValidation;
using Shared.Timestamps;

namespace {Module}.Application.Features.CreateTask;

public class CreateTaskValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.ActionTimeStamp)
            .NotEmpty()
            .Must(ts => ts <= DateTimeOffset.UtcNow)
            .WithMessage("ActionTimeStamp must not be in the future.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);
    }
}
```

> **Note:** `NotEmpty()` on `DateTimeOffset` rejects `default(DateTimeOffset)`.

Validator for a command with `ICommandWithTimestamp`:

```csharp
// {Module}.Application/Features/CreateTask/CreateTask.Validator.cs
using FluentValidation;
using Shared.Timestamps;

namespace {Module}.Application.Features.CreateTask;

public class CreateTaskValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.ActionTimeStamp)
            .NotEmpty()
            .Must(ts => ts <= DateTimeOffset.UtcNow)
            .WithMessage("ActionTimeStamp must not be in the future.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);
    }
}
```

> **Note:** `NotEmpty()` on `DateTimeOffset` rejects `default(DateTimeOffset)`.

Validator for a command with `ICommandWithTimestamp`:

```csharp
// {Module}.Application/Features/CreateTask/CreateTask.Validator.cs
using FluentValidation;
using Shared.Timestamps;

namespace {Module}.Application.Features.CreateTask;

public class CreateTaskValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.ActionTimeStamp)
            .NotEmpty()
            .Must(ts => ts <= DateTimeOffset.UtcNow)
            .WithMessage("ActionTimeStamp must not be in the future.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);
    }
}
```

> **Note:** `NotEmpty()` on `DateTimeOffset` rejects `default(DateTimeOffset)`.

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill|solution-command-integration]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create|{FeatureName}.Validator.cs]]
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.extend|{FeatureName}.Validator.cs]]

# Rules
MUST:
	- Extend `AbstractValidator<TCommand>`
	- Define all rules in the constructor
	- Enforce transport correctness only — presence, length, format, numeric range
	- Be named `{FeatureName}Validator`
	- Live in `/{Module}.Application/Features/{FeatureName}/{FeatureName}.Validator.cs`
	- Use `SetValidator(IValidator<Soft{ValueObject}>)` for Soft VO command properties
	- Validate `ActionTimeStamp` with `NotEmpty()` and a future-time guard when the command implements `ICommandWithTimestamp`
MUST NOT:
	- Inject repositories, `DbContext`, or any service — purely declarative on command properties
	- Contain business rules — entity existence checks, state checks, or invariant enforcement
	- Be shared across multiple commands
	- Validate `ActionTimeStamp` against local time (`DateTime.Now`)
	- Validate `ActionTimeStamp` against local time (`DateTime.Now`)
	- Validate `ActionTimeStamp` against local time (`DateTime.Now`)

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill|solution-command-integration]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create|{FeatureName}.Validator.cs]]
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.extend|{FeatureName}.Validator.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN Validate transport correctness of one command's input before it reaches the handler
- [ ] WHEN applied THEN Express validation rules as a declarative FluentValidation rule set — not imperative checks
- [ ] WHEN applied THEN Extends AbstractValidator<TCommand>
- [ ] WHEN applied THEN Rules defined in constructor via RuleFor(...)
- [ ] WHEN applied THEN Transport correctness only: NotEmpty, NotNull, MaximumLength, GreaterThan, InclusiveBetween, email format, regex format
- [ ] WHEN applied THEN No database access, no repository injection — purely declarative on the command's properties
- [ ] WHEN applied THEN No business logic — existence and state checks belong in handler guard or domain
- [ ] WHEN naming 'Command validator' THEN pattern matches convention
- [ ] WHEN command implements `ICommandWithTimestamp` THEN validator rejects default `ActionTimeStamp`
- [ ] WHEN command implements `ICommandWithTimestamp` THEN validator rejects future `ActionTimeStamp`
- [ ] WHEN command implements `ICommandWithTimestamp` THEN validator accepts current or past `ActionTimeStamp`
- [ ] WHEN command implements `ICommandWithTimestamp` THEN validator rejects default `ActionTimeStamp`
- [ ] WHEN command implements `ICommandWithTimestamp` THEN validator rejects future `ActionTimeStamp`
- [ ] WHEN command implements `ICommandWithTimestamp` THEN validator accepts current or past `ActionTimeStamp`
- [ ] WHEN command implements `ICommandWithTimestamp` THEN validator rejects default `ActionTimeStamp`
- [ ] WHEN command implements `ICommandWithTimestamp` THEN validator rejects future `ActionTimeStamp`
- [ ] WHEN command implements `ICommandWithTimestamp` THEN validator accepts current or past `ActionTimeStamp`

__Applied solutions:__
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill|solution-command-integration]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create|{FeatureName}.Validator.cs]]
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.extend|{FeatureName}.Validator.cs]]

