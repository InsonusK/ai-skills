---
description: A business predicate — IsValid() + IRuleBuilder extension + Check() — covering a single field (Format), several fields of one container (Semantic), or state preloaded from another Entity (Domain), uniformly
project_name: "{Module}.Domain.Rules"
name: "{Rule}"
element_kind: class
change_kind: create
tags:
  - solution/domain-rules
  - element/rule-cs
---

# Goals
- Encode a reusable business predicate as a single source of truth: condition, rejection code, default message, and structured state, declared exactly once
- Let the same rule be called fail-fast (VO/Entity constructor) and collect-all (DTO/Command validator) without divergent logic paths

# Core Principles
- A rule is always: bundle the values it needs into a **wrapper**, then apply `IsValid()`/`IRuleBuilder`-extension/`Check()` to that wrapper. What differs between Format, Semantic, and Domain is only where the wrapper's values come from — never how the rule itself is written or wired.
- **Format**: the wrapper already exists as a property of the container (`Complexity : SoftComplexity`).
- **Semantic**: the wrapper is assembled on the spot from the container's own other fields, no I/O (`new SoftSchedule(dto.StartDateTime, dto.DueDateTime)`).
- **Domain**: the wrapper is assembled the same way, but only after a `Load` step brings in values from another Entity or a repository — "Domain validation = data preload + semantic validation." The rule itself never knows or cares which arrow it came from.
- Name the wrapper (a `Soft{ValueObject}`) only when the combination is a reusable domain concept on its own; leave it an anonymous tuple when it exists only for this one comparison. A named wrapper additionally justifies a `PropertyValidator`-equivalent DI/isolated-testing layer; a rule-local tuple usually does not need one.

# Naming convention

| use case | class name pattern | class name |
| -------- | ------------------- | ---------- |
| Rule on one named wrapper | `{Concept}Rules` | `ComplexityRules` |
| Rule on an anonymous-tuple wrapper | `{Concept}Rule` (singular) | `TaskLinkSelfLinkRule` |

Singular vs. plural: `{Concept}Rule` (singular) when the class holds exactly one condition on a tuple wrapper; `{Concept}Rules` (plural) when it holds one or more conditions that all belong to the same named wrapper — a second, unrelated condition on that same wrapper gets its own class, not a second method here.

# Implementation changes

## Format — the wrapper is already a container property

```csharp
namespace {Module}.Domain.Rules;

using FluentValidation;
using FluentValidation.Results;
using {Module}.Domain.Rules.Common;
using {Module}.Interfaces.ValueObjects;

public static class {ValueObject}Rules
{
    // Rejection code — constant next to the rule, not in a central registry.
    public const string {Reason}Code = ModuleInfo.ModuleName + ".{ValueObject}.{Reason}";
    public const string {Reason}MessageTemplate = "{human-readable message with a {0} placeholder}";

    // 1. Pure predicate — no I/O. This is what mutation testing mutates and .feature scenarios prove.
    public static bool IsValid(this Soft{ValueObject} value) => /* condition over value's own fields */;

    // 2. FluentValidation wiring — the ONLY place ErrorCode/Message/State are declared.
    public static IRuleBuilderOptions<T, Soft{ValueObject}> {ValueObject}IsValid<T>(
        this IRuleBuilder<T, Soft{ValueObject}> rule)
        => rule.Must(x => x.IsValid())
               .WithErrorCode({Reason}Code)
               .WithMessage((_, x) => string.Format({Reason}MessageTemplate, x.Value))
               .WithState((_, x) => new { x.Value });

    // 3. Convenience for "just validate me one full value" — needed by the VO constructor
    //    and by Soft{ValueObject}.Check() from other call sites.
    private static readonly InlineValidator<Soft{ValueObject}> _validator = new();
    static {ValueObject}Rules() => _validator.RuleFor(x => x).{ValueObject}IsValid();
    public static ValidationResult Check(this Soft{ValueObject} value) => _validator.Validate(value);
}
```

Worked example (`TaskModule`, single condition on `SoftComplexity`):

