---
name: bdd-coverage-mutation-testing
description: Defines the three-layer testing quality gate — Cucumber/Gherkin scenarios as the readable test plan, code coverage as the floor, mutation testing as proof the assertions actually check behavior — plus the two-tier CI enforcement (delta-scoped at PR-to-master, full at merge-to-master) with published reports and README badges.
whenToUse: When setting up or reviewing a project's testing strategy or CI gates, when the same validation/business rule must be proven on more than one side of a boundary (e.g. frontend pre-flight checks and backend integrity checks), or when deciding what a "PR into master" check and a "merge into master" check must each run.
tags:
  - workflow/test
  - testing
  - bdd
  - cucumber
  - gherkin
  - code-coverage
  - mutation-testing
  - ci-cd
---

# Goal
- Give a project one readable, executable test plan (Cucumber/Gherkin) instead of scattered ad-hoc test cases, so both the person defining acceptance criteria and the code proving them share one source of truth.
- Use code coverage as a floor (something was executed) and mutation testing as the actual proof of assertion strength (the execution was checked), instead of relying on either alone.
- Make sure the trunk branch (`master`) can only move forward with proof, not with a green "build passed" badge that hides an untested corner case.

# Scope
This skill defines the language-agnostic protocol: what the three layers are, what CI must enforce at PR-to-master and at merge-to-master, and what must be visible in the README. It does not define:
- How to write a strong test/assertion — see [no-test-theater](../no-test-theater.skill/no-test-theater.skill.md); this skill assumes that discipline and adds the CI-enforcement and BDD-spec layer on top of it.
- The numeric coverage floor for a stack — see [code-coverage](../code-coverage.skill.md); this skill enforces that floor in CI instead of leaving it as a guideline.
- Stack-specific tool choice and setup (which Cucumber implementation, which coverage tool, which mutation-testing tool, concrete CI job files) — each target stack gets its own solution skill that extends this one:
  - .NET: [[skills/dotnet/architecture/solutions/dotnet-solution-conformance-testing.skill/dotnet-solution-conformance-testing.skill|solution-conformance-testing]]
  - Python: [[skills/python/architecture/solutions/python-solution-conformance-testing.skill/python-solution-conformance-testing.skill|solution-conformance-testing]]
  - TypeScript (framework-agnostic): [[skills/typescript/architecture/solutions/ts-solution-conformance-testing.skill/ts-solution-conformance-testing.skill|solution-conformance-testing]]
- The case where the same Cucumber spec must be proven by several independently-released implementations (different repositories/packages, possibly different languages). That is a separate, additional pattern layered on top of this one — do not improvise a multi-repository spec-sharing setup from this skill alone; a single implementation repository that owns its own `.feature` files is the default and the common case.

# Core Principle
- Three layers, each catching a gap the others cannot:
  - **Cucumber/Gherkin scenarios** are the readable, unambiguous test plan (living documentation): one scenario, one behavior claim, in `Given/When/Then` form.
  - **Code coverage** proves the code path was *executed*. It is necessary but says nothing about whether the result was checked.
  - **Mutation testing** proves the assertions actually *check* the executed behavior: a tool mutates the production code (flips a condition, changes a constant, removes a line) and the test suite must fail. A mutant that survives means a gap an assert never protected against — the same "coverage theater" [no-test-theater](../no-test-theater.skill/no-test-theater.skill.md) already warns about, just made mechanically detectable instead of relying on manual review.
