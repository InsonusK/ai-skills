---
name: class-rule
description: Create a domain rule — a centralized business predicate (IsValid() + IRuleBuilder extension + Check()) that VO constructors, Entity methods, PropertyValidators, and DTO/Command validators all call the same way
domain: skill
type: template
version: 20260821
plateau: default
tags:
  - skill/template/class
  - plateau/default
  - stack/dotnet
  - concern/architecture

created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]]"
---

# Goal
- Give a condition duplicated across a VO constructor, an Entity method, a PropertyValidator, and a DTO/Command validator exactly one place where it is declared
- Prevent duplication of business conditions across VO, Entity, PropertyValidator, and DTO/Command validator
- Separate the predicate from the enforcement mechanism (fail-fast vs collect-all)

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/{Rule}.cs.create|{Rule}.cs]]

# Core Principles
- Apply ONE plateau template per class
- Static class with `IsValid()` (pure predicate) + `IRuleBuilder<T,TValue>` extension (declares `ErrorCode`/`Message`/`State`) + `Check()` (convenience wrapping a `static readonly InlineValidator<TValue>`)
- `IsValid()` performs no I/O; loading (when needed) is always the caller's job
- Classified as Format (wrapper already exists as a container property), Semantic (wrapper assembled from the container's own fields), or Domain (wrapper assembled from data loaded elsewhere) — the rule itself never knows which; all three use the identical shape
- `ErrorCode`/`Message`/`State` are declared exactly once, inside the `IRuleBuilder` extension — every consumer calls it or forwards its `ValidationResult`, never re-declares them
- Lives in a dedicated `{Module}.Domain.Rules` project — referencing only FluentValidation and `{Module}.Interfaces`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/{Rule}.cs.create|{Rule}.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Rule on a named wrapper | {Concept}Rules | ComplexityRules | {Concept}Rules.cs | ComplexityRules.cs |
| Rule on an anonymous-tuple wrapper | {Concept}Rule (singular) | TaskLinkSelfLinkRule | {Concept}Rule.cs | TaskLinkSelfLinkRule.cs |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/{Rule}.cs.create|{Rule}.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-rule
//Plateau: default
//Version: 20260821
```

## Format — wrapper already exists as a container property

```csharp
namespace {Module}.Domain.Rules;

using FluentValidation;
using FluentValidation.Results;
using {Module}.Domain.Rules.Common;
using {Module}.Interfaces.ValueObjects;

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

    private static readonly InlineValidator<SoftComplexity> _validator = new();
    static ComplexityRules() => _validator.RuleFor(x => x).ComplexityIsValid();
    public static ValidationResult Check(this SoftComplexity c) => _validator.Validate(c);
}
```

## Semantic — wrapper assembled from the container's own fields

Named wrapper (reusable concept, e.g. `Schedule`):

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

Anonymous-tuple wrapper (no meaning beyond this one comparison, e.g. self-link check):

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

## Domain — wrapper assembled from data loaded elsewhere

Same mechanism as Semantic; loading happens only in the caller (Handler, DI-injected async wrapper, `CustomAsync`), never in the rule:

```csharp
public static class AccountWithdrawalRule
{
    public const string InsufficientBalanceCode = ModuleInfo.ModuleName + ".AccountWithdrawal.InsufficientBalance";

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

A same-aggregate Domain rule stays synchronous; a rule whose data lives in a different aggregate or service becomes a Try/Confirm process instead of ad hoc synchronization — see [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules' own Workflow]].

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/{Rule}.cs.create|{Rule}.cs]]

# Rules
MUST:
	- Be a `static class` in the `{Module}.Domain.Rules` project
	- Declare `IsValid()` + one `IRuleBuilder` extension + `Check()`
	- Declare `ErrorCode`/`Message`/`State` exactly once, inside the `IRuleBuilder` extension
	- Name the wrapper only when the field combination is a reusable domain concept; use an anonymous tuple otherwise
	- A Domain-classified rule performs the comparison itself, over already-loaded raw values — never over a pre-computed verdict
MUST NOT:
	- Perform I/O inside `IsValid()`, the `IRuleBuilder` extension, or `Check()`
	- Reference a repository, `DbContext`, or `{Module}.Domain`/`{Module}.Application`
	- Be re-declared (`Must`/`WithErrorCode`/`WithMessage`) by a consumer that already has this rule to call
	- Be instantiated with `new` — always static

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/{Rule}.cs.create|{Rule}.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN a condition duplicated across two or more consumers has exactly one declaration
- [ ] WHEN applied THEN IsValid() is a pure predicate with no I/O
- [ ] WHEN applied THEN ErrorCode/Message/State are declared only in the IRuleBuilder extension
- [ ] WHEN a wrapper is a named Soft{ValueObject} THEN Format and Semantic usage both resolve to the same Check()
- [ ] WHEN a Domain-classified rule runs THEN it receives already-loaded raw values, never a pre-computed bool

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/{Rule}.cs.create|{Rule}.cs]]