```csharp
namespace TaskUnderControl.Srv.TaskModule.Domain.Rules;

using FluentValidation;
using FluentValidation.Results;
using TaskUnderControl.Srv.TaskModule.Domain.Rules.Common;
using TaskUnderControl.Srv.TaskModule.Interfaces.ValueObjects;

public static class ComplexityRules
{
    public const string NonNegativeCode = ModuleInfo.ModuleName + ".Complexity.NonNegative";
    public const string NonNegativeMessageTemplate = "Complexity must be non-negative, but was {0}.";

    public static bool IsValid(this SoftComplexity c) => c.Value >= 0;

    public static IRuleBuilderOptions<T, SoftComplexity> ComplexityIsValid<T>(
        this IRuleBuilder<T, SoftComplexity> rule)
        => rule.Must(x => x.IsValid())
               .WithErrorCode(NonNegativeCode)
               .WithMessage((_, x) => string.Format(NonNegativeMessageTemplate, x.Value))
               .WithState((_, x) => new { x.Value });

    private static readonly InlineValidator<SoftComplexity> _formatValidator = new();
    static ComplexityRules() => _formatValidator.RuleFor(x => x).ComplexityIsValid();
    public static ValidationResult Check(this SoftComplexity c) => _formatValidator.Validate(c);
}
```

Two independent conditions on the same wrapper (`TaskTitle`: required + max length) bundle into **one combined, public extension**; the individual `Must()` calls stay `private` so nothing can call one and forget the other:

```csharp
public static class TaskTitleRules
{
    public const string RequiredCode = ModuleInfo.ModuleName + ".TaskTitle.Required";
    public const string MaxLengthCode = ModuleInfo.ModuleName + ".TaskTitle.MaxLength";

    public static bool IsRequired(this SoftTaskTitle t) => !string.IsNullOrWhiteSpace(t.Value);
    public static bool IsMaxLength(this SoftTaskTitle t) => t.Value is null || t.Value.Length <= 200;

    private static IRuleBuilderOptions<T, SoftTaskTitle> RequiredRule<T>(this IRuleBuilder<T, SoftTaskTitle> rule)
        => rule.Must(x => x.IsRequired()).WithErrorCode(RequiredCode).WithMessage("Task title is required.");

    private static IRuleBuilderOptions<T, SoftTaskTitle> MaxLengthRule<T>(this IRuleBuilder<T, SoftTaskTitle> rule)
        => rule.Must(x => x.IsMaxLength()).WithErrorCode(MaxLengthCode).WithMessage("Task title must not exceed 200 characters.");

    // Single public entry point — RequiredRule/MaxLengthRule cannot be called individually from outside.
    public static IRuleBuilderOptions<T, SoftTaskTitle> TaskTitleIsValid<T>(this IRuleBuilder<T, SoftTaskTitle> rule)
        => rule.RequiredRule().MaxLengthRule();

    private static readonly InlineValidator<SoftTaskTitle> _formatValidator = new();
    static TaskTitleRules() => _formatValidator.RuleFor(x => x).TaskTitleIsValid();
    public static ValidationResult Check(this SoftTaskTitle t) => _formatValidator.Validate(t);
}
```

## Semantic — the wrapper is assembled from the container's own fields

Two ways to assemble the wrapper — pick per rule:

