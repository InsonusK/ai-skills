---
uid: 2e5a9c3f-8d1b-4f7e-a4c6-b9e3f6a2d8c1
order: 10
name: validation
description: Defines the validation pattern — AbstractValidator per command co-located with the handler in {Module}.Application/Features, and ValidationBehavior in BuildingBlocks that intercepts commands before the handler runs and returns Result.Invalid on failure
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
  - dotnet
  - application
  - validation
  - fluent-validation
  - mediatr
  - pipeline
triggers:
  - add validation to command
  - define validator
  - validate command input
  - transport validation
  - fluent validation pipeline
creates:
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/classes/ValidationBehavior.class.skill|ValidationBehavior.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/classes/CommandValidator.class.skill|CommandValidator.class.skill]]"
extends:
  - "[[skills/dotnet/skill-graph/developing v2/developing/BuildingBlocks csproj/BuildingBlocks.csproj.skill|BuildingBlocks.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/{Module}.Application.csproj.skill|{Module}.Application.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Host csproj/App.Host.csproj.skill|App.Host.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/classes/CommandHandler.class.skill|CommandHandler.class.skill]]"
depends_on:
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill|01-module-boundary.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill|02-solution-layer-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/09-command-handler.solution.skill|09-command-handler.solution.skill]]"
---
>[!todo] Ресортировка solution
>Validation solution должен быть отдельным и раньше handler цель "мы можем валидировать сущности и регистрировать валидаторы"

# Goal
- Define `ValidationBehavior` as the single pipeline interception point for command input validation — handler never executes for invalid input
- Define `AbstractValidator<TCommand>` as the per-command validation contract co-located with its handler in the feature folder
- Separate transport correctness (not empty, max length, valid range) from business invariants (belongs in domain)
- Ensure validators are auto-discovered via assembly scan — no manual registration per validator

# Core Principles
- Validation is a pipeline concern — it runs before the handler, not inside it
- `ValidationBehavior` lives in BuildingBlocks — it references `ICommand` from Shared
- `ValidationBehavior` activates only on `ICommand` requests — queries are never validated here
- Handler receives only valid input — it never needs to re-check transport correctness
- Validator enforces transport correctness only — presence, length, format, range
- Business invariants belong in domain entities and domain services — never in validators
- One validator per command — no shared validators across commands
- Validators registered via assembly scan in `Register{ModuleName}Module()` — same scan as handlers
- No validator for query handlers — queries are read-only, input is not mutating state

# Depend on solutions
- [[01-module-boundary.solution.skill]] — defines BuildingBlocks and {Module}.Application project boundaries
- [[02-solution-layer-structure.solution.skill]] — defines App.Host project boundary
- [[09-command-handler.solution.skill]] — defines `ICommand` marker and feature folder structure that validators extend

# Requirements
- `FluentValidation` NuGet package — provides `AbstractValidator<T>`, `RuleFor`, validation rule DSL
- `FluentValidation.DependencyInjectionExtensions` NuGet package — provides `AddValidatorsFromAssembly`
- `MediatR` NuGet package — provides `IPipelineBehavior<TRequest, TResponse>`
- `Ardalis.Result` NuGet package — provides `Result.Invalid(errors)` and `ValidationError`

# Template Skill Mutations

## BuildingBlocks (.csproj) (extended)

### Project extension

#### Goal
- Own the `ValidationBehavior` pipeline behavior that intercepts all `ICommand` requests and validates them before the handler runs

#### Core Principals
- `ValidationBehavior` is generic — one implementation handles all commands across all modules
- Activates only on `ICommand` — query requests pass through untouched
- Returns `Result.Invalid(errors)` when any validator fails — handler never executes
- When no validator is registered for a command, pipeline proceeds normally — missing validator is not an error

#### Structure

