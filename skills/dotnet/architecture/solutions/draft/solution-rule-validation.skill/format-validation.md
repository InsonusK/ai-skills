# Format validation

One field, one check over the value of that field itself. Real code from `TaskModule` — check against the module's current state.

## Complexity — one rule on one field

### `Interfaces.csproj ValueObjects/SoftComplexity.cs`

Data only. No logic, no `Check()` — otherwise it would have to reference `Domain.Rules`, and `Domain.Rules` itself references `Interfaces` (a closed reference cycle between projects that the compiler will not allow).

```csharp
namespace TaskUnderControl.Srv.TaskModule.Interfaces.ValueObjects;

// "Soft" means: it can hold an invalid value. This is deliberate — a DTO with bad
// client data must reach this far so it can be validated, instead of failing
// during deserialization with no meaningful error.
public record SoftComplexity(int Value);
```

### `Domain.Rules.csproj Common/ModuleInfo.cs`

Once per module — the source of the rejection code's prefix (see the next block):

```csharp
namespace TaskUnderControl.Srv.TaskModule.Domain.Rules.Common;

internal static class ModuleInfo
{
    public const string ModuleName = "TaskModule";
}
```

### `Domain.Rules.csproj ComplexityRules.cs`

The single place where the condition "complexity >= 0" lives. No separate code registry file — the code is declared right here, in the format `{ModuleName}.{Class}.{Reason}`. Four members, each with its own role:

```csharp
namespace TaskUnderControl.Srv.TaskModule.Domain.Rules;

using FluentValidation;
using FluentValidation.Results;
using TaskUnderControl.Srv.TaskModule.Domain.Rules.Common;
using TaskUnderControl.Srv.TaskModule.Interfaces.ValueObjects;  // SoftComplexity

public static class ComplexityRules
{
    // Rejection code — a constant next to the rule, not in a centralized registry.
    // "+" concatenation of constants is a valid constant expression, evaluated at compile time.
    public const string NonNegativeCode = ModuleInfo.ModuleName + ".Complexity.NonNegative";

    // Ready-made default text — also a constant, but a template: const string cannot
    // hold a computed value, so the substitution happens in WithMessage((_, x) => ...) below.
    // A frontend that doesn't need localization outputs the ready-made text as-is; one
    // that needs its own takes ErrorCode + State and builds the text itself.
    public const string NonNegativeMessageTemplate = "Complexity must be non-negative, but was {0}.";

    // 1. The check itself. A pure function of an already-known value — no I/O,
    //    exactly this method is what the "rules.pure" mutation test mutates and
    //    checks directly against .feature scenarios, without VO and without FluentValidation around it.
    public static bool IsValid(this SoftComplexity c) => c.Value >= 0;

    // 2. FluentValidation wiring. The ONLY place where the error code, the
    //    default text (with a parameter), and the data for a custom text (State) are declared.
    //    Generic over T — meaning it plugs into a DTO validator, a PropertyValidator,
    //    or anywhere else that has SoftComplexity as a property.
    public static IRuleBuilderOptions<T, SoftComplexity> ComplexityIsValid<T>(
        this IRuleBuilder<T, SoftComplexity> rule)
        => rule.Must(x => x.IsValid())
               .WithErrorCode(NonNegativeCode)
               // Two-parameter WithMessage/WithState — (rootModel, propertyValue) => ...
               // The one-parameter x => x.Value binds x to T, not to SoftComplexity: it only compiles
               // while T == SoftComplexity, and breaks the first time it's projected from another DTO.
               .WithMessage((_, x) => string.Format(NonNegativeMessageTemplate, x.Value))   // "...but was -3."
               .WithState((_, x) => new { x.Value });   // a frontend MAY override the text with this data, but doesn't have to

    // 3. A convenient "just validate this one value as a whole" call —
    //    needed by the VO constructor (Domain) and by SoftComplexity.Check() elsewhere.
    //    The InlineValidator is built ONCE (static readonly + static
    //    constructor), not on every Check() call — otherwise the expression tree
    //    would be recompiled on every call, which is expensive.
    private static readonly InlineValidator<SoftComplexity> _formatValidator = new();
    static ComplexityRules() => _formatValidator.RuleFor(x => x).ComplexityIsValid();

    public static ValidationResult Check(this SoftComplexity c) => _formatValidator.Validate(c);
}
```