**Named wrapper**, when the field combination is itself a reusable domain concept (e.g. `Schedule`, a task's time window):

```csharp
public static class ScheduleRules
{
    public const string WindowInvertedCode = ModuleInfo.ModuleName + ".Schedule.WindowInverted";

    public static bool IsValid(this SoftSchedule s)
        => s.StartDateTime is null || s.DueDateTime is null || s.DueDateTime >= s.StartDateTime;

    public static IRuleBuilderOptions<T, SoftSchedule> ScheduleIsValid<T>(this IRuleBuilder<T, SoftSchedule> rule)
        => rule.Must(x => x.IsValid())
               .WithErrorCode(WindowInvertedCode)
               .WithMessage("Due date must not be earlier than start date.")
               .WithState((_, x) => new { x.StartDateTime, x.DueDateTime });

    private static readonly InlineValidator<SoftSchedule> _validator = new();
    static ScheduleRules() => _validator.RuleFor(x => x).ScheduleIsValid();
    public static ValidationResult Check(this SoftSchedule s) => _validator.Validate(s);
}
```

A DTO that stores `StartDateTime`/`DueDateTime` as two separate fields (not a `Schedule` property) assembles the wrapper ad hoc, reusing the same rule with no duplicated condition:

```csharp
public class TodoTaskPostRequestDtoValidator : AbstractValidator<TodoTaskPostRequestDto>
{
    public TodoTaskPostRequestDtoValidator()
        => RuleFor(dto => new SoftSchedule(dto.StartDateTime, dto.DueDateTime)).ScheduleIsValid();
}
```

**Anonymous tuple**, when the combination has no meaning beyond this one comparison (e.g. `TaskLink` not referencing itself):

```csharp
public static class TaskLinkSelfLinkRule
{
    public const string Code = ModuleInfo.ModuleName + ".TaskLinkSelfLink.SelfLink";

    public static bool IsNotSelfLink<T>(this (T Parent, T Child) ids) where T : IEquatable<T>
        => !ids.Parent.Equals(ids.Child);

    public static IRuleBuilderOptions<TRoot, (T Parent, T Child)> IsNotSelfLink<TRoot, T>(
        this IRuleBuilder<TRoot, (T Parent, T Child)> rule) where T : IEquatable<T>
        => rule.Must(x => x.IsNotSelfLink()).WithErrorCode(Code).WithMessage("A task cannot be linked to itself.");

    private static readonly InlineValidator<(int Parent, int Child)> _validator = new();
    static TaskLinkSelfLinkRule() => _validator.RuleFor(x => x).IsNotSelfLink();
    public static ValidationResult Check(this (int Parent, int Child) ids) => _validator.Validate(ids);
}
```

No `Soft{ValueObject}`/`PropertyValidator` layer exists for a rule-local tuple — it binds only to the layers that actually consume it (Entity, DtoValidator), not to a DI-resolvable type nobody else needs.

## Domain — the wrapper is assembled from data loaded elsewhere

Same mechanism as Semantic; the only new concern is the loading step, which lives in the caller, never in the rule. Worked example (hypothetical `Account`/`Transaction`, one aggregate, `Account` the root):

```csharp
public static class AccountWithdrawalRule
{
    public const string InsufficientBalanceCode = ModuleInfo.ModuleName + ".AccountWithdrawal.InsufficientBalance";

    // Raw values, not a pre-computed bool — the comparison happens here, so mutation
    // testing scoped to Domain.Rules has something real to mutate.
    public static bool CanWithdraw(this (decimal Balance, decimal Amount) tx) => tx.Amount <= tx.Balance;

    public static IRuleBuilderOptions<T, (decimal Balance, decimal Amount)> CanWithdraw<T>(
        this IRuleBuilder<T, (decimal Balance, decimal Amount)> rule)
        => rule.Must(x => x.CanWithdraw())
               .WithErrorCode(InsufficientBalanceCode)
               .WithMessage((_, x) => $"Withdrawal amount exceeds account balance. Balance {x.Balance} < Amount {x.Amount}.")
               .WithState((_, x) => new { x.Balance, x.Amount });

    private static readonly InlineValidator<(decimal Balance, decimal Amount)> _validator = new();
    static AccountWithdrawalRule() => _validator.RuleFor(x => x).CanWithdraw();
    public static ValidationResult Check(this (decimal Balance, decimal Amount) tx) => _validator.Validate(tx);
}
```

The async loading step (not part of `Domain.Rules` — lives in `solution-dto-property-validators`'s async validator wrapper) preloads `(Balance, Amount)` via a repository, then calls `.Check()` on it and forwards the result — no comparison logic duplicated at the async call site:

```csharp
public sealed class TransactionWithdrawalCheck(IReadRepository<Transaction> transactionRepository)
{
    public async Task CheckAsync(
        UpdateTransactionAmountCommand cmd, ValidationContext<UpdateTransactionAmountCommand> context, CancellationToken ct)
    {
        var transaction = await transactionRepository.FirstOrDefaultAsync(
            new TransactionByIdWithAccountSpec(cmd.TransactionId), ct);
        if (transaction?.Account is null) return; // existence is a different check

        var result = (transaction.Account.Balance, cmd.Payload.NewAmount - transaction.Amount).Check();
        foreach (var failure in result.Errors) context.AddFailure(failure);
    }
}
```

### Same-aggregate vs. cross-aggregate — when Try/Confirm is required

`Account.Withdraw` reading and writing `Balance` synchronously is safe only because `Account`'s own `Version`/write-lock serializes concurrent withdrawals within one aggregate. The moment `Account` and `Transaction` are different aggregates (even in the same service) or different services, this stops being safe without extra serialization — the correct move is always Try/Confirm, never a hand-rolled synchronization scheme:

- **Try**: create the dependent Entity `Pending`, using a preliminary check against a possibly-stale local replica — fast, non-authoritative.
- **Confirm**: the owning aggregate's existing, unmodified rule/method (`Account.Withdraw`, same `AccountWithdrawalRule.Check()`) runs authoritatively, asynchronously, publishing `Confirmed`/`Rejected`.

The rule (`AccountWithdrawalRule`) does not change between the same-aggregate and Try/Confirm cases — only how and when it gets called changes. See [[skills/dotnet/architecture/v3.1/solutions/solution-domain-rules.skill/solution-domain-rules.skill#boundaries|Boundaries]] for why the saga's Handler/Consumer wiring is not part of this solution.

# Rule changes

## MUST
- Declare `ErrorCode`/default `Message`/`State` exactly once, inside the `IRuleBuilder` extension — never re-declared by a `PropertyValidator`, a DTO validator, or a VO constructor
- Use `result.Errors.FirstOrDefault(e => e.Severity == Severity.Error)` (or `.Any(...)`) to decide whether to throw — never bare `ValidationResult.IsValid`
- Name the wrapper only when the field combination is a reusable domain concept; otherwise use an anonymous tuple
- Assemble a Semantic wrapper from the container's own already-available fields — never perform I/O to build it
- Perform the actual comparison inside `Domain.Rules`, over already-loaded raw values — never pass a pre-computed boolean verdict into a rule
- Load data only in the caller (Handler, DI-injected async wrapper, `CustomAsync`) — `Domain.Rules` never references a repository or `DbContext`
- Combine two or more conditions on the same wrapper into one public extension with `private` individual `Must()` calls
- Never re-declare `WithErrorCode`/`WithMessage` at a call site that already has a `Check()` to forward from — a legitimate new `ErrorCode` constant only appears at an async call site with no sync counterpart
- Never reimplement logic that already exists in another rule
- Never instantiate a rule with `new` — rules are static, never instantiated

## SHOULD
- Prefer `CustomAsync` + forwarding an existing `Check()`'s `ValidationResult.Errors` over `MustAsync` + manually re-declaring `WithErrorCode`/`WithMessage`, whenever a synchronous counterpart already exists

# Check list
- [ ] Every rejection code is `public const string` next to the rule that produces it, format `{ModuleName}.{Class}.{Reason}`
- [ ] Every rule has exactly one `IRuleBuilder` extension declaring `Must`/`WithErrorCode`/`WithMessage`/`WithState`
- [ ] `IsValid()` is a pure, synchronous predicate with no I/O
- [ ] `Check()` reuses a `static readonly InlineValidator<TWrapper>` built once, not per call
- [ ] A Domain-classified rule's comparison happens inside the rule, over already-loaded raw values
- [ ] A cross-aggregate/cross-service Domain rule is documented as requiring Try/Confirm, not ad hoc synchronization

# Unittest TestCases
- [ ] WHEN applied THEN the rule's condition is a single source of truth — no other file re-declares it
- [ ] WHEN a wrapper is a named Soft{ValueObject} THEN Format usage (VO ctor) and Semantic usage (ad hoc assembly) both resolve to the same Check()
- [ ] WHEN two conditions share one wrapper THEN calling the combined extension runs both and the individual Must() calls are not independently callable
- [ ] WHEN a Domain rule's data comes from another aggregate THEN the rule itself performs the comparison over already-loaded values, never a pre-computed bool
