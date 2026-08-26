---
name: plateau-v1--class-application-rule-steps
description: Step definitions proving a redirected rule's @semantic/@domain scenarios hold through the DtoValidator / {Feature}Check adapter, in the v1 plateau
whenToUse: when a DTO validator or async {Feature}Check condition was redirected to a centralized rule and its shared @semantic/@domain scenarios need proving again at this adapter
domain: skill
type: template
plateau: v1
version: 20260824150000
tags:
  - skill/template/class
  - plateau/v1
created_by:
  - "[[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Prove that once a DTO validator or async `{Feature}Check` was redirected to a centralized rule, it still rejects/accepts exactly what the shared `.feature` scenario says

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Application.Tests.csproj.extend/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Implementation
```csharp
//Skill: class-application-rule-steps
//Plateau: v1
//Version: 20260824150000

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

Only the loading step (the repository) is mocked — the comparison itself still runs for real, inside `AccountWithdrawalRule`. See [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Application.Tests.csproj.extend/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]] for the companion `@semantic` `ScheduleDtoSteps` worked example.

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Application.Tests.csproj.extend/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Rules
MUST:
- Call the real `{ValueObject}PropertyValidator`/`{Dto}Validator`/`{Feature}Check` — never `{Rule}.Check()` directly
- For a `@domain`-tagged scenario, mock only the loading step
- Only bind scenarios tagged `@semantic`/`@domain`
MUST NOT:
- Bind an `@format`-tagged scenario — those belong to `{Module}.Domain.Tests`
- Mock or stub the rule's own `Check()`/`IsValid()`

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Application.Tests.csproj.extend/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Check list
- [ ] Every `@semantic`/`@domain`-tagged scenario for a redirected rule has a matching step class here
- [ ] Step definitions call the real validator/`{Feature}Check`, never the rule directly
- [ ] A `@domain` scenario's mock covers only the loading step, not the comparison

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Application.Tests.csproj.extend/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]
