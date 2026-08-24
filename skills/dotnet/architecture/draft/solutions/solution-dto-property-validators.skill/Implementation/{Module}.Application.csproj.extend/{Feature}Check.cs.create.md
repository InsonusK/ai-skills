---
description: DI-injected async wrapper that preloads data and checks a locally-owned cross-aggregate condition — the only place a repository call is allowed in this solution
project_name: "{Module}.Application"
name: "{Feature}Check.cs"
element_kind: class
change_kind: create
tags:
  - solution/dto-property-validators
  - element/feature-check-cs
---

# Goals
- Let a Command validator reject an invalid request before the Handler runs, for a condition whose data needs to be loaded from a repository

# Core Principles
- Loading is this class's job — a `{Dto}Validator`/`{ValueObject}PropertyValidator` never performs I/O
- The condition is written locally in this class, alongside the loading step — this solution owns it and does not require a shared rules abstraction. The owning Entity's own method (`solution-domain-behaviour`) enforces the same invariant independently, as the authoritative backstop; keeping the two conditions in agreement is a manual concern today (see Boundaries in the parent solution)
- `CustomAsync`, not `MustAsync` alone spread across the validator body — the whole loading-and-checking method lives in this DI-injected class, not inline in the validator, so it can be tested in isolation
- This solution's own `built_on_plateau` (`plateau-stateless-non-interactive-service`) has no repository or other data-loading abstraction yet — `{Feature}Check` only has something to load from once the module has one (e.g. `IReadRepository<T>`, added by `solution-repository-integration`, composed in `plateau-statefull-service`). The worked example below injects `IReadRepository<T>` to show the concrete shape; treat it as illustrative until that abstraction actually exists in the module you're applying this to

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Async cross-aggregate check | `{Feature}Check` | `TransactionWithdrawalCheck` | `{Feature}Check.cs` | `TransactionWithdrawalCheck.cs` |

# Implementation changes

Worked example (`AccountModule`, hypothetical `Account`/`Transaction`) — preload, then check locally:

```csharp
// {Module}.Application/Validators/Async/TransactionWithdrawalCheck.cs
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

    // Signature for FluentValidation CustomAsync: (value, ValidationContext, CancellationToken) -> Task.
    // The comparison is local to this class — the same condition the Account entity's own
    // Withdraw method enforces independently (solution-domain-behaviour), not shared code today.
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

Wired into the Command validator:

```csharp
public sealed class UpdateTransactionAmountValidator : AbstractValidator<UpdateTransactionAmountCommand>
{
    public UpdateTransactionAmountValidator(TransactionWithdrawalCheck check)
        => RuleFor(x => x).CustomAsync(check.CheckAsync);
}
```

This runs in the pipeline **before** the Handler — the client gets the rejection without the Handler running. The Handler's own preload and the Entity method's own check remain the authoritative backstop: a second Handler, a background job, or a future caller that bypasses this validator still runs into the same condition inside the Entity.

# Rule changes

## MUST
- Load data only inside this class
- Own its condition locally in this class, next to the loading step
- Be wired into its Command validator via `RuleFor(x => x).CustomAsync(...)`

## SHOULD
- Return early (no failure added) when the data needed to run the check could not be loaded at all — existence is a separate check, not this one

# Check list
- [ ] `{Feature}Check` loads data, then checks it locally — the loading step and the condition live in the same class
- [ ] Wired via `RuleFor(x => x).CustomAsync(check.CheckAsync)`
- [ ] The same condition's Entity-side enforcement (via `solution-domain-behaviour`) still runs, independent of this validator

# Unittest TestCases
- [ ] WHEN the preloaded data fails the condition THEN a validation error is added to the ValidationContext
- [ ] WHEN required data cannot be loaded THEN no failure is added by this check (existence is a separate concern)
