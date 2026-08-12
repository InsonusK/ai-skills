---
name: test-driven-development
description: Apply red-green-refactor test-first development, scoped to the task type — new behavior, large-scale refactor, or local/mechanical change
whenToUse: before writing code for new business logic, before starting a refactor that spans many units/files, or before making a small local code change. Classify the task type first (see Task types) — the required rigor is different for each.
tags:
  - concern/testing
  - tdd
  - workflow
  - refactoring
  - stack

---

# Goal
- Define when full test-first (red-green-refactor) is worth its cost, and when it is not.
- Prevent two failure modes: writing tests after the fact as an afterthought that just mirrors the implementation, and forcing test-first ceremony on trivial changes where it adds no design pressure.

# Scope
This skill defines the order of writing tests vs. code. It does not define the test case format or coverage rules:
- Use [usecases_list.md](../workflow-unittest-testplan.skill/templates/usecases_list.md) (from [workflow-unittest-testplan](../workflow-unittest-testplan.skill/workflow-unittest-testplan.skill.md)) as the source of test cases to drive red-green cycles for a unit.
- Use [code-coverage](../code-coverage.skill.md) to decide what must be covered.
- When the task is new business logic decomposed into units, this skill governs step 4-5 of [solid-decomposition](skills/common-workflow/develop/solid-decomposition.skill/solid-decomposition.skill.md) (attach test cases, then generate code): drive each unit with red-green-refactor instead of writing the implementation first.

# Core Principle
- Classify the task before choosing an approach — new behavior, large-scale refactor, and local/mechanical change need different rigor.
- A test written after the implementation only confirms what the code already does; a test written before the implementation specifies what the code must do. Prefer the latter whenever a real design decision is being made.
- Do not spend test-first ceremony on changes that involve no new decision — the ceremony's value comes from the design pressure of "make this fail first," which trivial changes don't need.
- Never let a refactor and a behavior change happen without a green baseline in between.

# Task types

## New behavior (new unit, new function, new business rule)
Full red-green-refactor, one test case at a time:
1. Take the next uncovered case from the unit's [usecases_list.md](../workflow-unittest-testplan.skill/templates/usecases_list.md) entry.
2. **Red** — write the test for that case before the implementation exists. Run it and confirm it fails for the expected reason (missing behavior, not a typo).
3. **Green** — write the minimum code to make that test pass. Do not implement unrelated cases yet.
4. **Refactor** — clean up with the test suite green, without changing observable behavior.
5. Repeat from step 1 until the unit's test case list is fully covered.

## Large-scale refactor (structural change across many units/files, no intended behavior change)
Characterization-first, not test-first:
1. Before touching structure, confirm the current behavior is captured by tests (existing or newly added characterization tests) and that the full suite is green.
2. Refactor while keeping the suite green throughout — run tests after every structural step, not only at the end.
3. Do not write speculative new tests for behavior that isn't changing; only add or update tests where the refactor intentionally changes an observable contract.
4. If a step breaks a test, stop and determine whether the break is an intended contract change (update the test) or a regression (fix the code) before continuing.

## Local/mechanical change (rename, small fix inside one function, no new decision logic)
Test-first is not required:
1. Run the existing tests to get a green baseline before making the change.
2. Make the change.
3. Run the tests again.
4. If the change fixes a bug and no existing test caught it, add exactly one regression test for that gap. Do not build a full test-first cycle around a one-line fix.

# Rule

## MUST
- Classify the task as new behavior, large-scale refactor, or local/mechanical change before deciding the testing approach.
- For new behavior: write a failing test for a case before writing the implementation that satisfies it.
- For a large-scale refactor: keep the full test suite green throughout; only change test expectations where the refactor intentionally changes an observable contract.
- For a local/mechanical change: run tests before and after the change.
- Add a regression test when a local fix addresses a bug that had no covering test.

## SHOULD
- Drive red-green cycles in the order test cases appear in the unit's `usecases_list.md` entry.
- Keep each red-green cycle to one test case at a time; do not batch several cases into one implementation pass.

## SHOULD NOT
- Treat "no time for TDD" as a reason to skip writing any test for new business logic — write it after implementation instead of skipping entirely, but prefer before.

## MUST NOT
- Force full red-green-refactor ceremony on a trivial local/mechanical change.
- Refactor structure and change behavior in the same step without a green baseline separating them.
- Write implementation for multiple uncovered test cases before any of their tests exist.

# Anti-patterns
- **Tests written after the implementation as a coverage exercise**
  - Consequence: tests mirror what the code does rather than what it must do; they miss cases the implementation never considered and pass trivially.
  - Instead: for new behavior, write the failing test from the unit's test case list before the implementation.

- **Full TDD ceremony on a one-line fix**
  - Example: writing a new test class, fixtures, and a red-green cycle to fix a typo in a log message.
  - Consequence: wastes time without reducing risk, since there was no design decision to pressure-test.
  - Instead: treat it as a local/mechanical change — run tests before/after, add a regression test only if a real gap was found.

- **Refactoring without a green baseline**
  - Consequence: when a test fails mid-refactor, nobody can tell whether it is a pre-existing issue or a regression introduced by the refactor.
  - Instead: confirm the full suite is green before starting structural changes, and keep it green after every step.

# Check list
- [ ] The task was classified as new behavior, large-scale refactor, or local/mechanical change before choosing the approach.
- [ ] New behavior: a failing test existed before the passing implementation, for every test case.
- [ ] Large-scale refactor: the suite was green before structural changes began and stayed green after every step.
- [ ] Local/mechanical change: tests were run before and after; a regression test was added only if a gap was found.
