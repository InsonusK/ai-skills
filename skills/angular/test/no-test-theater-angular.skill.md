---
name: no-test-theater-angular
description: Angular/TestBed-specific rules for assertion strength — DOM/output assertions over toBeTruthy(), HttpClientTestingModule verification, form-validation assertions, RxJS timing, and the E2E-vs-unit boundary.
whenToUse: When writing or reviewing TestBed/Jasmine/Jest tests in an Angular project.
updated: 20260909
tags:
  - stack/typescript
  - concern/testing/unit
  - concern/testing
  - framework/angular
---

# Goal
- Angular/TestBed component tests whose assertions check rendered DOM, an emitted output, or a recorded call — never only `toBeTruthy()`.
- Every HTTP-calling service test verifying its request set through `HttpClientTestingModule` and `httpMock.verify()`.
- Every form covered for its invalid state and its concrete validator error, not only its valid state.
- Every RxJS timing test driven by `fakeAsync`/`tick()` or `TestScheduler`, never real timers.
- Unit/component coverage gaps closed by unit/component tests, not by a single E2E test.

# Scope
Adds Angular-specific rules to [no-test-theater](skills/common-workflow/test/no-test-theater.skill/no-test-theater.skill.md); the language-agnostic assertion-strength protocol lives there. Covers TestBed/Jasmine/Jest component and service tests. Does not cover E2E test design or non-Angular test suites.

# Core Principle
- **Creation is not behaviour** - "the component was created" is not a behaviour claim; a component test must prove what the component renders, emits, or calls.

# Rule

## MUST

### Apply no-test-theater alongside this skill
Apply every rule of [no-test-theater](skills/common-workflow/test/no-test-theater.skill/no-test-theater.skill.md) together with the Angular rules here.
- Risk: the Angular rules assume the language-agnostic assertion-strength rules are already in force; used alone they leave non-Angular gaps unchecked.
- Fix: run both skills' check lists against the same test suite.

### Name tests by behaviour and condition
Name every test `it('should <behavior> when <condition>', ...)`.
- Risk: a name that omits the behaviour or the condition hides what regressed when the test fails.
- Fix: state the observable behaviour and the triggering condition in the title.

### Assert DOM or output in component tests
Assert a DOM result (`fixture.debugElement.query(...)`) or an emitted event/output in every component test.
- Violation: `expect(component).toBeTruthy()` as a component spec's only assertion.
- Risk: the component can render nothing, emit nothing, or call the wrong thing and the test still passes.
- Fix: keep `toBeTruthy()` only as the first smoke test; assert rendered DOM or an emitted output for the behaviour under test.

### Verify HTTP requests
For services with HTTP calls, use `HttpClientTestingModule` + `httpMock.expectOne(...)` and call `httpMock.verify()` in `afterEach`.
- Risk: an unexpected extra request goes unnoticed without `verify()`.
- Fix: add `httpMock.verify()` to `afterEach` in every HTTP-calling service spec.

### Test the invalid form state
For forms, assert the invalid state (`form.invalid === true`) and the concrete validator error, not only `form.valid`.
- Risk: a validator that never fires still passes a valid-only test.
- Fix: add a case that drives the form invalid and asserts the specific validator error message.

### Drive RxJS timing with fake time
For RxJS streams, use `fakeAsync`/`tick()` or marble testing (`TestScheduler`) instead of real timers or `setTimeout`.
- Violation: `setTimeout(() => { expect(...); done(); }, 1000)` in an RxJS test.
- Risk: the test is slow and flaky, and the usual "fix" of raising the timeout masks real race conditions.
- Fix: replace real timers with `fakeAsync`/`tick()` or `TestScheduler` marble testing.

### Close unit gaps with unit tests
Close a unit or component coverage gap with a unit/component test, never with a single E2E (Playwright/Cypress) test.
- Violation: a checkbox's business logic has no unit test, but one Cypress test clicks it once.
- Risk: E2E tests are slow, less isolated, and do not pinpoint which unit broke; the logic stays unverified at the unit level.
- Fix: add a unit/component test for the behaviour; keep E2E for cross-component flows.

# Check list
- [ ] Every component spec file has at least one assertion beyond `toBeTruthy()`.
- [ ] Every HTTP-calling service test calls `httpMock.verify()` in `afterEach`.
- [ ] Every form has a test for the invalid state and the specific validator error message.
- [ ] No test uses real timers/`setTimeout` where `fakeAsync`/marble testing would apply.
- [ ] No unit/component coverage gap is closed only by an E2E test.
- [ ] [no-test-theater](skills/common-workflow/test/no-test-theater.skill/no-test-theater.skill.md)'s check list also passes for this suite.
