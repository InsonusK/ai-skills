---
description: Author browser interactions and reviewed visual assertions.
element_kind: module
change_kind: create
updated: 20261008
tags:
  - solution/conformance-testing-in-angular
  - element/ui-spec
  - stack/typescript
  - framework/angular
  - concern/testing
---

# Goal
- Browser specs asserting user-visible flow outcomes in the served Angular application.

# Mutations
Create `{SourceRoot}/{Component}/test/{Flow}.ui.spec.ts`, using [Playwright](../glossary/playwright.md)'s `test` and `expect`. This uses the Angular extension's UI config and server; inherited Cucumber remains the business-rule specification.

Visit a real route, locate controls by role/accessible name, perform the user's inputs and click, then await locator assertions on the resulting page. For a backend-free UI contract, install `page.route` before navigation and return controlled success/error responses; label that scope as UI with a controlled backend. For an integrated flow, provision the actual test backend and reset its state per test. Neither approach may replace the application document with synthetic HTML.

Where appearance is part of the requirement, wait for loading to finish and fonts to settle, then assert a stable component/region with `toHaveScreenshot`. Use deterministic data and review any masks; do not mask the behavior being tested. Functional UI tests do not need a screenshot assertion solely to increase coverage.

The [counter browser specs](../examples/test/counter.ui.spec.ts) illustrate a user click and a separate visual contract against the same served component. Author the application’s real flow and reviewed [visual baseline](../glossary/visual-baseline.md).

# Rules
## MUST
### Assert a user-visible outcome
Assert the result of the actual browser interaction rather than only page loading or HTTP status.
- Risk: the route opens while the flow is broken.
- Fix: use accessible locators, auto-waiting expectations and the final visible state.

### Review visual baselines explicitly
Create or update screenshot baselines only as a separate review step and keep normal kind runs in `updateSnapshots: 'none'` mode.
- Risk: a regression becomes its own expected screenshot or a fresh checkout silently approves pixels.
- Fix: inspect expected/actual/diff images, run the local pinned Playwright CLI with `--update-snapshots` and the same config under a disposable `TEST_KIND_DIR` only when the owner authorizes the new expectations, then commit the reviewed baseline files beside the spec.

### Preserve failures and discovery
Keep retries disabled, reject focused tests and treat skipped tests, empty suites and missing browser/server prerequisites as red evidence.
- Risk: a green badge omits the flow or hides flaky behavior.
- Fix: retain the delivered config and adapter; fix or explicitly resolve the failing test instead of weakening its assertion.
