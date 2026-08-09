---
description: Step definitions binding a Gherkin feature file to the package's real function/class
project_name: "{Package}"
name: "{rule}_steps"
element_kind: functions
change_kind: create
---

# Goals
- Prove every scenario in `features/{rule}.feature` against `{Package}`'s real implementation of the rule.

# Core Principles
- Step functions hold no business logic of their own — they only translate Gherkin steps into calls against the package's real public API and assertions on the result.

# Naming convention
| use case | function name pattern | file name pattern | file name |
| -------- | --------------------- | ------------------ | --------- |
| Step definitions for one rule | step_{verb}_{...} | features/steps/{rule}_steps.py | features/steps/email_format_steps.py |

# Implementation changes
```code example
from behave import given, when, then
from {package}.{rule}_validator import validate


@given('the input "{value}"')
def step_given_input(context, value):
    context.input = value


@when("the email format rule validates it")
def step_when_validated(context):
    context.result = validate(context.input)


@then("the result is valid")
def step_then_valid(context):
    assert context.result.is_valid is True


@then('the result is invalid with error "{error_code}"')
def step_then_invalid_with_error(context, error_code):
    assert context.result.error_code == error_code
```

# Rule changes

## MUST
- Import and call `{package}`'s real validation function/class — never re-implement the rule's logic inline in a step.
- Assert a specific `error_code`/result value in negative scenarios, not just `assert context.result.is_valid is False`.

## MUST NOT
- Stub or monkeypatch `{package}`'s validator inside these step definitions — the whole point of the scenario is to prove the real implementation.

# Anti-patterns
- **Re-implementing the rule inside a step function**
  - Example: `step_when_validated` computes validity with a local regex instead of calling `{package}.{rule}_validator.validate`.
  - Consequence: the scenario can stay green after the real validator is broken.
  - Instead: call the real function and assert on its actual return value.

# Check list
- [ ] Every `Given/When/Then` in `{rule}.feature` has a matching, non-duplicated step function.
- [ ] Step functions call `{package}`'s real public API, not a local re-implementation.

# Unittest TestCases
- [ ] WHEN a scenario's input is valid THEN `step_then_valid` passes against the real validator.
- [ ] WHEN a scenario's input is invalid THEN `step_then_invalid_with_error` asserts the exact error code the real validator returns.