##### Project Structure
```
/BuildingBlocks
  /MediatR
    ValidationBehavior.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /MediatR/ValidationBehavior.cs | Pipeline behavior that validates ICommand requests | ValidationBehavior.class.skill |

#### NuGet Packages
| Package | Purpose |
| --- | --- |
| `FluentValidation` | Provides `IValidator<T>` injected into ValidationBehavior |
| `MediatR` | Provides `IPipelineBehavior<TRequest, TResponse>` |
| `Ardalis.Result` | Provides `Result.Invalid` and `ValidationError` |

#### Rules
MUST:
- `ValidationBehavior` defined in BuildingBlocks — one implementation for all modules
- Activates only on `where TRequest : ICommand` — never on queries
- Returns `Result.Invalid(errors)` when validation fails — not an exception
- References `ICommand` from Shared

MUST NOT:
- `ValidationBehavior` contain any command-specific validation logic
- `ValidationBehavior` throw exceptions for validation failures — always return `Result.Invalid`

---

### Class extension

#### ValidationBehavior (created)

##### Goal
- Intercept every `ICommand` request before the handler runs
- Collect all validation errors from all registered validators for the command
- Short-circuit with `Result.Invalid(errors)` if any errors exist — handler never runs for invalid input
- Pass through to the handler if no validators are registered or all validators pass

##### Core Principals
- Receives `IEnumerable<IValidator<TRequest>>` via DI — zero, one, or multiple validators supported
- Runs all validators and collects all errors before short-circuiting — full error list, not fail-fast per field
- Maps FluentValidation `ValidationFailure` to `Ardalis.Result` `ValidationError`
- Constrained to `where TRequest : ICommand` and `where TResponse : IResult` — only activates on write operations that return a Result

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Validation pipeline behavior | `ValidationBehavior<TRequest, TResponse>` | `ValidationBehavior<CreateTaskCommand, Result<CreateTaskResult>>` | `ValidationBehavior.cs` | `ValidationBehavior.cs` |

##### Implementation changes

```csharp
// BuildingBlocks/MediatR/ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand
    where TResponse : IResult
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // no validators registered — proceed to handler
        if (!_validators.Any())
            return await next();

        // run all validators and collect all errors
        var errors = _validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Where(e => e is not null)
            .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
            .ToList();

        // any errors — short-circuit, handler never runs
        if (errors.Count > 0)
            return (TResponse)Result.Invalid(errors);

        return await next();
    }
}
```

##### Rule changes
MUST:
- Constrained to `where TRequest : ICommand` and `where TResponse : IResult`
- Collect all errors from all validators before returning — full error set, not first-error-only
- Return `Result.Invalid(errors)` on failure — not throw an exception
- Pass through when no validators registered — missing validator is not a fault

MUST NOT:
- Contain any command-specific conditions
- Throw `ValidationException` — always return typed `Result.Invalid`

---

## {Module}.Application (.csproj) (extended)

### Project extension

#### Goal
- Own one `AbstractValidator<TCommand>` per command, co-located with the handler in the feature folder
- Enforce transport correctness only — presence, length, format, numeric range

#### Core Principals
- Validator lives in the same feature folder as its handler — one folder, both files
- Validator enforces transport correctness only — not business rules
- No validator for query handlers — queries do not mutate state and do not need transport validation
- Validators auto-registered via `AddValidatorsFromAssembly` in the module registration (solution 09)

#### Structure

##### Project Structure
```
/{Module}.Application
  /Features
    /Create{Entity}
      Create{Entity}.Handler.cs    ← from solution 09
      Create{Entity}.Validator.cs  ← added by this solution
    /Update{Entity}
      Update{Entity}.Handler.cs
      Update{Entity}.Validator.cs
    /Delete{Entity}
      Delete{Entity}.Handler.cs
      Delete{Entity}.Validator.cs
    /Get{Entity}
      Get{Entity}.Handler.cs       ← no validator — query handler
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Features/{FeatureName}/{FeatureName}.Validator.cs | Transport correctness validator for command | CommandValidator.class.skill |

#### NuGet Packages
| Package | Purpose |
| --- | --- |
| `FluentValidation` | Provides `AbstractValidator<T>` and rule DSL |
| `FluentValidation.DependencyInjectionExtensions` | Provides `AddValidatorsFromAssembly` used in module registration |

