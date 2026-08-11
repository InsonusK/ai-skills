---
description: Step definitions binding a Gherkin feature file to the package's real exported validator
project_name: "{Package}"
name: "{rule}.steps"
element_kind: module
change_kind: create
---

# Goals
- Prove every scenario in `features/{rule}.feature` against the package's real implementation of the rule.

# Core Principles
- The step-definition module holds no business logic of its own — it only translates Gherkin steps into calls against the package's real exported function/class and assertions on the result.

# Naming convention
| use case | file name pattern | file name |
| -------- | ------------------ | --------- |
| Step definitions for one rule | features/step-definitions/{rule}.steps.ts | features/step-definitions/email-format.steps.ts |

# Implementation changes
```typescript
import { Given, When, Then } from "@cucumber/cucumber";
import assert from "node:assert/strict";
import { validateEmailFormat } from "../../src/index";

interface World {
  input: string;
  result: { isValid: boolean; errorCode?: string };
}

Given("the input {string}", function (this: World, input: string) {
  this.input = input;
});

When("the email format rule validates it", function (this: World) {
  this.result = validateEmailFormat(this.input);
});

Then("the result is valid", function (this: World) {
  assert.equal(this.result.isValid, true);
});

Then("the result is invalid with error {string}", function (this: World, errorCode: string) {
  assert.equal(this.result.errorCode, errorCode);
});
```

# Rule changes

## MUST
- Import `validateEmailFormat` (or the package's equivalent real entry point) from `src/index.ts` — never re-implement the validation logic inline in a step.
  - Violation: the `When` step computes validity with a local regex instead of calling `validateEmailFormat`.
  - Risk: the scenario can stay green after `validateEmailFormat` is broken.
  - Fix: call the real exported function and assert on its actual return value.
- Assert a specific `errorCode`/result value in negative scenarios, not just `assert.equal(this.result.isValid, false)`.
  - Risk: a boolean-only assertion passes for any failure reason, so a scenario claiming a specific error code doesn't actually verify it.
  - Fix: assert the exact `errorCode` (or equivalent result value) the real validator returns.
- Never stub or mock the package's validator inside these step definitions — the whole point of the scenario is to prove the real implementation.
  - Risk: the scenario looks green but no longer proves anything about the real validator, since a stand-in replaced the behavior under test.
  - Fix: exercise the package's real validator end-to-end; stub only genuine external dependencies, never the validator itself.

# Check list
- [ ] Every `Given/When/Then` in `{rule}.feature` has a matching, non-duplicated step definition.
- [ ] The step-definition module imports from `src/index.ts`, not from an internal module path.

# Unittest TestCases
- [ ] WHEN a scenario's input is valid THEN the `Then` step passes against the real validator.
- [ ] WHEN a scenario's input is invalid THEN the `Then` step asserts the exact error code the real validator returns.
