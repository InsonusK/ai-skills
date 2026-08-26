---
name: plateau-shared-rules--class-rule
description: A business predicate (Rule) in the shared-rules plateau — IsValid() + IRuleBuilder extension + Check(), covering a single field (Format), several fields of one container (Semantic), or state preloaded elsewhere (Domain), uniformly
whenToUse: when centralizing a condition already duplicated across a VO constructor, an Entity method, a PropertyValidator, or a DTO/Command validator
domain: skill
type: template
plateau: shared-rules
version: 20260824150000
tags:
  - skill/template/class
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Encode a reusable business predicate as a single source of truth: condition, rejection code, default message, and structured state, declared exactly once
- Let the same rule be called fail-fast (VO/Entity constructor) and collect-all (DTO/Command validator) without divergent logic paths

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/{Rule}.cs.create.md|{Rule}.cs.create]]

# Core Principles
- Bundle the values a rule needs into a **wrapper**, then apply `IsValid()`/`IRuleBuilder`-extension/`Check()` to that wrapper — Format, Semantic, and Domain differ only in where the wrapper's values come from, never in how the rule itself is written or wired
- **Format**: the wrapper already exists as a property of the container. **Semantic**: the wrapper is assembled on the spot from the container's own other fields, no I/O. **Domain**: the same assembly, but only after a `Load` step brings in values from elsewhere — "Domain validation = data preload + semantic validation"
- Name the wrapper (`Soft{ValueObject}`) only when the field combination is a reusable domain concept on its own; leave it an anonymous tuple when it exists only for this one comparison
- Never performs I/O — loading is always the caller's job (Handler, DI-injected async wrapper, `CustomAsync`/`MustAsync`)
- `ErrorCode`/default `Message`/`State` declared exactly once, inside the `IRuleBuilder` extension — every other adapter forwards its `ValidationResult`, never re-declares
- A blocking check reads `result.Errors.Any(e => e.Severity == Severity.Error)` (or `FirstOrDefault`), never bare `ValidationResult.IsValid`

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/{Rule}.cs.create.md|{Rule}.cs.create]]

# Naming convention
| use case | class name pattern | class name |
| -------- | ------------------- | ---------- |
| Rule on one named wrapper | `{Concept}Rules` | `ComplexityRules` |
| Rule on an anonymous-tuple wrapper | `{Concept}Rule` (singular) | `AccountWithdrawalRule` |

# Implementation

Format — the wrapper is already a container property:

```csharp
//Skill: class-rule
//Plateau: shared-rules
//Version: 20260824150000

namespace {Module}.Domain.Rules;

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

Domain — the wrapper is an anonymous tuple, assembled after a `Load` step that lives outside this class entirely:

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

For the Semantic case (wrapper assembled ad hoc from a container's own already-available fields, no I/O) and the full same-aggregate-vs-Try/Confirm decision for Domain rules crossing aggregate/service boundaries, see [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/{Rule}.cs.create.md|{Rule}.cs.create]] — the worked examples there (`ScheduleRules`, the Try/Confirm sequence diagram) are the authoritative reference this class file summarizes.

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/{Rule}.cs.create.md|{Rule}.cs.create]]

# Rules
MUST:
- Declare `ErrorCode`/default `Message`/`State` exactly once, inside the `IRuleBuilder` extension
- Use `result.Errors.FirstOrDefault(e => e.Severity == Severity.Error)`/`.Any(...)` to decide whether to throw — never bare `ValidationResult.IsValid`
- Name the wrapper only when the field combination is a reusable domain concept; otherwise use an anonymous tuple
- Perform the actual comparison inside `Domain.Rules`, over already-loaded raw values — never accept a pre-computed boolean verdict
MUST NOT:
- Re-declare `WithErrorCode`/`WithMessage` at a call site that already has a `Check()` to forward from
- Reference a repository or `DbContext` anywhere
- Be instantiated with `new` — rules are static

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/{Rule}.cs.create.md|{Rule}.cs.create]]

# Check list
- [ ] Every rejection code is `public const string` next to the rule that produces it, format `{ModuleName}.{Class}.{Reason}`
- [ ] `IsValid()` is a pure, synchronous predicate with no I/O
- [ ] `Check()` reuses a `static readonly InlineValidator<TWrapper>` built once, not per call
- [ ] A Domain-classified rule's comparison happens inside the rule itself, over already-loaded raw values

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/{Rule}.cs.create.md|{Rule}.cs.create]]