#### Rules
MUST:
- One validator per command — co-located with handler in the same feature folder
- Validator file named `{FeatureName}.Validator.cs`
- Validator class named `{FeatureName}Validator`
- Validators registered via `AddValidatorsFromAssembly` in module registration

MUST NOT:
- Query handlers have validators — queries are read-only, validation not applicable
- Validators contain business rules — transport correctness only
- Validators registered manually — use assembly scan

#### Anti-patterns
- `CreateTaskCommandValidator.cs` as file name — use `CreateTask.Validator.cs`
- Business rule in validator: `RuleFor(x => x.AssigneeId).MustAsync(async (id, ct) => await _repo.AnyAsync(...))` — belongs in handler guard or domain
- Validator placed outside its feature folder — always co-located with handler
- Shared validator used for multiple commands — one validator per command

---

### Class extension

#### CommandValidator (created)

##### Goal
- Validate transport correctness of one command's input before it reaches the handler
- Express validation rules as a declarative FluentValidation rule set — not imperative checks

##### Core Principals
- Extends `AbstractValidator<TCommand>`
- Rules defined in constructor via `RuleFor(...)`
- Transport correctness only: `NotEmpty`, `NotNull`, `MaximumLength`, `GreaterThan`, `InclusiveBetween`, email format, regex format
- No database access, no repository injection — purely declarative on the command's properties
- No business logic — an `AssigneeId` validator checks `GreaterThan(0)`, not whether the assignee exists

##### What belongs in a validator vs domain

| Concern | Belongs in | Example |
| --- | --- | --- |
| Field is required | Validator | `RuleFor(x => x.Title).NotEmpty()` |
| Max string length | Validator | `RuleFor(x => x.Title).MaximumLength(200)` |
| Numeric range | Validator | `RuleFor(x => x.AssigneeId).GreaterThan(0)` |
| Valid format | Validator | `RuleFor(x => x.Email).EmailAddress()` |
| Entity must exist | Handler guard | `if (assignee is null) return Result.NotFound()` |
| Business state allows action | Domain entity | `task.Assign(assigneeId)` throws `DomainException` |
| Unique constraint | Domain / DB | Unique index + handler conflict guard |

##### Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Command validator | `{FeatureName}Validator` | `CreateTaskValidator` | `{FeatureName}.Validator.cs` | `CreateTask.Validator.cs` |

##### Implementation changes
Validator declares rules for each command property in the constructor:

