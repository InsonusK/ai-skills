---
description: Author concrete component DOM and output assertions.
element_kind: module
change_kind: create
updated: 20261008
tags:
  - solution/conformance-testing-in-angular
  - element/component-spec
  - stack/typescript
  - framework/angular
  - concern/testing
---

# Goal
- Prove the real Angular component's observable behavior through [TestBed](../glossary/testbed.md).

# Mutations
Create `{SourceRoot}/{Component}/test/{Component}.component.spec.ts` beside its production component. This uses the Angular extension's package setup; business contracts keep the inherited Cucumber bindings.

Import the real standalone component in TestBed (or its real NgModule for a module-based application), create its fixture, set inputs with `fixture.componentRef.setInput`, await `fixture.whenStable()`, and assert the rendered text, accessible state or emitted output. Trigger the actual DOM event to exercise template bindings; a direct method call alone does not test the template.

Cover the component's meaningful initial, loading, success, empty and error states as applicable. Subscribe to an output before clicking its real button and assert the emitted payload. For HTTP-backed state, provide the real HttpClient with `provideHttpClient()` and a testing backend with `provideHttpClientTesting()` in that order, flush the expected request and verify no pending requests remain after each test. Use a framework-independent domain fixture already checked by Cucumber when helpful rather than restating the business algorithm.

The [runnable Angular example](../example/README.md) uses the [real form component](../example/src/app/app.ts) and [component specs](../example/src/app/test/validation.component.spec.ts); `make test-kind-components` executes them and produces the shared report inputs.

# Rules
## MUST
### Assert a visible contract
Make each test fail when the template binding, displayed value, accessibility state or output payload is broken.
- Risk: fixture construction or `toBeTruthy()` alone passes with a nonfunctional UI.
- Fix: follow [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md) and include a concrete DOM/output assertion.

### Await deterministic state
Wait for Angular stabilization or controlled async completion and tear down test-local resources after each test.
- Risk: arbitrary sleeps hide races and leaked state makes test order affect results.
- Fix: await `whenStable`, flush the testing HTTP backend, verify pending requests and restore any [Vitest](../glossary/vitest.md) fake timers; do not mix Zone-only `fakeAsync` helpers into the delivered Vitest setup.

### Respect the component boundary
Keep TestBed assertions about Angular rendering and collaboration rather than duplicating Cucumber's business scenarios.
- Risk: two independent specifications drift while neither proves the browser flow.
- Fix: reuse domain fixtures and add browser behavior in the separate UI spec.
