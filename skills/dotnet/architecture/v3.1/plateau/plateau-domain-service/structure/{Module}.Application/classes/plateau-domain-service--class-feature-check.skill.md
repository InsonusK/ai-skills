---
name: plateau-domain-service--class-feature-check
description: Class {Feature}Check in the plateau-domain-service plateau — the DI-injected async cross-aggregate check seam; its Load step has no data source until VP2
whenToUse: when creating or editing an async command-level check in {Module}.Application/Validators/Async, or deciding whether a check needs preloaded data
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]"
---

# Goal
- Let a command validator reject an invalid request before the handler runs, for a condition whose data must be loaded.
- At plateau-core this class defines the **shape** only — a DI-injected wrapper with a `Load` step and a `CheckAsync` step wired via `CustomAsync`. `Load` has no data source until a persistence solution (VP2) supplies one through its own `.extend.md`.

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.create.md|{Feature}Check.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- Loading is this class's job — a `{Dto}Validator` / `{ValueObject}PropertyValidator` never performs I/O.
- The condition is written locally, next to the loading step; the owning entity's own method (VP1) enforces the same invariant independently as the authoritative backstop.
- Wired into the command validator via `RuleFor(x => x).CustomAsync(check.CheckAsync)`.
- Does not reference a concrete data-loading abstraction (e.g. `IReadRepository<T>`) in this file — that arrives via `solution-repository-integration`'s `{Feature}Check.cs.extend.md` (VP2). Until then `Load` throws `NotSupportedException`.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Async cross-aggregate check | `{Feature}Check` | `TransactionWithdrawalCheck` | `{Feature}Check.cs` | `TransactionWithdrawalCheck.cs` |

# Implementation
```csharp
// Skill: plateau-domain-service--class-feature-check
// Plateau: core
// Version: 20260902000000
using FluentValidation;

namespace {Module}.Application.Validators.Async;

public sealed class TransactionWithdrawalCheck
{
    private Task<(decimal Balance, decimal Amount)?> Load(UpdateTransactionAmountCommand cmd, CancellationToken ct)
        => throw new NotSupportedException(
            "TransactionWithdrawalCheck has no data-loading abstraction composed yet (arrives with VP2).");

    public async Task CheckAsync(
        UpdateTransactionAmountCommand cmd,
        ValidationContext<UpdateTransactionAmountCommand> context,
        CancellationToken ct)
    {
        var loaded = await Load(cmd, ct);
        if (loaded is null) return;

        if (loaded.Value.Amount > loaded.Value.Balance)
            context.AddFailure(nameof(cmd.NewAmount), "Withdrawal amount exceeds account balance.");
    }
}
```
Wired into the validator:
```csharp
public sealed class UpdateTransactionAmountValidator : AbstractValidator<UpdateTransactionAmountCommand>
{
    public UpdateTransactionAmountValidator(TransactionWithdrawalCheck check)
        => RuleFor(x => x).CustomAsync(check.CheckAsync);
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.create.md|{Feature}Check.cs.create]]

# Rules
MUST:
- Load data only inside this class; own the condition locally, next to `Load`.
- Be wired into its command validator via `RuleFor(x => x).CustomAsync(...)`.
- Never reference a concrete data-loading abstraction in this file at plateau-core — leave `Load` throwing until VP2's `.extend.md` supplies the body.
- Return early (no failure) when the data needed could not be loaded — existence is a separate check.
- Never apply several plateau templates per class.

# Check list
- [ ] `{Feature}Check` has a `Load` step and a `CheckAsync` step in one class, in `/Validators/Async`.
- [ ] Wired via `RuleFor(x => x).CustomAsync(check.CheckAsync)`.
- [ ] `Load` throws (no concrete abstraction) at plateau-core.

# Unittest TestCases
- [ ] WHEN preloaded data fails the condition THEN a failure is added to the `ValidationContext`.
- [ ] WHEN required data cannot be loaded THEN no failure is added.