```csharp
// Task.Application/Features/CreateTask/CreateTask.Validator.cs
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
// Task.Application/Features/AssignTask/AssignTask.Validator.cs
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

```csharp
// Task.Application/Features/UpdateTask/UpdateTask.Validator.cs
public class UpdateTaskValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .GreaterThan(0);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);
    }
}
```

##### Rule changes
MUST:
- Extend `AbstractValidator<TCommand>`
- Define all rules in the constructor
- Enforce transport correctness only — presence, length, format, numeric range
- Be named `{FeatureName}Validator`
- Live in `/{Module}.Application/Features/{FeatureName}/{FeatureName}.Validator.cs`

MUST NOT:
- Inject repositories, DbContext, or any service — purely declarative on command properties
- Contain business rules — entity existence checks, state checks, or invariant enforcement
- Be shared across multiple commands

---

## App.Host (.csproj) (extended)

### Project extension

#### Goal
- Register `ValidationBehavior` as the first pipeline behavior — it must run before all other behaviors including `UnitOfWorkBehavior` (solution 11)

#### Core Principals
- `ValidationBehavior` registered first — invalid commands are rejected before any other behavior activates
- Registered as open generic `Transient` — one instance per pipeline invocation
- Pipeline order is the single authoritative record of behavior execution sequence

#### Rules
MUST:
- `ValidationBehavior` registered before all other behaviors
- Registered as `AddTransient` with open generic types

---

### Class extension

#### PipelineRegistration (extended)

##### Goal
- Add `ValidationBehavior` as the first entry in the pipeline registration sequence

##### Implementation changes
`PipelineRegistration` from solution 09 extended with `ValidationBehavior` as first behavior:

```csharp
// App.Host/DependencyInjection/PipelineRegistration.cs
public static class PipelineRegistration
{
    public static IServiceCollection AddPipeline(
        this IServiceCollection services)
    {
        // 1. validation — rejects invalid commands before anything else runs
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        // solution 11 (unit-of-work) adds: UnitOfWorkBehavior
        // solution 14 (concurrency) adds: ConcurrencyBehavior
        // solution 15 (external-created) adds: GuidResolvingBehavior

        return services;
    }
}
```

##### Rule changes
MUST:
- `ValidationBehavior` registered as the first `IPipelineBehavior` entry
- Registered before `UnitOfWorkBehavior` — invalid commands must never open a unit of work

---

# Rules

MUST:
- `ValidationBehavior` defined in BuildingBlocks — one implementation for all modules
- `ValidationBehavior` constrained to `ICommand` and `IResult` — never activates on queries
- `ValidationBehavior` returns `Result.Invalid(errors)` on failure — not an exception
- One `AbstractValidator<TCommand>` per command — co-located with handler in feature folder
- Validator file named `{FeatureName}.Validator.cs`, class named `{FeatureName}Validator`
- Validators registered via `AddValidatorsFromAssembly` in module registration
- `ValidationBehavior` registered first in pipeline — before `UnitOfWorkBehavior`
- No validator for query handlers

MUST NOT:
- Validator contain business rules — transport correctness only
- Validator inject repositories or services — purely declarative
- Validator be shared across multiple commands
- `ValidationBehavior` throw exceptions — always return typed `Result.Invalid`
- Pipeline behaviors registered in module registration — belongs in App.Host

SHOULD:
- Validator rules cover all command properties that carry input constraints
- Transport validation boundary table used to decide what belongs in validator vs handler vs domain

# Anti-patterns
- `RuleFor(x => x.AssigneeId).MustAsync(async (id, ct) => await _repo.AnyAsync(...))` — entity existence is a handler guard, not transport validation
- `CreateTaskCommandValidator.cs` as file name — use `CreateTask.Validator.cs`
- Shared validator across commands: `public class EntityIdValidator : AbstractValidator<IHasId>` — one validator per command
- `ValidationBehavior` registered after `UnitOfWorkBehavior` — invalid commands would open a unit of work unnecessarily
- Business invariant in validator: `RuleFor(x => x.Status).Must(s => s != TaskStatus.Closed)` — belongs in domain entity

# Check list
- [ ] `ValidationBehavior` defined in `BuildingBlocks/MediatR/ValidationBehavior.cs`
- [ ] `ValidationBehavior` constrained to `where TRequest : ICommand` and `where TResponse : IResult`
- [ ] `ValidationBehavior` collects all errors — not fail-fast on first error
- [ ] `ValidationBehavior` returns `Result.Invalid(errors)` — not exception
- [ ] `ValidationBehavior` passes through when no validators registered
- [ ] `ValidationBehavior` registered first in App.Host pipeline
- [ ] One validator per command in `/{Module}.Application/Features/{FeatureName}`
- [ ] Validator file named `{FeatureName}.Validator.cs`
- [ ] Validator class named `{FeatureName}Validator`
- [ ] Validator extends `AbstractValidator<TCommand>`
- [ ] Validator rules cover transport correctness only — no business rules, no DB access
- [ ] No validator exists for any query handler
- [ ] Validators registered via `AddValidatorsFromAssembly` in module registration

# Unittest TestCases
- [ ] When command with empty required field is sent Then `ValidationBehavior` returns `Result.Invalid` before handler runs
- [ ] When command with field exceeding max length is sent Then `Result.Invalid` returned with correct property name
- [ ] When command with multiple invalid fields is sent Then all field errors returned in single `Result.Invalid` — not just first
- [ ] When command with all valid fields is sent Then handler executes normally
- [ ] When command has no registered validator Then pipeline proceeds to handler without error
- [ ] When query is dispatched Then `ValidationBehavior` does not activate
- [ ] When two validators registered for same command Then both validators run and errors are merged
