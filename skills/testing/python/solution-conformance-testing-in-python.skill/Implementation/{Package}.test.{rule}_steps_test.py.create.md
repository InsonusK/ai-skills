---
description: Step definitions binding a Gherkin feature file to the package's real function/class
project_name: "{Package}"
name: "{rule}_steps_test"
element_kind: functions
change_kind: create
tags:
  - solution/conformance-testing-in-python
  - element/package-test-rule-steps-test-py
---

# Goals
- Prove every scenario in `{package}/features/{rule}.feature` against `{Package}`'s real implementation of the rule.

# Core Principles
- Step functions hold no business logic of their own — they only translate Gherkin steps into calls against the package's real public API and assertions on the result.
- The step module is the `pytest` module that runs the feature: `scenarios("../features/{rule}.feature")` turns every scenario into a test collected by the one `pytest` run.

# Naming convention
| use case | function name pattern | file name pattern | file name |
| -------- | --------------------- | ------------------ | --------- |
| Step definitions for one rule | step_{verb}_{...} | src/{package}/test/{rule}_steps_test.py | src/validators/test/email_format_steps_test.py |

# Implementation changes
```code example
import pytest
from pytest_bdd import given, parsers, scenarios, then, when

from {package}.{rule}_validator import validate

scenarios("../features/{rule}.feature")


@pytest.fixture
def world():
    return {}


@given(parsers.parse('the input "{value}"'))
def step_given_input(world, value):
    world["input"] = value
    print(f"given: input={value!r}")


@when("the email format rule validates it")
def step_when_validated(world):
    world["result"] = validate(world["input"])
    print(f"when: validate({world['input']!r}) -> {world['result']}")


@then("the result is valid")
def step_then_valid(world):
    print(f"then: is_valid={world['result'].is_valid} want=True")
    assert world["result"].is_valid is True


@then(parsers.parse('the result is invalid with error "{error_code}"'))
def step_then_invalid_with_error(world, error_code):
    print(f"then: error_code={world['result'].error_code!r} want={error_code!r}")
    assert world["result"].error_code == error_code
```

# Rule changes

## MUST
- Call `scenarios("../features/{rule}.feature")` once in the module, and write no other `test_*` function in it.
  - Violation: a step module with step functions only, or a hand-written `test_*` beside the scenarios.
  - Risk: without `scenarios(...)` no scenario of the feature is collected — the run is green and the scenario report shows `missing`; a hand-written test duplicates a scenario outside the feature file.
  - Fix: one `scenarios(...)` call per step module, bound to that module's feature.
- Pass state between steps through a fixture (`world`), never a module-level variable.
  - Risk: a module global leaks from one scenario into the next and makes the result depend on the order.
  - Fix: a function-scoped fixture every step takes as a parameter.
- Import and call `{package}`'s real validation function/class — never re-implement the rule's logic inline in a step.
  - Violation: `step_when_validated` computes validity with a local regex instead of calling `{package}.{rule}_validator.validate`.
  - Risk: the scenario can stay green after the real validator is broken.
  - Fix: call the real function and assert on its actual return value.
- Assert a specific `error_code`/result value in negative scenarios, not just `assert world["result"].is_valid is False`.
  - Risk: a boolean-only assertion passes for any failure reason, so a scenario claiming a specific error code doesn't actually verify it.
  - Fix: assert the exact `error_code` (or equivalent result value) the real validator returns.
- Never stub or monkeypatch `{package}`'s validator inside these step definitions — the whole point of the scenario is to prove the real implementation.
  - Risk: the scenario looks green but no longer proves anything about the real validator, since a stand-in replaced the behavior under test.
  - Fix: exercise `{package}`'s real validator end-to-end; stub only genuine external dependencies, never the validator itself.

# Check list
- [ ] The module calls `scenarios("../features/{rule}.feature")` and holds no hand-written `test_*` function.
- [ ] Every `Given/When/Then` in `{rule}.feature` has a matching, non-duplicated step function.
- [ ] Step functions call `{package}`'s real public API, not a local re-implementation; state travels through a fixture.

# Unittest TestCases
- [ ] WHEN a scenario's input is valid THEN `step_then_valid` passes against the real validator.
- [ ] WHEN a scenario's input is invalid THEN `step_then_invalid_with_error` asserts the exact error code the real validator returns.