### `Domain.csproj ValueObjects/Complexity.cs`

```csharp
namespace TaskUnderControl.Srv.TaskModule.Domain.ValueObjects;

using TaskUnderControl.Srv.TaskModule.Domain.Rules;   // ComplexityRules extensions — Check()
using TaskUnderControl.Srv.TaskModule.Interfaces.ValueObjects;

public sealed record Complexity : SoftComplexity
{
    public Complexity(int value) : base(value)
    {
        // this.Check() — an extension method from Domain.Rules, physically defined
        // elsewhere, but looks like a method of SoftComplexity thanks to the extension.
        var result = this.Check();

        // Errors.Any(Severity == Error), NOT !result.IsValid: ValidationResult.IsValid is
        // !Errors.Any() as a whole, regardless of Severity. Right now Complexity only has
        // Error-severity reasons, so both variants behave the same — but we write the Severity
        // filter from the start, so throw-sites don't need rewriting later, once the first
        // Warning-severity rule appears.
        var blocking = result.Errors.FirstOrDefault(e => e.Severity == Severity.Error);
        if (blocking is not null)
            // ValidationFailure.ErrorCode/ErrorMessage — the same thing WithErrorCode
            // declared in Domain.Rules. The VO decides nothing itself, it just forwards it.
            throw new DomainException(blocking.ErrorCode, blocking.ErrorMessage);
    }
}
```

### `Application.csproj Validators/Property/ComplexityPropertyValidator.cs`

The body — one call to the same extension as the VO above, so this is not logic duplication. But it isn't an empty formality either — the class exists for two specific things, neither of which is about the predicate itself.

```csharp
namespace TaskUnderControl.Srv.TaskModule.Application.Validators.Property;

using FluentValidation;
using TaskUnderControl.Srv.TaskModule.Domain.Rules;
using TaskUnderControl.Srv.TaskModule.Interfaces.ValueObjects;

public class ComplexityPropertyValidator : AbstractValidator<SoftComplexity>
{
    public ComplexityPropertyValidator() => RuleFor(x => x).ComplexityIsValid();
}
```

Why it stays, instead of being deleted in favor of calling `ComplexityRules.ComplexityIsValid()` directly everywhere:

1. **DI decoupling.** The `DtoValidator` below receives `IValidator<SoftComplexity>` through its constructor and never references `Domain.Rules` — if `DtoValidator` called `.ComplexityIsValid()` directly on every one of its fields, `Application/Validators/Model` would have to reference `Domain.Rules` for every VO type used there. This way — only `Application/Validators/Property`, one file per VO, knows about `Domain.Rules`; `Model` validators know only about `IValidator<T>`.
2. **Isolated testing of a single field.** A `.feature` for `Complexity` can be bound to `ComplexityPropertyValidator` without assembling a whole valid `TodoTaskPostRequestDto` — if the Application-side adapter were `TodoTaskPostRequestDtoValidator` directly, every scenario would have to be assembled together with valid values for every other field, to avoid confusing which error comes from which field.

### `Domain.csproj Entities/TodoTask.cs` — the check inside the Entity

The Entity does not call `ComplexityRules` directly — the entire Format check is already performed inside the VO constructor. The entity method simply constructs the VO; if the value is invalid, the `Complexity` constructor throws `DomainException` itself, before the method can assign anything:

