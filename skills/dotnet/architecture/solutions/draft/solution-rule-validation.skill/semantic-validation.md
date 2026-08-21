# Semantic validation

Several fields of one DTO/Command/Entity, with no database access. The mechanism below splits into two ways to assemble the "wrapper" from these fields — a **named `SoftVO`** and an **anonymous tuple** — and both show the same thing: once the wrapper is assembled, everything else works **literally the same way as in [format-validation.md](./format-validation.md)** — `IsValid()` + `IRuleBuilder` extension + `Check()`, the same throw pattern, the same set of adapters. Semantic validation is not a separate mechanism — it is Format validation over a wrapper that had to be assembled on the spot, rather than taken as a ready-made container property.

## Way 1 — a named `SoftVO`: Schedule

A terminology clarification, which is exactly why this example is worth covering separately: `SoftSchedule(Start, Due)` is itself a composite type, but a check **over `SoftSchedule` as a single value** is Format, not Semantic: `SoftSchedule` is just as atomic an entity as `SoftComplexity`, it just holds two dates instead of one. Semantic validation doesn't start where a value has two fields — it starts where those two fields are **separate, independent properties of a DTO or Entity**, and it's specifically the DTO/Entity validator's job to match them up by assembling a `SoftSchedule` on the spot.

That's exactly the case in `TaskModule`: `TodoTaskPostRequestDto` and `TodoTask` don't expose `Schedule` as a separate field — they store `StartDateTime`/`DueDateTime` as two separate properties (a decision fixed in [Schedule DTO Shape](<../../../ADR & Tips/Schedule DTO Shape.md>), for backward compatibility of the OpenAPI contract).

### `Domain.Rules.csproj ScheduleRules.cs`

```csharp
public static class ScheduleRules
{
    public const string WindowInvertedCode = ModuleInfo.ModuleName + ".Schedule.WindowInverted";
    public const string WindowInvertedMessage = "Due date must not be earlier than start date.";

    // Both dates are optional — the rule only fires if both are set.
    public static bool IsValid(this SoftSchedule s)
        => s.StartDateTime is null || s.DueDateTime is null || s.DueDateTime >= s.StartDateTime;

    // IRuleBuilder<T, SoftSchedule> — the same shape as ComplexityIsValid, not
    // IRuleBuilder<T, T> where T : SoftSchedule. The difference matters: this shape works
    // both when T itself is SoftSchedule (RuleFor(x => x) inside AbstractValidator<SoftSchedule>),
    // and when SoftSchedule is a PROJECTION from something else (RuleFor(x => new SoftSchedule(...))
    // inside AbstractValidator<TodoTaskPostRequestDto> — see below).
    public static IRuleBuilderOptions<T, SoftSchedule> ScheduleIsValid<T>(this IRuleBuilder<T, SoftSchedule> rule)
        => rule.Must(x => x.IsValid())
               .WithErrorCode(WindowInvertedCode)
               .WithMessage(WindowInvertedMessage)
               // (rootModel, propertyValue) => ...: the one-parameter WithState(x => ...)
               // binds x to T, not to SoftSchedule — fails when T != SoftSchedule (a projection).
               .WithState((_, x) => new { x.StartDateTime, x.DueDateTime });

    private static readonly InlineValidator<SoftSchedule> _validator = new();
    static ScheduleRules() => _validator.RuleFor(x => x).ScheduleIsValid();
    public static ValidationResult Check(this SoftSchedule s) => _validator.Validate(s);
}
```

### The same Rule used in Format style — where `SoftSchedule` exists as a separate VO

`Schedule : SoftSchedule` in Domain calls `this.Check()` exactly like `Complexity`; `SchedulePropertyValidator : AbstractValidator<SoftSchedule>` calls `RuleFor(x => x).ScheduleIsValid()`. This is needed, for example, when a `SoftSchedule` has already been assembled as a whole somewhere in the code (inside a Handler) and needs to be validated as a single value — the same `ScheduleRules`, just invoked with an already-built wrapper rather than with the two fields of a container.

### Semantic usage — real code from `TodoTaskPostRequestDto`/`TodoTask`

