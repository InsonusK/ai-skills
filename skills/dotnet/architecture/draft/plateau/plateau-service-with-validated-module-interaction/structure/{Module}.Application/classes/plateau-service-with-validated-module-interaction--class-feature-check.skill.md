---
name: class-feature-check
description: Class {Feature}Check in the service-with-validated-module-interaction plateau
whenToUse: when a Command validator needs to reject an invalid request before the Handler runs, for a condition whose data must be loaded from a repository
domain: skill
type: template
plateau: service-with-validated-module-interaction
version: 20260824160000
tags:
  - skill/template/class
  - plateau/service-with-validated-module-interaction
created_by:
  - "[[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]"
---

# Goal
- Let a Command validator reject an invalid request before the Handler runs, for a condition whose data needs to be loaded from a repository

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.create.md|{Feature}Check.cs.create]]

# Core Principles
- Loading is this class's job — a `{Dto}Validator`/`{ValueObject}PropertyValidator` never performs I/O
- The condition is written locally in this class, alongside the loading step — the owning Entity's own method (`solution-domain-behaviour`) enforces the same invariant independently, as the authoritative backstop
- Wired into the pipeline via `CustomAsync`, not `MustAsync` spread across the validator body
- This class defines the shape only — `Load` deliberately throws `NotSupportedException` here. This plateau has no repository or other data-loading abstraction; a persistence-introducing plateau (`plateau-statefull-service`, via `solution-repository-integration`) gives `Load` a real implementation on top of this same class

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.create.md|{Feature}Check.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Async cross-aggregate check | {Feature}Check | TransactionWithdrawalCheck | {Feature}Check.cs | TransactionWithdrawalCheck.cs |

# Implementation
```csharp
//Skill: class-feature-check
//Plateau: service-with-validated-module-interaction
//Version: 20260824160000

public sealed class TransactionWithdrawalCheck
{
    private Task<(decimal Balance, decimal Amount)?> Load(UpdateTransactionAmountCommand cmd, CancellationToken ct)
    {
        // No data-loading abstraction exists in this plateau. A persistence-introducing plateau
        // (plateau-statefull-service, via solution-repository-integration) implements this method
        // via its own {Feature}Check.cs.extend.md.
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

Wired into the Command validator: `RuleFor(x => x).CustomAsync(check.CheckAsync)`.

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.create.md|{Feature}Check.cs.create]]

# Rules
MUST:
- Load data only inside this class, live in `/{Module}.Application/Validators/Async`
- Own its condition locally in this class, next to the loading step
- Be wired into its Command validator via `RuleFor(x => x).CustomAsync(...)`
- Never reference a concrete data-loading abstraction in this plateau — `Load` stays a documented shape until a persistence-introducing plateau extends this same class
SHOULD:
- Return early (no failure added) when the data needed to run the check could not be loaded at all

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.create.md|{Feature}Check.cs.create]]

# Check list
- [ ] Loads data, then checks it locally, in the same class
- [ ] Wired via `RuleFor(x => x).CustomAsync(check.CheckAsync)`
- [ ] The same condition's Entity-side enforcement still runs, independent of this check
- [ ] `Load` does not reference a concrete data-loading abstraction at this plateau

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.create.md|{Feature}Check.cs.create]]