```csharp
public void UpdateComplexity(int complexity, DateTimeOffset userActionTimeStamp)
{
    Complexity = new Complexity(complexity);   // throws DomainException here if invalid
    UserUpdatedDateTime = userActionTimeStamp;
}
```

There is no separate "adapter for the Entity" for Format rules, and none is needed — the `Domain.Tests` adapter, which already calls `new Complexity(_value)` directly (see the three bindings below), checks exactly the same path that `UpdateComplexity` takes.

### `Application.csproj Validators/Model/TodoTaskPostRequestDtoValidator.cs` — the check in the DTO/Command Validator

```csharp
public class TodoTaskPostRequestDtoValidator : AbstractValidator<TodoTaskPostRequestDto>
{
    public TodoTaskPostRequestDtoValidator(IValidator<SoftComplexity> complexityValidator)
    {
        // ...
        // SetValidator — not Must/ComplexityIsValid(): DtoValidator doesn't know
        // WHAT exactly complexityValidator checks, only that the field needs to be
        // run through it. This file doesn't need Domain.Rules at all.
        RuleFor(x => x.Complexity).SetValidator(complexityValidator);
        // ...
    }
}
```

### `Domain.Rules.Spec rules/Complexity.feature`

```gherkin
Feature: Complexity must be non-negative

  Scenario Outline: Complexity boundary values
    Given complexity is <value>
    When complexity is checked
    Then the verdict is <verdict>
    And the rejection code is <code>

    Examples:
      | value | verdict  | code                       |
      | -1    | rejected | Complexity.NonNegative     |
      | 0     | accepted | -                          |
      | 100   | accepted | -                          |
```

### Three bindings — one `.feature`, three adapters

```csharp
// Domain.Rules.Tests/StepDefinitions/ComplexitySteps.cs — the "raw Check()" adapter
[When(@"^complexity is checked$")]
public void WhenComplexityIsChecked() => context.Verdict = new SoftComplexity(_value).Check();
```
```csharp
// Domain.Tests/StepDefinitions/Rules/ComplexitySteps.cs — the "VO constructor" adapter
[When(@"^complexity is checked$")]
public void WhenComplexityIsChecked()
    => context.Verdict = TestSupport.ValidationResultAdapters.FromDomainAction(
        () => new Complexity(_value));
```
```csharp
// Application.Tests/StepDefinitions/Rules/ComplexitySteps.cs — the "PropertyValidator" adapter
[When(@"^complexity is checked$")]
public void WhenComplexityIsChecked()
    => context.Verdict = new ComplexityPropertyValidator().Validate(new SoftComplexity(_value));
```

Three different assemblies, three different `[When]`s with the same step text — this is legal, step ambiguity is resolved **per assembly**, not globally across the repository. `Given`/`Then` are omitted from the example — they're reused unchanged from `TestSupport`.

Neither the Entity nor `TodoTaskPostRequestDtoValidator` has its own, fourth adapter — both only **call** places already covered by an adapter (the VO constructor and `ComplexityPropertyValidator` respectively), and contain no condition of their own. The only thing left to prove here is that `TodoTaskPostRequestDtoValidator` really did wire up `.SetValidator(complexityValidator)` for the `Complexity` field and didn't forget it — that's no longer a question of the rule's own correctness, but of "do both places that must call `ComplexityPropertyValidator` actually call it" — the same wiring architecture test as for the VO constructor.

---

## TaskTitle — several Format rules on one VO, different codes

`TaskTitle` shows why a single class can hold more than one rule: "required" and "no longer than 200 characters" — two independent conditions on the same field, each with its own rejection reason, hence its own code. Both stay in one `TaskTitleRules` class — because both are about the same `SoftTaskTitle`, and that's the criterion by which Format rules get bundled together (unlike Semantic/Domain rules — see [semantic-validation.md](./semantic-validation.md)).

