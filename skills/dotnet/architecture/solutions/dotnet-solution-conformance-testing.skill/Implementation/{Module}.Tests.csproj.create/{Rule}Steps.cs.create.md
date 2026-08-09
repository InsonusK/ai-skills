---
description: Step definitions binding a Gherkin feature file to the module's real validator
project_name: "{Module}.Tests"
name: "{Rule}Steps"
element_kind: class
change_kind: create
---

# Goals
- Prove every scenario in `Rules/{Rule}.feature` against `{Module}`'s real implementation of the rule.

# Core Principles
- The step definition class holds no business logic of its own — it only translates Gherkin steps into calls against `{Module}`'s public API and assertions on the result.

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
- Call `{Module}.{Rule}Validator` (or the module's equivalent real entry point) — never re-implement the validation logic inline in a step.
- Assert a specific `ErrorCode`/result value in negative scenarios, not just `Assert.False(_result.IsValid)`.

## MUST NOT
- Stub or mock `{Module}`'s validator inside these step definitions — the whole point of the scenario is to prove the real implementation.

# Anti-patterns
- **Re-implementing the rule inside `When`**
  - Example: `WhenTheRuleValidatesIt` computes validity with a local regex instead of calling `{Module}.{Rule}Validator`.
  - Consequence: the scenario can stay green after `{Module}.{Rule}Validator` is broken.
  - Instead: call the real validator and assert on its actual return value.

# Check list
- [ ] Every `Given/When/Then` in `{Rule}.feature` has a matching, non-duplicated step method.
- [ ] `{Rule}Steps` calls `{Module}`'s real public API, not a local re-implementation.

# Unittest TestCases
- [ ] WHEN a scenario's input is valid THEN `ThenTheResultIsValid` passes against the real validator.
- [ ] WHEN a scenario's input is invalid THEN `ThenTheResultIsInvalidWithError` asserts the exact error code the real validator returns.