```csharp
// Interfaces/DTOs/TodoTaskPostRequestDto.cs — two SEPARATE fields, not a Schedule
public sealed record TodoTaskPostRequestDto
{
    // ...
    public DateTimeOffset? StartDateTime { get; init; }
    public DateTimeOffset? DueDateTime { get; init; }
    // ...
}
```

```csharp
// Application/Validators/Model/TodoTaskPostRequestDtoValidator.cs
public class TodoTaskPostRequestDtoValidator : AbstractValidator<TodoTaskPostRequestDto>
{
    public TodoTaskPostRequestDtoValidator()
    {
        // ...
        // Semantic: two fields of THIS DTO are matched up by THIS DTO's validator.
        // SoftSchedule is assembled here, ad hoc, purely to reuse the ready-made
        // ScheduleRules condition — the DTO itself doesn't store this value.
        // RuleFor accepts a projection (it doesn't have to be a property directly), so
        // ScheduleIsValid() is called the same way, in one line, without a manual Must/WithErrorCode —
        // the code, message, and State are picked up from ScheduleRules automatically.
        RuleFor(dto => new SoftSchedule(dto.StartDateTime, dto.DueDateTime)).ScheduleIsValid();
    }
}
```

```csharp
// Domain/Entities/TodoTask.cs — the same technique at the Entity level
public void UpdateSchedule(DateTimeOffset? startDateTime, DateTimeOffset? dueDateTime, DateTimeOffset userActionTimeStamp)
{
    var result = new SoftSchedule(startDateTime, dueDateTime).Check();
    var blocking = result.Errors.FirstOrDefault(e => e.Severity == Severity.Error);
    if (blocking is not null)
        throw new DomainException(blocking.ErrorCode, blocking.ErrorMessage);

    StartDateTime = startDateTime;
    DueDateTime = dueDateTime;
    UserUpdatedDateTime = userActionTimeStamp;
}
```

The same condition (`ScheduleRules.IsValid`), three call sites: the `Schedule` VO constructor (Format, when something already assembled the VO), `TodoTaskPostRequestDtoValidator` (Semantic — matches two DTO fields), `TodoTask.UpdateSchedule` (Semantic — matches two Entity fields). One `.feature` with the boundary case `Start == Due` (valid) and `Start > Due` (invalid) is bound to all three — the same way as in [format-validation.md](./format-validation.md).

---

## Way 2 — an anonymous tuple: TaskLink does not reference itself

The second way to assemble a wrapper is when the fields **should not** be given a name and a type, because the combination isn't a domain concept on its own — it's purely what this one comparison needs. `(int ParentId, int ChildId)` is not "something," it's just the pair of `TaskLink`'s fields that the comparison is about; introducing a `SoftTaskLinkIds` type for this would be artificial.

### `Domain.Rules.csproj TaskLinkSelfLinkRule.cs`

Singular in the class name — if a check for a cycle in the task graph appears tomorrow, that's `TaskLinkNoCycleRule`, a separate class, not a third method here (one rule = one rejection reason = one semantics, even if it's generic over the value's representation).

```csharp
public static class TaskLinkSelfLinkRule
{
    public const string Code = ModuleInfo.ModuleName + ".TaskLinkSelfLink.SelfLink";
    public const string Message = "A task cannot be linked to itself.";

    // Generic over T — the same condition serves both (int ParentId, int ChildId) on the Entity
    // (after string ids are resolved) and (string ParentStringId, string ChildStringId) on the DTO
    // (before resolution). One predicate, two representations of the same concept "the same thing."
    public static bool IsNotSelfLink<T>(this (T Parent, T Child) ids) where T : IEquatable<T>
        => !ids.Parent.Equals(ids.Child);

    public static IRuleBuilderOptions<TRoot, (T Parent, T Child)> IsNotSelfLink<TRoot, T>(
        this IRuleBuilder<TRoot, (T Parent, T Child)> rule) where T : IEquatable<T>
        => rule.Must(x => x.IsNotSelfLink())
               .WithErrorCode(Code)
               .WithMessage(Message)
               .WithState((_, x) => new { x.Parent, x.Child });

    // Check() is defined only for (int, int) — the representation that actually
    // needs to be checked in isolation (Entity/raw predicate). The string representation
    // is only used inside DtoValidator via a projection, it doesn't need its own Check().
    private static readonly InlineValidator<(int Parent, int Child)> _validator = new();
    static TaskLinkSelfLinkRule() => _validator.RuleFor(x => x).IsNotSelfLink();
    public static ValidationResult Check(this (int Parent, int Child) ids) => _validator.Validate(ids);
}
```

