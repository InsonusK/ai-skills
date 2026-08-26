---
description: Step definitions binding a Gherkin feature file to a command handler's orchestration
project_name: "{Module}.Application.Tests"
name: "{Rule}Steps"
element_kind: class
change_kind: create
---

# Goals
- Prove every scenario in `Rules/{Rule}.feature` against `{Module}.Application`'s real handler — that it loads the right state, calls the right guarded domain method, and returns the expected `Result`.

# Core Principles
- Unlike `{Module}.Domain.Tests`' validator-shaped scenarios (input → valid/invalid), Application scenarios are command-shaped: a command goes in, a `Result` comes out. The step definition never re-implements the handler's orchestration — it only sends the command and asserts on the real handler's result.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Step definitions for one handler's scenarios | {Rule}Steps | ChangeCustomerEmailSteps | {Rule}Steps.cs | ChangeCustomerEmailSteps.cs |

# Implementation changes
```csharp
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

# Rule changes

## MUST
- Call the real `{Handler}` — never re-implement the orchestration (load, validate, call domain method, stage) inline in the step.
  - Violation: `WhenTheHandlerProcessesIt` manually constructs the expected `Result` instead of calling `_handler.Handle(...)`.
  - Risk: the scenario can stay green after the real handler's orchestration breaks.
  - Fix: always invoke the real handler and assert on its actual return value.
- Assert the specific error code/message in a failure scenario, not just `IsSuccess == false`.
  - Risk: a boolean-only assertion passes for any failure reason, so a scenario claiming a specific error doesn't actually verify it.
  - Fix: assert the exact error the real handler returns.

# Check list
- [ ] Every `Given/When/Then` in `{Rule}.feature` has a matching, non-duplicated step method.
- [ ] `{Rule}Steps` calls the real `{Handler}`, not a hand-computed expected result.

# Unittest TestCases
- [ ] WHEN a scenario's command is valid THEN `ThenTheResultIsSuccessful` passes against the real handler.
- [ ] WHEN a scenario's command is invalid THEN `ThenTheResultFailsWithError` asserts the exact error the real handler returns.
