---
name: class-rule-steps
description: Class {Rule}Steps in {Module}.Application.Tests, in the service-with-validated-module-interaction plateau
whenToUse: when adding step definitions for a new command-handler Gherkin feature file
domain: skill
type: template
plateau: service-with-validated-module-interaction
version: 20260822140000
tags:
  - skill/template/class
  - plateau/service-with-validated-module-interaction
created_by:
  - "[[../../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
---

# Goal
- Prove every scenario in `Rules/{Rule}.feature` against `{Module}.Application`'s real handler.

__Applied solutions:__
- [[../../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../../solutions/solution-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Core Principles
- Apply ONE plateau template per class
- Command-shaped, not validator-shaped: the step definition never re-implements the handler's orchestration — it only sends the command and asserts on the real handler's result

__Applied solutions:__
- [[../../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../../solutions/solution-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Step definitions for one handler | {Rule}Steps | ChangeCustomerEmailSteps | {Rule}Steps.cs | ChangeCustomerEmailSteps.cs |

# Implementation
```csharp
//Skill: class-rule-steps
//Plateau: service-with-validated-module-interaction
//Version: 20260822140000

[Binding]
public sealed class {Rule}Steps
{
    private {Command} _command = null!;
    private Result _result = null!;
    private readonly {Handler} _handler = new({module dependencies});

    [Given(@"a {Rule} command with ""(.*)""")]
    public void GivenACommand(string input) => _command = new {Command}(input);

    [When(@"the handler processes it")]
    public async Task WhenTheHandlerProcessesIt() =>
        _result = await _handler.Handle(_command, CancellationToken.None);

    [Then(@"the result is successful")]
    public void ThenTheResultIsSuccessful() => Assert.True(_result.IsSuccess);

    [Then(@"the result fails with error ""(.*)""")]
    public void ThenTheResultFailsWithError(string errorCode) =>
        Assert.Contains(errorCode, _result.Errors);
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../../solutions/solution-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Rules
MUST:
- Call the real `{Handler}` — never re-implement the orchestration inline in the step
- Assert the specific error code/message in a failure scenario, not just `IsSuccess == false`

__Applied solutions:__
- [[../../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../../solutions/solution-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Check list
- [ ] Every `Given/When/Then` in `{Rule}.feature` has a matching, non-duplicated step method
- [ ] `{Rule}Steps` calls the real `{Handler}`, not a hand-computed expected result

__Applied solutions:__
- [[../../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../../solutions/solution-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]
