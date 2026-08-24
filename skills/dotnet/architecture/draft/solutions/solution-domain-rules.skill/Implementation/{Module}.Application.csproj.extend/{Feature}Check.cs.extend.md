---
description: Replace solution-dto-property-validators's local comparison with a call to a centralized, Domain-classified Check()
project_name: "{Module}.Application"
name: "{Feature}Check.cs"
element_kind: class
change_kind: extend
tags:
  - solution/domain-rules
  - element/feature-check-cs
---

# Goals
- Stop duplicating the comparison this async check performs once the same condition is also enforced by the owning Entity's own method

# Implementation changes

Before (per `solution-dto-property-validators`, local comparison):

```csharp
public async Task CheckAsync(
    UpdateTransactionAmountCommand cmd, ValidationContext<UpdateTransactionAmountCommand> context, CancellationToken ct)
{
    var loaded = await Load(cmd, ct);
    if (loaded is null)
        return;

    if (loaded.Value.Amount > loaded.Value.Balance)
        context.AddFailure(nameof(cmd.Payload.NewAmount), "Withdrawal amount exceeds account balance.");
}
```

After (redirected to the centralized `AccountWithdrawalRule.Check()` — the exact same comparison `Account.Withdraw` uses):

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

The local `if (...) context.AddFailure(...)` comparison is deleted — no `ErrorCode`/`Message` is re-declared at this call site, both are forwarded from `{Module}.Domain.Rules`.

# Rule changes

## MUST
- Forward an existing `Check()`'s `ValidationResult.Errors` via `context.AddFailure(failure)` instead of comparing locally, once redirected
- Delete the local comparison this file used to define

## MUST NOT
- Keep the local comparison alongside the centralized one
- Re-declare `ErrorCode`/`Message` at this call site when a synchronous counterpart (the Entity method) already has one to forward from
