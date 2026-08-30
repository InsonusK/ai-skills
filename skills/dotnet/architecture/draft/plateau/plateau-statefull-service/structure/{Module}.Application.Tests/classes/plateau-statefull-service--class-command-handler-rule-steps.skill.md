---
name: plateau-statefull-service--class-command-handler-rule-steps
description: Class {Rule}Steps in {Module}.Application.Tests, in the statefull-service plateau
whenToUse: when adding step definitions for a new command-handler Gherkin feature file
domain: skill
type: template
plateau: statefull-service
version: 20260824100000
tags:
  - skill/template/class
  - plateau/statefull-service
created_by:
  - "[[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
---

# Goal
- Prove every scenario in `Rules/{Rule}.feature` against `{Module}.Application`'s real handler.

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Core Principles
- Apply ONE plateau template per class
- Command-shaped, not validator-shaped: the step definition never re-implements the handler's orchestration — it only sends the command and asserts on the real handler's result

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Step definitions for one handler | {Rule}Steps | ChangeCustomerEmailSteps | {Rule}Steps.cs | ChangeCustomerEmailSteps.cs |

# Implementation
```csharp
//Skill: class-command-handler-rule-steps
//Plateau: statefull-service
//Version: 20260824100000

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
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Rules
MUST:
- Call the real `{Handler}` — never re-implement the orchestration inline in the step
- Assert the specific error code/message in a failure scenario, not just `IsSuccess == false`

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Check list
- [ ] Every `Given/When/Then` in `{Rule}.feature` has a matching, non-duplicated step method
- [ ] `{Rule}Steps` calls the real `{Handler}`, not a hand-computed expected result

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]