- A step definition calls the real production validation code. It never re-implements the rule it is supposed to prove — that would test the step definition, not the code.
- The primary use case for this skill is a rule enforced on more than one side of a boundary — for example the same validation rule implemented once on the frontend (fail fast, good UX, avoid a doomed request) and once on the backend (the actual integrity guarantee, since the frontend can always be bypassed). The Cucumber scenarios describe the rule once; each side's test suite proves its own implementation against the same scenarios.
- Not every layer needs to be re-proven at every gate: PR-time feedback must be fast, merge-time proof must be complete. See [# CI enforcement](#ci-enforcement).

# CI enforcement
Two distinct gates, not one:

| Gate | Trigger | Runs | Blocks |
| --- | --- | --- | --- |
| PR gate | Pull request targeting `master` | Full unit + Cucumber suite (must be 100% green); mutation testing **scoped to the changed files/lines only** (delta) | Merge, if red or if a changed line has a new surviving mutant without justification |
| Trunk gate | Merge/push to `master` | Full unit + Cucumber suite; full code-coverage report; full-repository mutation testing; publish both reports as CI artifacts; regenerate README badges from this run | Nothing (informational + badge source), but a red trunk gate must be treated as an incident, not backlog |

Running full-repository mutation testing on every PR is deliberately out of scope for the PR gate — it is slow, and a slow-enough gate gets disabled by the team under deadline pressure. Delta-scoped mutation testing at PR time keeps feedback fast while still catching the corner cases in the code that PR actually touches; the trunk gate is where completeness is proven.

See [templates/ci-gate-structure.template.md](./templates/ci-gate-structure.template.md) for the generic job outline; each stack's solution skill provides the concrete CI file.

# Rule

## MUST
- Give every new or changed behavior/validation rule at least one Cucumber/Gherkin scenario, written in `Given/When/Then` form, before or alongside its implementation.
- Implement step definitions that call the production validation code directly — never re-implement the rule's logic inside a step definition.
- Run, on every PR targeting `master`: the full unit + Cucumber suite (must pass) and mutation testing scoped to the PR's changed files/lines.
- Run, on every merge/push to `master`: the full unit + Cucumber suite, a full code-coverage report, and full-repository mutation testing, and publish both reports as CI artifacts.
- Keep code coverage at or above the project's floor (see [code-coverage](../code-coverage.skill.md)) at both gates — coverage is necessary, not sufficient, on its own.
- Treat any surviving mutant in changed business-logic/validation code as a review-blocking finding at the PR gate unless the PR description explicitly justifies it.
- Publish a code-coverage badge and a mutation-score badge in the repository README, both regenerated from the latest trunk-gate run — never from a PR run.
- Record the chosen Cucumber implementation, coverage tool, and mutation-testing tool for each stack as an ADR in that stack's own solution skill — do not leave the tool choice implicit.

## SHOULD
- Reuse a scenario's Gherkin title as the behavior-claim source for the test name, matching [no-test-theater](../no-test-theater.skill/no-test-theater.skill.md)'s naming rule.
- Keep one `.feature` file per business capability/rule, not one giant file covering unrelated rules.
- Fail the PR gate loudly (not just log a warning) when a new surviving mutant appears in changed code, so it cannot be missed in review.

## MUST NOT
- Merge a PR into `master` with a failing test, a red Cucumber scenario, or coverage below the project's floor.
- Treat a passing "build" status alone, without a coverage badge and a mutation-score badge, as proof of test quality.
- Run full-repository mutation testing as part of the PR gate — that belongs to the trunk gate only.
- Let a step definition stub/mock away the exact behavior a scenario exists to prove.
- Improvise a multi-repository or multi-language shared-spec setup from this skill alone — see [# Scope](#scope).

# Anti-patterns
- **Step definition re-implements the rule instead of calling production code**
  - Example: a step definition for "Then the request is rejected" hand-computes whether the input should be valid instead of calling the real validator.
  - Consequence: the scenario can pass even after the real validator is broken, because the step definition has its own, drifted copy of the rule.
  - Instead: call the production validation function/endpoint from the step definition and assert on its actual result.

- **Coverage-only gate, no mutation testing**
  - Example: CI enforces "coverage >= 80%" and nothing else.
  - Consequence: a test can execute every branch of a validation rule while asserting nothing about the outcome (`NotNull`-only, `expect(x).toBeTruthy()`) and still pass the gate — [no-test-theater](../no-test-theater.skill/no-test-theater.skill.md)'s "coverage theater" survives undetected.
  - Instead: add mutation testing so a weak assertion shows up as a surviving mutant.

- **Full-repository mutation testing on every PR**
  - Example: the PR gate runs the same mutation-testing scope as the trunk gate.
  - Consequence: PR feedback takes tens of minutes, the team starts merging with the check disabled or ignored.
  - Instead: scope PR-gate mutation testing to the PR's changed files/lines only; run the full scope on the trunk gate.

- **Badge shows build status, not test quality**
  - Example: README shows only a green "CI passing" badge.
  - Consequence: a reader (or a future contributor) has no visibility into whether tests are strong, just that something ran without crashing.
  - Instead: publish coverage and mutation-score badges sourced from the latest trunk-gate report, alongside the build badge.

- **Reaching for a multi-repository shared spec before it is needed**
  - Example: a project with a single frontend and a single backend in one repository extracts its Cucumber `.feature` files into a separate published package "in case we need it later."
  - Consequence: the team now maintains cross-repository versioning, publishing, and CI wiring for a case that does not exist yet.
  - Instead: keep `.feature` files inside the one repository until there are genuinely two or more independently-released implementations that must conform to the same spec.

# Check list
- [ ] Every new/changed behavior has a Cucumber/Gherkin scenario in `Given/When/Then` form.
- [ ] Every step definition calls production code; none re-implements the rule it proves.
- [ ] The PR gate ran the full test suite plus delta-scoped mutation testing, and is green.
- [ ] The trunk gate (on merge to `master`) ran the full test suite, full coverage, and full-repository mutation testing, with reports published.
- [ ] README shows a coverage badge and a mutation-score badge, both sourced from the latest trunk-gate run.
- [ ] No surviving mutant exists in changed business-logic/validation code without an explicit justification in the PR.
- [ ] The stack-specific tool choice (Cucumber implementation, coverage tool, mutation-testing tool) is recorded as an ADR in that stack's solution skill.
