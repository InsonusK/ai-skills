---
description: Step definitions binding a Gherkin feature file to the real production code being proven — canonical pattern, reused by every test project this solution creates
project_name: "{Module}.Domain.Tests"
name: "{Rule}Steps"
element_kind: class
change_kind: create
tags:
  - solution/dotnet-conformance-testing
  - element/module-domain-tests-rulesteps

---

# Goals
- Prove every scenario in `Rules/{Rule}.feature` against `{Module}.Domain`'s real implementation of the rule.

# Core Principles
- The step definition class holds no business logic of its own — it only translates Gherkin steps into calls against the tested project's public API and assertions on the result.
- This exact pattern (a `{Rule}Steps` class per `.feature` file, calling the project's real API) repeats unchanged in `{Module}.Application.Tests`, `{Module}.Interfaces.Tests`, `Shared.Tests`, and `BuildingBlocks.Tests` — only which real API it calls differs.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Step definitions for one rule | {Rule}Steps | EmailFormatSteps | {Rule}Steps.cs | EmailFormatSteps.cs |

# Implementation changes
```csharp
[Binding]
public sealed class {Rule}Steps
{
    private string _input = string.Empty;
    private ValidationResult _result = null!;

    [Given(@"the input ""(.*)""")]
    public void GivenTheInput(string input) => _input = input;

    [When(@"the {Rule} rule validates it")]
    public void WhenTheRuleValidatesIt() => _result = {Module}.{Rule}Validator.Validate(_input);

    [Then(@"the result is valid")]
    public void ThenTheResultIsValid() => Assert.True(_result.IsValid);

    [Then(@"the result is invalid with error ""(.*)""")]
    public void ThenTheResultIsInvalidWithError(string errorCode) =>
        Assert.Equal(errorCode, _result.ErrorCode);
}
```

# Rule changes

## MUST
- Call the tested project's real entry point (e.g. `{Module}.Domain.{Rule}Validator`, or the equivalent real method) — never re-implement the validation logic inline in a step.
  - Violation: `WhenTheRuleValidatesIt` computes validity with a local regex instead of calling the real code.
  - Risk: the scenario can stay green after the real implementation is broken.
  - Fix: call the real code and assert on its actual return value.
- Assert a specific `ErrorCode`/result value in negative scenarios, not just `Assert.False(_result.IsValid)`.
  - Risk: a boolean-only assertion passes for any failure reason, so a scenario claiming a specific error code doesn't actually verify it.
  - Fix: assert the exact `ErrorCode` (or equivalent result value) the real code returns.
- Never stub or mock the code under test inside these step definitions — the whole point of the scenario is to prove the real implementation.
  - Risk: the scenario looks green but no longer proves anything about the real code, since a stand-in replaced the behavior under test.
  - Fix: exercise the real code end-to-end; stub only genuine external dependencies, never the code under test itself.

# Check list
- [ ] Every `Given/When/Then` in `{Rule}.feature` has a matching, non-duplicated step method.
- [ ] `{Rule}Steps` calls the tested project's real public API, not a local re-implementation.

# Unittest TestCases
- [ ] WHEN a scenario's input is valid THEN `ThenTheResultIsValid` passes against the real code.
- [ ] WHEN a scenario's input is invalid THEN `ThenTheResultIsInvalidWithError` asserts the exact error code the real code returns.
