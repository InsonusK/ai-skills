---
description: Step definitions binding a Gherkin feature file to the package's real exported validator
project_name: "{Package}"
name: "{rule}.steps"
element_kind: module
change_kind: create
tags:
  - solution/conformance-testing-in-typescript
  - element/rule-steps-ts
---

# Goals
- Prove every scenario in `src/{package}/features/{rule}.feature` against the package's real implementation of the rule.

# Core Principles
- The step-definition module holds no business logic of its own — it only translates Gherkin steps into calls against the package's real exported function/class and assertions on the result.

# Naming convention
| use case | file name pattern | file name |
| -------- | ------------------ | --------- |
| Step definitions for one rule | src/{package}/test/{rule}.steps.ts | src/{package}/test/email-format.steps.ts |

# Implementation changes
```typescript
import { Given, When, Then } from "@cucumber/cucumber";
import assert from "node:assert/strict";
import { validateEmailFormat } from "../{rule}-validator";

interface World {
  attach: (message: string, mediaType: string) => void;
  input: string;
  result: { isValid: boolean; errorCode?: string };
}

Given("the input {string}", function (this: World, input: string) {
  this.input = input;
  this.attach(`given: input=${input}`, "text/plain");
});

When("the email format rule validates it", function (this: World) {
  this.result = validateEmailFormat(this.input);
  this.attach(`when: result=${JSON.stringify(this.result)}`, "text/plain");
});

Then("the result is valid", function (this: World) {
  this.attach(`then: valid=${this.result.isValid}`, "text/plain");
  assert.equal(this.result.isValid, true);
});

Then("the result is invalid with error {string}", function (this: World, errorCode: string) {
  this.attach(`then: error=${this.result.errorCode}`, "text/plain");
  assert.equal(this.result.isValid, false);
  assert.equal(this.result.errorCode, errorCode);
});
```

# Rule changes

## MUST
- Import `validateEmailFormat` (or the package's equivalent real entry point) from the production module beside `test/` — never re-implement the validation logic inline in a step.
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
- [ ] The step-definition module imports the real module beside `test/`; a contract scenario imports the package entry point.

# Unittest TestCases
- [ ] WHEN a scenario's input is valid THEN the `Then` step passes against the real validator.
- [ ] WHEN a scenario's input is invalid THEN the `Then` step asserts the exact error code the real validator returns.