### `Domain.csproj Entities/TaskLink.cs`

```csharp
public static TaskLink Create(int parentId, int childId, DateTimeOffset ts, /* ... */)
{
    var result = (parentId, childId).Check();
    var blocking = result.Errors.FirstOrDefault(e => e.Severity == Severity.Error);
    if (blocking is not null)
        throw new DomainException(blocking.ErrorCode, blocking.ErrorMessage);

    return new TaskLink { ParentId = parentId, ChildId = childId, /* ... */ };
}
```

### `Application.csproj Validators/Model/TaskLinkPostRequestDtoValidator.cs`

Comparing string ids is resolved without a single trip to the database — if `ParentStringId == ChildStringId`, it's the same task regardless of whether it even exists; so this is still a Semantic check, not a Domain one (existence is covered in [domain-validation.md](./domain-validation.md)):

```csharp
public class TaskLinkPostRequestDtoValidator : AbstractValidator<TaskLinkPostRequestDto>
{
    public TaskLinkPostRequestDtoValidator()
    {
        // ...
        RuleFor(dto => (dto.ParentStringId, dto.ChildStringId)).IsNotSelfLink();
        // ...
    }
}
```

### `.feature` and adapters

```gherkin
Feature: TaskLink cannot connect a task to itself

  Scenario Outline: Self-link rejection
    Given parent id <parentId>
    And child id <childId>
    When the link is checked
    Then the verdict is <verdict>
    And the rejection code is <code>

    Examples:
      | parentId | childId | verdict  | code                                  |
      | 1        | 1       | rejected | TaskModule.TaskLinkSelfLink.SelfLink  |
      | 1        | 2       | accepted | -                                     |
```

```csharp
// Domain.Rules.Tests/StepDefinitions/TaskLinkSelfLinkSteps.cs — the "raw Check()" adapter
[When(@"^the link is checked$")]
public void WhenTheLinkIsChecked() => context.Verdict = (_parentId, _childId).Check();
```
```csharp
// Domain.Tests/StepDefinitions/Entity/TaskLinkSteps.cs — the "Entity" adapter
[When(@"^the link is checked$")]
public void WhenTheLinkIsChecked()
    => context.Verdict = TestSupport.ValidationResultAdapters.FromDomainAction(
        () => TaskLink.Create(_parentId, _childId, DateTimeOffset.UtcNow));
```
```csharp
// Application.Tests/StepDefinitions/Dto/TaskLinkPostRequestDtoSteps.cs — the "DtoValidator" adapter
[When(@"^the link is checked$")]
public void WhenTheLinkIsChecked()
    => context.Verdict = new TaskLinkPostRequestDtoValidator().Validate(
        _validTaskLinkPostRequestDto with { ParentStringId = _parentId.ToString(), ChildStringId = _childId.ToString() });
```

Three adapters, not five — this rule has no separate `SoftVO`/`PropertyValidator` (it's a tuple, not a standalone type): it binds exactly to the layers that actually check it. The `PropertyValidator` layer (see [format-validation.md](./format-validation.md)) only appears when the wrapped value is a reusable, DI-resolvable type; a working tuple doesn't need that isolation.

---

## Summary: what the two ways have in common

| | Named SoftVO (Schedule) | Anonymous tuple (TaskLink) |
|---|---|---|
| When to choose | The field combination is a domain concept on its own, potentially an Entity property | The combination is only the input for this one comparison |
| `IsValid()` | `this SoftSchedule` | `this (T Parent, T Child)`, generic |
| `IRuleBuilder` extension | `IRuleBuilder<T, SoftSchedule>` | `IRuleBuilder<TRoot, (T,T)>` |
| `Check()` | `this SoftSchedule` | `this (int,int)` (the representation checked in isolation) |
| `PropertyValidator` layer | Present (the VO is reusable) | Usually absent (a tuple isn't reused outside) |

Next — Domain validation: the same mechanism, the same `Check()`, the only new thing is that the wrapper isn't assembled from the container's own fields, but from data that must first be loaded. See [domain-validation.md](./domain-validation.md).
