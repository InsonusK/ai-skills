---
description: Step definitions proving a redirected rule's @semantic/@domain scenarios hold through the DtoValidator / {Feature}Check adapter (collect-all, ValidationResult)
project_name: "{Module}.Application.Tests"
name: "{Rule}Steps.cs"
element_kind: class
change_kind: create
tags:
  - solution/domain-rules
  - element/application-rule-steps-cs
---

# Goals
- Prove that once a DTO validator or async `{Feature}Check` was redirected to a centralized rule, it still rejects/accepts exactly what the shared `.feature` scenario says

# Implementation changes

Worked example (`@semantic`, `TodoTaskPostRequestDtoValidator` redirected to `ScheduleRules` — the DTO's own two separate date fields assembled ad hoc into `SoftSchedule`):

```csharp
[Binding]
public sealed class ScheduleDtoSteps
{
    private readonly TodoTaskPostRequestDtoValidator _validator = new(Mock.Of<IValidator<SoftComplexity>>());
    private TodoTaskPostRequestDto _dto = new();
    private FluentValidation.Results.ValidationResult _result = null!;

    [Given(@"a start date of ""(.*)"" and a due date of ""(.*)""")]
    public void GivenDates(DateTimeOffset start, DateTimeOffset due)
        => _dto = _dto with { StartDateTime = start, DueDateTime = due };

    [When(@"ScheduleRules validates it")]
    public void WhenScheduleRulesValidatesIt() => _result = _validator.Validate(_dto);

    [Then(@"the result is valid")]
    public void ThenTheResultIsValid() => Assert.True(_result.IsValid);

    [Then(@"the result is invalid with error code ""(.*)""")]
    public void ThenTheResultIsInvalidWithErrorCode(string errorCode) =>
        Assert.Contains(_result.Errors, e => e.ErrorCode == errorCode);
}
```

Worked example (`@domain`, `TransactionWithdrawalCheck` redirected to `AccountWithdrawalRule` — the loading step is mocked here, since this project proves the *redirection*, not persistence; `IReadRepository<T>`'s interface already exists in `Shared`, even before a real `solution-repository-integration`-backed implementation is composed on top of this plateau):

```csharp
[Binding]
public sealed class AccountWithdrawalCheckSteps
{
    private readonly Mock<IReadRepository<Transaction>> _repository = new();
    private UpdateTransactionAmountCommand _command = null!;
    private ValidationContext<UpdateTransactionAmountCommand> _context = null!;

    [Given(@"an account balance of (\d+)")]
    public void GivenAnAccountBalanceOf(decimal balance)
        => _repository.Setup(r => r.FirstOrDefaultAsync(It.IsAny<TransactionByIdWithAccountSpec>(), default))
            .ReturnsAsync(new Transaction { Account = new Account { Balance = balance } });

    [Given(@"a withdrawal amount of (\d+)")]
    public void GivenAWithdrawalAmountOf(decimal amount)
        => _command = new UpdateTransactionAmountCommand(NewAmount: amount);

    [When(@"AccountWithdrawalRule validates it")]
    public async Task WhenAccountWithdrawalRuleValidatesIt()
    {
        _context = new ValidationContext<UpdateTransactionAmountCommand>(_command);
        await new TransactionWithdrawalCheck(_repository.Object).CheckAsync(_command, _context, default);
    }

    [Then(@"the result is valid")]
    public void ThenTheResultIsValid() => Assert.Empty(_context.Failures);

    [Then(@"the result is invalid with error code ""(.*)""")]
    public void ThenTheResultIsInvalidWithErrorCode(string errorCode) =>
        Assert.Contains(_context.Failures, f => f.ErrorCode == errorCode);
}
```

# Rule changes

## MUST
- Call the real `{ValueObject}PropertyValidator`/`{Dto}Validator`/`{Feature}Check` — never `{Rule}.Check()` from `{Module}.Domain.Rules` directly
- For a `@domain`-tagged scenario, mock only the loading step (the repository/data source) — the comparison itself still runs for real, inside the rule
- Only bind scenarios tagged `@semantic`/`@domain` from the linked spec

## MUST NOT
- Bind an `@format`-tagged scenario — those have no DtoValidator/`{Feature}Check` adapter to prove
- Mock or stub the rule's own `Check()`/`IsValid()` — only the data-loading dependency is a legitimate mock target here

# Check list
- [ ] Every `@semantic`/`@domain`-tagged scenario for a redirected rule has a matching step class here
- [ ] Step definitions call the real validator/`{Feature}Check`, never the rule directly
- [ ] A `@domain` scenario's mock covers only the loading step, not the comparison
