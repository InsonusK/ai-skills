---
name: class-feature-check
description: Class {Feature}Check in the shared-rules plateau
whenToUse: when a Command validator needs to reject an invalid request before the Handler runs, for a condition whose data must be loaded from a repository
domain: skill
type: template
plateau: shared-rules
version: 20260824163000
tags:
  - skill/template/class
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]"
  - "[[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Let a Command validator reject an invalid request before the Handler runs, for a condition whose data needs to be loaded from a repository

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.create.md|{Feature}Check.cs.create]]

# Core Principles
- Loading is this class's job — a `{Dto}Validator`/`{ValueObject}PropertyValidator` never performs I/O
- The condition is written locally in this class, alongside the loading step — the owning Entity's own method (`solution-domain-behaviour`) enforces the same invariant independently, as the authoritative backstop
- Wired into the pipeline via `CustomAsync`, not `MustAsync` spread across the validator body
- `Load` injects `IReadRepository<T>` and queries through a named spec — never `DbContext`, never inline LINQ. This plateau composes `plateau-statefull-service` as its parent, so this concrete realization is inherited unchanged, not re-derived here
- Once the same condition is found duplicated elsewhere (an Entity method, a PropertyValidator), `CheckAsync`'s local comparison is redirected to a centralized `{Rule}.Check()` — see below

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.create.md|{Feature}Check.cs.create]]
- [[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../../solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.extend.md|{Feature}Check.cs.extend]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Async cross-aggregate check | {Feature}Check | TransactionWithdrawalCheck | {Feature}Check.cs | TransactionWithdrawalCheck.cs |

# Implementation
```csharp
//Skill: class-feature-check
//Plateau: shared-rules
//Version: 20260824163000

public sealed class TransactionWithdrawalCheck(IReadRepository<Transaction> transactionRepository)
{
    private async Task<(decimal Balance, decimal Amount)?> Load(UpdateTransactionAmountCommand cmd, CancellationToken ct)
    {
        var transaction = await transactionRepository.FirstOrDefaultAsync(
            new TransactionByIdWithAccountSpec(cmd.TransactionId), ct);

        if (transaction?.Account is null)
            return null; // existence is a different check, not this one

        return (transaction.Account.Balance, cmd.Payload.NewAmount - transaction.Amount);
    }

    public async Task CheckAsync(
        UpdateTransactionAmountCommand cmd, ValidationContext<UpdateTransactionAmountCommand> context, CancellationToken ct)
    {
        var loaded = await Load(cmd, ct);
        if (loaded is null)
            return;

        if (loaded.Value.Amount > loaded.Value.Balance)
            context.AddFailure(nameof(cmd.Payload.NewAmount), "Withdrawal amount exceeds account balance.");
    }
}
```

Wired into the Command validator: `RuleFor(x => x).CustomAsync(check.CheckAsync)`.

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.create.md|{Feature}Check.cs.create]]
- [[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../../solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.extend.md|{Feature}Check.cs.extend]]

## Once the same condition is duplicated elsewhere: forward a centralized Check() instead of comparing locally

Optional, applied only once the same comparison is found duplicated in an Entity method (e.g. `Account.Withdraw`). The local `if (...) context.AddFailure(...)` is deleted, not kept alongside — no `ErrorCode`/`Message` re-declared here, both are forwarded from `{Module}.Domain.Rules`:

```csharp
public async Task CheckAsync(
    UpdateTransactionAmountCommand cmd, ValidationContext<UpdateTransactionAmountCommand> context, CancellationToken ct)
{
    var loaded = await Load(cmd, ct);
    if (loaded is null)
        return;

    var result = loaded.Value.Check();   // AccountWithdrawalRule.Check() — the same InlineValidator Account.Withdraw uses
    foreach (var failure in result.Errors)
        context.AddFailure(failure);
}
```

`Load` and the constructor are untouched by this redirect — they keep the concrete `IReadRepository<T>` realization this plateau inherits from `plateau-statefull-service`. Only `CheckAsync`'s body changes. See [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] and its [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.extend.md|{Feature}Check.cs.extend]]. `.Check()` itself is real and testable now (see `{Module}.Domain.Rules.Tests`), and — because this plateau's parent is `plateau-statefull-service`, not `plateau-service-with-validated-module-interaction` — `Load` is real too, not a stub: this is the first plateau in the lineage where both halves of `{Feature}Check` are genuinely usable at once.

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.extend.md|{Feature}Check.cs.extend]]

# Rules
MUST:
- Load data only inside this class, live in `/{Module}.Application/Validators/Async`
- Own its condition locally in this class, next to the loading step
- Be wired into its Command validator via `RuleFor(x => x).CustomAsync(...)`
- Inject `IReadRepository<T>` for the entity this check needs, query through a named spec — never `DbContext`, never inline LINQ
- Forward an existing `Check()`'s `ValidationResult.Errors` via `context.AddFailure(failure)` instead of comparing locally, once redirected — delete the local comparison, never keep both
SHOULD:
- Return early (no failure added) when the data needed to run the check could not be loaded at all

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.create.md|{Feature}Check.cs.create]]
- [[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../../solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.extend.md|{Feature}Check.cs.extend]]
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.extend.md|{Feature}Check.cs.extend]]

# Check list
- [ ] Loads data, then checks it locally, in the same class
- [ ] Wired via `RuleFor(x => x).CustomAsync(check.CheckAsync)`
- [ ] The same condition's Entity-side enforcement still runs, independent of this check
- [ ] `Load` injects `IReadRepository<T>` and queries through a named spec — no `NotSupportedException` stub remains
- [ ] If redirected to a centralized `Check()`, the local comparison is deleted, not kept alongside

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.create.md|{Feature}Check.cs.create]]
- [[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../../solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.extend.md|{Feature}Check.cs.extend]]
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.extend.md|{Feature}Check.cs.extend]]
