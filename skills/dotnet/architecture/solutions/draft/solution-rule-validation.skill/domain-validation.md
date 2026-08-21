# Domain validation

State of several Entities, with no database access **inside the rule itself**. Formula, in short: **Domain validation = data preload + Semantic check** — the same `IsValid()`/`IRuleBuilder`-extension/`Check()` mechanism as in [semantic-validation.md](./semantic-validation.md), the only difference is that the wrapper (a SoftVO or a tuple) is assembled not from the container's own fields, but from data that must first be loaded from another Entity or via a repository. The rule itself doesn't know where the values came from — like in Semantic validation, it just receives an already-assembled pair of numbers and compares them.

One running example — a **hypothetical** `Account`/`Transaction` (there is no such module in the repository; it exists here only as a teaching illustration of the "cannot withdraw more than the balance" invariant, a classic example of an aggregate boundary).

## Account / Transaction — a Domain rule within a single aggregate

**Scope of this section: `Account` and `Transaction` are one aggregate** — `Account` is the root, `Transaction` is its child entity, both written in one business transaction. All the code below, up to the section [«When Account and Transaction are not one aggregate»](#when-account-and-transaction-are-not-one-aggregate-tryconfirm), rests on exactly this assumption: `Account.Withdraw` synchronously reads and writes `Balance`, and this is safe only because the `Account` aggregate's `Version`/lock serializes concurrent withdrawals. As soon as `Account` and `Transaction` are different aggregates (even within the same service) or different services, this approach **no longer works** without extra write serialization, and the right move is not "just SELECT the balance," it's Try/Confirm — see the section below.

Invariant: `Transaction.Amount` cannot be greater than `Account.Balance`. The rule **does not go to the database itself**: it receives already-loaded values — the preload (step 1 of the formula above) is entirely the Handler's/DtoValidator's job, and the comparison itself (step 2) is set up exactly as in [semantic-validation.md](./semantic-validation.md), way 2 (anonymous tuple): `(Balance, Amount)` is not a domain concept on its own, just the input for this comparison.

The class is called `AccountWithdrawalRule` (singular), not `AccountRules` — every Semantic/Domain rule lives in its own class, not in a shared class per Entity, even if other `Account` rules appear nearby (a daily operation-count limit, etc. — each gets its own class).

### `Domain.Rules.csproj Common/ModuleInfo.cs`

```csharp
namespace TaskUnderControl.Srv.AccountModule.Domain.Rules.Common;

internal static class ModuleInfo
{
    public const string ModuleName = "AccountModule";
}
```

### `Domain.Rules.csproj AccountWithdrawalRule.cs`

```csharp
using TaskUnderControl.Srv.AccountModule.Domain.Rules.Common;

namespace TaskUnderControl.Srv.AccountModule.Domain.Rules;

public static class AccountWithdrawalRule
{
    public const string InsufficientBalanceCode = ModuleInfo.ModuleName + ".AccountWithdrawal.InsufficientBalance";
    // A template, not a ready-made string — {0}/{1} are substituted in WithMessage((_, x) => ...) below,
    // the same technique as ComplexityRules.NonNegativeMessageTemplate.
    public const string InsufficientBalanceMessageTemplate = "Withdrawal amount exceeds account balance. Balance {0} < Amount {1}.";

    // Raw values, not an already-computed bool "is there enough money" — the comparison is done
    // by the Rule itself, not by the calling code. If an already-computed bool were passed in,
    // this line would stop being testable logic — the mutation test on Domain.Rules
    // would be pointless, there'd be nothing to mutate.
    public static bool CanWithdraw(this (decimal Balance, decimal Amount) tx) => tx.Amount <= tx.Balance;

    // The same shape as ComplexityIsValid/ScheduleIsValid: works both when the tuple is
    // the object being validated (RuleFor(x => x) inside an InlineValidator below), and as a projection
    // from something else (RuleFor(x => (transaction.Account.Balance, delta)) inside someone else's validator).
    public static IRuleBuilderOptions<T, (decimal Balance, decimal Amount)> CanWithdraw<T>(
        this IRuleBuilder<T, (decimal Balance, decimal Amount)> rule)
        => rule.Must(x => x.CanWithdraw())
               .WithErrorCode(InsufficientBalanceCode)
               .WithMessage((_, x) => string.Format(InsufficientBalanceMessageTemplate, x.Balance, x.Amount))
               .WithState((_, x) => new { x.Balance, x.Amount });

    // The static ctor calls the same CanWithdraw() extension, doesn't duplicate Must/WithErrorCode/
    // WithMessage/WithState a second time — the same technique as ComplexityRules.
    private static readonly InlineValidator<(decimal Balance, decimal Amount)> _validator = new();
    static AccountWithdrawalRule() => _validator.RuleFor(x => x).CanWithdraw();

    public static ValidationResult Check(this (decimal Balance, decimal Amount) tx) => _validator.Validate(tx);
}
```

### `Domain.Rules.csproj Entities/Account.cs`

The check happens **before** assignment, inside the method — no separate "EntityValidator after the fact":

```csharp
public class Account
{
    public decimal Balance { get; internal set; }

    public void Withdraw(decimal amount)
    {
        var result = (Balance, amount).Check();
        var blocking = result.Errors.FirstOrDefault(e => e.Severity == Severity.Error);
        if (blocking is not null)
            throw new DomainException(blocking.ErrorCode, blocking.ErrorMessage);

        Balance -= amount;
    }
}
```

### `Domain.csproj Entities/Transaction.cs` — where an already-loaded related entity is needed

`Transaction` does not hold its own instance of the rule — it relies on the `Account` it's attached to. `Account` in the example below is a navigation property that the Handler **must** load, not a method parameter:

```csharp
public class Transaction
{
    public Account? Account { get; internal set; }   // navigation, may not be loaded
    public decimal Amount { get; internal set; }

    public void UpdateAmount(decimal newAmount)
    {
        // Not DomainException: Transaction itself is valid, the request itself could be
        // valid — it's the Handler that didn't load what was needed for this operation.
        if (Account is null)
            throw new EntityNotLoadedException(nameof(Transaction), nameof(Account));

        // "undo" the old amount, check the new one against the same balance
        Account.Withdraw(newAmount - Amount);
        Amount = newAmount;
    }
}
```

### `Interfaces.csproj DTOs/UpdateTransactionAmountRequestDto.cs`

```csharp
public sealed record UpdateTransactionAmountRequestDto
{
    public decimal NewAmount { get; init; }
}
```

### `Application.csproj Validators/Async/TransactionWithdrawalCheck.cs` — preload and forward the result

This is the case that **cannot** live in `Domain.Rules` — a repository is needed to find out the current balance before entering the Handler. That loading step is exactly the difference from Semantic validation — this class doesn't need to compare `Balance`/`Amount` itself, `AccountWithdrawalRule.Check()` already knows how to do that and already returns a ready `ValidationResult` with the code/message/state. The whole `CustomAsync` delegate lives here entirely, not in the validator's body — it's already a DI class with a repository, so it's the right place for this method too, not only for the private loading step:

```csharp
public sealed class TransactionWithdrawalCheck(IReadRepository<Transaction> transactionRepository)
{
    private async Task<(decimal Balance, decimal Amount)?> Load(UpdateTransactionAmountCommand cmd, CancellationToken ct)
    {
        var transaction = await transactionRepository.FirstOrDefaultAsync(
            new TransactionByIdWithAccountSpec(cmd.TransactionId), ct);

        if (transaction?.Account is null)
            return null;   // existence is a different check, not this one

        return (transaction.Account.Balance, cmd.Payload.NewAmount - transaction.Amount);
    }

    // Signature for FluentValidation CustomAsync: (value, ValidationContext, CancellationToken) -> Task.
    // No CanWithdraw() is called here — the comparison lives entirely in AccountWithdrawalRule.Check(),
    // this method only calls it and forwards the result.
    public async Task CheckAsync(
        UpdateTransactionAmountCommand cmd, ValidationContext<UpdateTransactionAmountCommand> context, CancellationToken ct)
    {
        var loaded = await Load(cmd, ct);
        if (loaded is null)
            return;

        var result = loaded.Value.Check();   // AccountWithdrawalRule.Check() — the same InlineValidator as in Account.Withdraw
        foreach (var failure in result.Errors)
            context.AddFailure(failure);
    }
}
```

### `Application.csproj Features/UpdateTransactionAmount/UpdateTransactionAmount.Validator.cs`

`MustAsync` doesn't fit here — it forces the validator to decide for itself what to write into `WithErrorCode`/`WithMessage`, even though that's already decided in `AccountWithdrawalRule`. `CustomAsync` gives access to `ValidationContext`, and the whole method that writes into it is already defined in `TransactionWithdrawalCheck` — the validator just needs to reference it, the same as in the synchronous examples:

```csharp
public sealed class UpdateTransactionAmountValidator : AbstractValidator<UpdateTransactionAmountCommand>
{
    public UpdateTransactionAmountValidator(TransactionWithdrawalCheck check)
        => RuleFor(x => x).CustomAsync(check.CheckAsync);
}
```

This validator runs in the MediatR pipeline **before** `UpdateTransactionAmountHandler` — the client gets `AccountModule.AccountWithdrawal.InsufficientBalance` (the code from `AccountWithdrawalRule`, not some separate one) in the request's general error pool, without ever reaching the Handler.

### `Application.csproj Features/UpdateTransactionAmount/UpdateTransactionAmount.Handler.cs`

So why is `Account.Withdraw` still needed in the Entity, if the validator already filtered out the invalid case earlier? **Defense in depth**: the validator is a fast, informative rejection for a normal HTTP request, while `Account.Withdraw` is a guarantee that holds regardless of whether the call went through this particular Command (a second Handler, a background job, a future call from somewhere else — all of them will still run into the same `Withdraw`, even if their own validator forgot about it).

```csharp
public class UpdateTransactionAmountHandler : IRequestHandler<UpdateTransactionAmountCommand, Result>
{
    public async Task<Result> Handle(UpdateTransactionAmountCommand request, CancellationToken ct)
    {
        // .Include(t => t.Account) — this is exactly the preload from the formula above:
        // "the Handler correctly loads the entity before working with it."
        var transaction = await _repository.FirstOrDefaultAsync(
            new TransactionByIdWithAccountSpec(request.TransactionId), ct);

        transaction.UpdateAmount(request.Payload.NewAmount);   // Account is guaranteed loaded
        await _repository.UpdateAsync(transaction, ct);
        return Result.Success();
    }
}
```

If the `Specification` forgot `.Include(t => t.Account)`, `UpdateAmount` would throw `EntityNotLoadedException`, and the Handler test would fail immediately — instead of silently treating `Account == null` as "no constraints" (a common hidden bug if the null check is skipped).

### `.feature` and two adapters

The same condition (`AccountWithdrawalRule.CanWithdraw`) is checked both in the Entity and in the Command validator — one `.feature`, two adapters:

```gherkin
Feature: Transaction amount cannot exceed account balance

  Scenario Outline: Withdrawal boundary values
    Given account balance <balance>
    And withdrawal amount <amount>
    When the withdrawal is checked
    Then the verdict is <verdict>
    And the rejection code is <code>

    Examples:
      | balance | amount | verdict  | code                                                  |
      | 100     | 101    | rejected | AccountModule.AccountWithdrawal.InsufficientBalance   |
      | 100     | 100    | accepted | -                                                      |
```

```csharp
// Domain.Tests/StepDefinitions/Entity/AccountSteps.cs — the "Entity" adapter
[When(@"^the withdrawal is checked$")]
public void WhenTheWithdrawalIsChecked()
    => context.Verdict = TestSupport.ValidationResultAdapters.FromDomainAction(
        () => new Account { Balance = _balance }.Withdraw(_amount));
```
```csharp
// Application.Tests/StepDefinitions/Dto/UpdateTransactionAmountSteps.cs — the "CommandValidator" adapter
[When(@"^the withdrawal is checked$")]
public async Task WhenTheWithdrawalIsChecked()
{
    // _fakeCheck — a TransactionWithdrawalCheck over a fake repository that returns
    // a Transaction with Account.Balance = _balance, Amount = 0, so that delta equals _amount.
    var cmd = new UpdateTransactionAmountCommand(_transactionId, new UpdateTransactionAmountRequestDto { NewAmount = _amount }, DateTimeOffset.UtcNow);
    context.Verdict = await new UpdateTransactionAmountValidator(_fakeCheck).ValidateAsync(cmd);
}
```

Two adapters, not five — because this rule has no separate `SoftAccount`/`PropertyValidator` (it's about a pair of `decimal`s, not a standalone VO): it binds exactly to the layers that actually check it.

---

## When Account and Transaction are not one aggregate: Try/Confirm

As soon as `Account` and `Transaction` are different aggregates (even within the same service) or different services, the transition is the same in both cases: don't try to serialize the write some special way within one service, and don't try to synchronously reach into another service — go straight to a Try/Confirm implementation. Below is the same invariant, the same `AccountWithdrawalRule`, but `Transaction` is now its own aggregate with a status, not a child entity of `Account`.

**Idea:** `Transaction` is created immediately, but in an intermediate `Pending` status — based on a **preliminary** check against a local, possibly stale replica of the balance (a fast response to the client, with no guarantee). The real, authoritative check happens where `Account` is the single source of truth: **the same `Account.Withdraw`** as in the section above, just called asynchronously through a message instead of directly. Confirm does not reinvent the invariant — the whole Try/Confirm exists only around the fact that the real check may not be able to happen immediately.

### `Domain.csproj Entities/Transaction.cs` — its own aggregate with a status

`Account` is no longer a navigation property — `Transaction` is not in the same aggregate as `Account`, so it only holds `AccountId` (a soft reference by id, the same principle already used elsewhere in this solution for `TaskLink`↔`TodoTask` — see [Using a Soft Link instead of a Foreign Key](<../../../ADR & Tips/Использование Soft Link вместо Foreign Key между Task и Tag.md>)):

```csharp
public enum TransactionStatus { Pending, Confirmed, Rejected }

public class Transaction
{
    public int AccountId { get; internal set; }        // soft reference, not a navigation — a different aggregate
    public decimal Amount { get; internal set; }
    public TransactionStatus Status { get; internal set; }
    public string? RejectionCode { get; internal set; }   // filled only when Status == Rejected

    public static Transaction CreatePending(int accountId, decimal amount, DateTimeOffset userActionTimeStamp)
        => new() { AccountId = accountId, Amount = amount, Status = TransactionStatus.Pending, /* ... */ };

    // State transitions are also an Entity invariant — you cannot confirm/reject
    // something that isn't Pending anymore (a redelivered message shouldn't mutate it again).
    public void Confirm()
    {
        if (Status != TransactionStatus.Pending)
            throw new DomainException("AccountModule.Transaction.NotPending", "Only a pending transaction can be confirmed.");
        Status = TransactionStatus.Confirmed;
    }

    public void Reject(string rejectionCode)
    {
        if (Status != TransactionStatus.Pending)
            throw new DomainException("AccountModule.Transaction.NotPending", "Only a pending transaction can be rejected.");
        Status = TransactionStatus.Rejected;
        RejectionCode = rejectionCode;
    }
}
```

### Try — a preliminary check against a local replica

```csharp
// A local projection of the balance — populated by a Kafka consumer from the Account
// service's (master's) own events. It may lag behind the real balance — this is deliberately
// acceptable here, because it's only used for a fast preliminary rejection, not the final decision.
public class AccountBalanceSnapshot
{
    public int AccountId { get; internal set; }
    public decimal Balance { get; internal set; }
    public DateTimeOffset SyncedAt { get; internal set; }
}
```

```csharp
public class CreateTransactionHandler(
    IRepository<Transaction> repository,
    IReadRepository<AccountBalanceSnapshot> snapshotRepository,
    IOutbox outbox)
    : IRequestHandler<CreateTransactionCommand, Result<TransactionResponseDto>>
{
    public async Task<Result<TransactionResponseDto>> Handle(CreateTransactionCommand request, CancellationToken ct)
    {
        var snapshot = await snapshotRepository.FirstOrDefaultAsync(
            new AccountBalanceSnapshotByAccountIdSpec(request.AccountId), ct);

        // Preliminary rejection — the same AccountWithdrawalRule.CanWithdraw as in Confirm
        // below, just against possibly stale data. If there's clearly not enough money by the last
        // known replica, don't create a Transaction at all, without waiting for Confirm.
        if (snapshot is not null && !(snapshot.Balance, request.Amount).CanWithdraw())
            return Result<TransactionResponseDto>.Invalid(new ValidationFailure(
                nameof(request.Amount), AccountWithdrawalRule.InsufficientBalanceCode));

        var transaction = Transaction.CreatePending(request.AccountId, request.Amount, request.ActionTimeStamp);
        await repository.AddAsync(transaction, ct);

        // The owner of the invariant — Account — will receive the message and confirm/reject asynchronously.
        await outbox.Publish(new RequestWithdrawalConfirmation(transaction.Id, request.AccountId, request.Amount), ct);

        return Result<TransactionResponseDto>.Success(transaction.ToResponseDto());   // the client sees Status = Pending
    }
}
```

### Confirm — the authoritative check on the side that owns the invariant

This is literally the same `Account.Withdraw` as in the single-aggregate section — Confirm contains no new logic, it just calls it asynchronously, where `Account` is its own, single aggregate (so it's again safe to synchronously write `Balance` here):

```csharp
public class RequestWithdrawalConfirmationConsumer(
    IRepository<Account> accountRepository, IOutbox outbox)
    : IConsumer<RequestWithdrawalConfirmation>
{
    public async Task Handle(RequestWithdrawalConfirmation message, CancellationToken ct)
    {
        var account = await accountRepository.FirstOrDefaultAsync(new AccountByIdSpec(message.AccountId), ct);

        try
        {
            account.Withdraw(message.Amount);   // the same method as above — not redefined
            await accountRepository.UpdateAsync(account, ct);
            await outbox.Publish(new WithdrawalConfirmed(message.TransactionId), ct);
        }
        catch (DomainException ex)
        {
            await outbox.Publish(new WithdrawalRejected(message.TransactionId, ex.Code), ct);
        }
    }
}
```

### Finalizing `Transaction` based on the Confirm result

```csharp
public class WithdrawalConfirmedConsumer(IRepository<Transaction> repository) : IConsumer<WithdrawalConfirmed>
{
    public async Task Handle(WithdrawalConfirmed message, CancellationToken ct)
    {
        var transaction = await repository.GetByIdAsync(message.TransactionId, ct);
        transaction.Confirm();
        await repository.UpdateAsync(transaction, ct);
    }
}

public class WithdrawalRejectedConsumer(IRepository<Transaction> repository) : IConsumer<WithdrawalRejected>
{
    public async Task Handle(WithdrawalRejected message, CancellationToken ct)
    {
        var transaction = await repository.GetByIdAsync(message.TransactionId, ct);
        transaction.Reject(message.RejectionCode);   // compensation: notifying the client, etc. — outside the example
        await repository.UpdateAsync(transaction, ct);
    }
}
```

### One service vs. different services — only the transport changes, not the shape

If `Account` and `Transaction` are different aggregates within one service, the `IOutbox`/`IConsumer` from the example above are an outbox table and a local dispatcher (`App.Infrastructure`, already present in this solution) — the message never leaves the process. If they are different services, the same `IOutbox` publishes to Kafka instead, and `AccountBalanceSnapshot` is synchronized by a separate consumer from the master service's topic. `Transaction`'s states (`Pending`/`Confirmed`/`Rejected`), `CreateTransactionHandler` (Try), and `Account.Withdraw` as the single place of authoritative checking (Confirm) — do not change at all. This is exactly why the move to Try/Confirm should always be immediate, instead of looking for some simpler way to synchronize the write between aggregates of one service — the shape of the solution is the same in both cases, only the wire between Try and Confirm differs.

### What this example does not cover

The saga itself (status transitions, message delivery, consumer idempotency) is not checked by the same `.feature` + adapters mechanism as a `Rule` — it's about orchestrating a process over time, not about whether a function was called from the right place. `AccountWithdrawalRule.CanWithdraw` is still covered by the `.feature` from the section above (it's used unchanged in both Try and Confirm) — but the `Pending → Confirmed/Rejected` sequence itself needs a separate integration/process test, not a conformance spec.
