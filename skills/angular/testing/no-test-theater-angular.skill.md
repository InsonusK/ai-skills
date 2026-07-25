---
name: no-test-theater-angular
description: Angular/TestBed-specific rules for assertion strength — DOM/output assertions over toBeTruthy(), HttpClientTestingModule verification, form-validation assertions, RxJS timing, and the E2E-vs-unit boundary.
whenToUse: When writing or reviewing TestBed/Jasmine/Jest tests in an Angular project.
tags:
  - angular
  - unit-testing
  - workflow/test
---

# Goal
- Add Angular/TestBed-specific rules on top of the language-agnostic test-quality rules.

# Scope
This skill extends [no-test-theater](skills/common-workflow/test/no-test-theater.skill/no-test-theater.skill.md) — apply both together; this skill only adds Angular-specific rules.

# Core Principle
- A component "was created" is not a behavior claim; a component test must prove what the component renders, emits, or calls.

# Rule

## MUST
- Name tests `it('should <behavior> when <condition>', ...)`.
- Assert a DOM result (`fixture.debugElement.query(...)`) or an emitted event/output in component tests. `expect(component).toBeTruthy()` is allowed only as a first smoke test — never as a component's only test.
- For services with HTTP calls, use `HttpClientTestingModule` + `httpMock.expectOne(...)`, and call `httpMock.verify()` in `afterEach` to catch unexpected extra requests.
- For forms, add a test for the invalid state (`form.invalid === true`) and for the concrete validator error message — not only for `form.valid`.
- For RxJS streams, use `fakeAsync`/`tick()` or marble testing (`TestScheduler`) instead of real timers/`setTimeout` in tests.

## MUST NOT
- Close a unit/component coverage gap by adding a single E2E (Playwright/Cypress) test "for the checkbox" — E2E does not replace unit/component tests.

# Anti-patterns
- **`toBeTruthy()` as the only component test**
  - Example: `expect(component).toBeTruthy();` with no other assertion in the spec file.
  - Consequence: the component can render nothing, emit nothing, or call nothing wrong and the test still passes.
  - Instead: assert the rendered DOM or an emitted output for the behavior under test.

- **HTTP test without `httpMock.verify()`**
  - Example: a test asserts the expected request but never calls `httpMock.verify()` in `afterEach`.
  - Consequence: an unexpected second HTTP call goes unnoticed.
  - Instead: always call `httpMock.verify()` in `afterEach`.

- **Real timers instead of `fakeAsync`/marble testing**
  - Example: `setTimeout(() => { expect(...); done(); }, 1000)` in an RxJS test.
  - Consequence: the test is slow and flaky; when it flakes, the common "fix" is raising the timeout, which masks real race conditions instead of exposing them.
  - Instead: use `fakeAsync`/`tick()` or `TestScheduler` marble testing.

- **One E2E test used to claim unit coverage**
  - Example: a checkbox's business logic has no unit test, but a Cypress E2E test clicks it once.
  - Consequence: E2E tests are slow, less isolated, and do not pinpoint which unit broke; the underlying logic stays unverified at the unit level.
  - Instead: add a unit/component test for the checkbox's behavior; keep E2E for cross-component flows.

# Check list
- [ ] Every component spec file has at least one assertion beyond `toBeTruthy()`.
- [ ] Every HTTP-calling service test calls `httpMock.verify()` in `afterEach`.
- [ ] Every form has a test for the invalid state and the specific validator error message.
- [ ] No test uses real timers/`setTimeout` where `fakeAsync`/marble testing would apply.
- [ ] No unit/component coverage gap is closed only by an E2E test.