Important: "bundled together" doesn't just mean "live in the same file" — it means **assembled into one combined extension** (`TaskTitleIsValid`), which is the single public entry point for the field. `RequiredRule`/`MaxLengthRule` are `private`. If the `PropertyValidator` decided for itself which of them to call (as it used to), nothing would stop it from forgetting one of the calls when a third rule is added in the future — the same "did we forget to call both" question as for two fields of one Semantic rule, just here for two rules of one field. The combined `private` method removes the question physically: there's nothing to call individually from outside.

```csharp
public static class TaskTitleRules
{
    public const string RequiredCode = ModuleInfo.ModuleName + ".TaskTitle.Required";
    public const string RequiredMessage = "Task title is required.";

    public const string MaxLengthCode = ModuleInfo.ModuleName + ".TaskTitle.MaxLength";
    public const string MaxLengthMessage = "Task title must not exceed 200 characters.";

    public static bool IsRequired(this SoftTaskTitle t) => !string.IsNullOrWhiteSpace(t.Value);
    public static bool IsMaxLength(this SoftTaskTitle t) => t.Value is null || t.Value.Length <= 200;

    // Two separate Must() calls — not one combined IsValid() — otherwise there'd be no way
    // to tell which of the two reasons fired, and so the code (and text) would be the same for both.
    // private: the only thing callable from outside (PropertyValidator, DtoValidator, Check()) is
    // TaskTitleIsValid() below. This makes it impossible to call RequiredRule and forget MaxLengthRule —
    // individually, they can't be called from outside at all, there's only one, combined entry point.
    private static IRuleBuilderOptions<T, SoftTaskTitle> RequiredRule<T>(this IRuleBuilder<T, SoftTaskTitle> rule)
        => rule.Must(x => x.IsRequired()).WithErrorCode(RequiredCode).WithMessage(RequiredMessage)
               .WithState((_, x) => new { x.Value });

    private static IRuleBuilderOptions<T, SoftTaskTitle> MaxLengthRule<T>(this IRuleBuilder<T, SoftTaskTitle> rule)
        => rule.Must(x => x.IsMaxLength()).WithErrorCode(MaxLengthCode).WithMessage(MaxLengthMessage)
               .WithState((_, x) => new { Length = x.Value?.Length });

    // The single public entry point for the field as a whole — what
    // PropertyValidator/DtoValidator/Check() must call, and the only thing they CAN call.
    // Chained on one RuleFor: IRuleBuilderOptions inherits IRuleBuilder, so
    // .RequiredRule().MaxLengthRule() adds both Must() calls to one rule, not two
    // different ones — FluentValidation collects both errors by default (CascadeMode.Continue),
    // rather than stopping at the first one.
    public static IRuleBuilderOptions<T, SoftTaskTitle> TaskTitleIsValid<T>(this IRuleBuilder<T, SoftTaskTitle> rule)
        => rule.RequiredRule().MaxLengthRule();

    private static readonly InlineValidator<SoftTaskTitle> _formatValidator = new();
    static TaskTitleRules() => _formatValidator.RuleFor(x => x).TaskTitleIsValid();

    public static ValidationResult Check(this SoftTaskTitle t) => _formatValidator.Validate(t);
}
```

`PropertyValidator` — now also a single line, like `Complexity`/`Schedule`, instead of collecting a list of rules itself:

```csharp
public class TaskTitlePropertyValidator : AbstractValidator<SoftTaskTitle>
{
    public TaskTitlePropertyValidator() => RuleFor(x => x).TaskTitleIsValid();
}
```

VO constructor — the same technique as `Complexity`, just picking the first blocking reason out of potentially two:

```csharp
public sealed record TaskTitle : SoftTaskTitle
{
    public TaskTitle(string? value) : base(value)
    {
        var result = this.Check();
        var blocking = result.Errors.FirstOrDefault(e => e.Severity == Severity.Error);
        if (blocking is not null)
            throw new DomainException(blocking.ErrorCode, blocking.ErrorMessage);
    }
}
```
