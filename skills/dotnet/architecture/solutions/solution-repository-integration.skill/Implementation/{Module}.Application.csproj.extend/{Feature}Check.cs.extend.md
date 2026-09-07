---
description: Give {Feature}Check's Load method a real IReadRepository<T> implementation, now that persistence is composed
project_name: "{Module}.Application"
name: "{Feature}Check.cs"
element_kind: class
change_kind: extend
tags:
  - solution/repository-integration
  - element/feature-check-cs
---

# Goals
- Give `{Feature}Check`'s `Load` method something real to load from, now that `IReadRepository<T>` and named specs exist

# Implementation changes

Before (per `solution-dto-property-validators`, no data-loading abstraction assumed):

```csharp
public sealed class TransactionWithdrawalCheck
{
    private Task<(decimal Balance, decimal Amount)?> Load(UpdateTransactionAmountCommand cmd, CancellationToken ct)
    {
        throw new NotSupportedException(
            "TransactionWithdrawalCheck has no data-loading abstraction composed yet.");
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

After (constructor takes `IReadRepository<Transaction>`, `Load` queries it via a named spec):

```csharp
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

Only `Load` and the constructor change — `CheckAsync` is untouched, still owned by `solution-dto-property-validators`. `TransactionByIdWithAccountSpec` follows the same named-spec convention as every other query in this solution (see `{Entity}ByIdSpec.cs.create.md`).

# Rule changes

## MUST
- Inject `IReadRepository<T>` for the entity `{Feature}Check` needs — never `DbContext`
  - Risk: injecting `DbContext` directly bypasses the repository abstraction this solution exists to enforce, and reintroduces the coupling `solution-repository-integration`'s own Check list already forbids everywhere else.
  - Fix: constructor-inject `IReadRepository<T>` for the entity being checked, exactly like every other read path this solution defines.
- Query through a named spec (e.g. `TransactionByIdWithAccountSpec`) — no inline LINQ.
  - Risk: an inline LINQ query here duplicates the same "no raw LINQ on repository methods" violation this solution's Anti-patterns section already calls out for handlers.
  - Fix: define a named `Specification<T>` next to the check's feature, following this solution's own spec-naming convention.
- Leave `CheckAsync` unchanged — only `Load` and the constructor gain a real implementation.
  - Risk: touching `CheckAsync` here duplicates content `solution-dto-property-validators` already owns and creates two sources of truth for the same condition.
  - Fix: extend only `Load`'s body and the constructor's parameter list; the condition check itself stays exactly as `solution-dto-property-validators` wrote it.
- Never leave the `NotSupportedException` stub in place once this extension is applied.
  - Risk: a merged file that still throws gives the false impression persistence isn't composed yet, even though it is.
  - Fix: replace the stub `Load` body entirely with the concrete implementation shown above.
