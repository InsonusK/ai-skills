---
name: bdd-coverage-mutation-testing
description: Defines the three-layer testing quality gate — Cucumber/Gherkin scenarios as the readable test plan, code coverage as the floor, mutation testing as proof the assertions actually check behavior — and the uniform `make` command / report contract (`tmp/result/*.json` + `tmp/report/<kind>/`) that lets one CI mechanism enforce it across every stack.
whenToUse: When setting up or reviewing a project's testing strategy, when the same validation/business rule must be proven on more than one side of a boundary (e.g. frontend pre-flight checks and backend integrity checks), or when deciding what a project's test/coverage/mutation tooling must expose for CI to consume.
tags:
  - workflow/test
  - testing
  - bdd
  - cucumber
  - gherkin
  - code-coverage
  - mutation-testing
  - make
---

# Goal
- Give a project one readable, executable test plan (Cucumber/Gherkin) instead of scattered ad-hoc test cases, so both the person defining acceptance criteria and the code proving them share one source of truth.
- Use code coverage as a floor (something was executed) and mutation testing as the actual proof of assertion strength (the execution was checked), instead of relying on either alone.
- Expose all three as a handful of uniform `make` commands producing a uniform, normalized report format, so the same CI/badge/publishing mechanism works unchanged regardless of stack.

# Terminology
- **BDD (Behavior-Driven Development)**: a practice where behavior/acceptance criteria are written in a shared, readable `Given/When/Then` form before or alongside implementation, so the people who define a rule and the code that proves it stay in sync. This skill's chosen BDD notation and tooling family is **Cucumber**, which executes plain-text scenarios written in the **Gherkin** language against step definitions bound to production code.

# Scope
This skill defines the language-agnostic protocol: what the three layers are, and the `make`-command/report contract a project must expose so any CI can enforce them uniformly. It does not define:
- How to write a strong test/assertion — see [no-test-theater](../no-test-theater.skill/no-test-theater.skill.md); this skill assumes that discipline and adds the BDD-spec layer on top of it.
- The numeric coverage floor for a stack — see [code-coverage](../code-coverage.skill.md); this skill's `coverage-test.json` reports against that floor instead of leaving it as an unverified guideline.
- **The actual CI wiring** (which workflow triggers on what, the PR-gate/trunk-gate job split, badge generation, GitHub Pages publishing) — that is entirely owned by [[skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md|devops-github-wf-bdd-report-publish]] (which itself builds on the base triggers/jobs from [[skills/devops/devops-github-wf-pr-validation.skill/devops-github-wf-pr-validation.skill.md|devops-github-wf-pr-validation]]). This skill only guarantees the `make` targets and report files those workflows consume; it does not itself define a single CI job.
- Stack-specific tool choice and setup (which Cucumber implementation, which coverage tool, which mutation-testing tool, the concrete `Makefile`) — each target stack gets its own solution skill that extends this one:
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
- Not every layer needs to be re-proven on every run — mutation testing in particular is slow enough that it needs a cheap, scoped mode. That trade-off is exactly why the `make` contract below has `ONLY_DELTA`/`DELTA_BASE` and `WITH_CODE_COVERAGE` toggles: a caller (a developer, or the CI workflow defined in [[skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md|devops-github-wf-bdd-report-publish]]) decides per run how much proof it needs, without the underlying tooling changing.

# Make command contract
A project applying this skill exposes exactly these `make` targets, regardless of stack. This is what lets one CI mechanism, in [[skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md|devops-github-wf-bdd-report-publish]], stay identical across every implementation:

| Target | Purpose | Toggles |
| --- | --- | --- |
| `make cucumber-test` | Run the Cucumber/Gherkin conformance suite | `WITH_CODE_COVERAGE=true` — also collect and report line coverage |
| `make mutation-test` | Run mutation testing against the project | `ONLY_DELTA=true DELTA_BASE=<git-ref>` — mutate only code changed since `DELTA_BASE`, instead of the whole project |
| `make result-page` | Assemble a static report site from the normalized results below (no test/build tooling involved — pure assembly, so it can run as its own CI job or locally once the two targets above have populated `tmp/`) | — |

Each `*-test` target writes two kinds of output:
- **Normalized result** — a small, stack-independent JSON file under `tmp/result/`, read by `make result-page` and by badge generation. Minimum schema:
  - `tmp/result/cucumber-test.json` — `{ "total": <int>, "passed": <int>, "failed": <int> }`
  - `tmp/result/coverage-test.json` — `{ "linePct": <number> }` (only produced when `WITH_CODE_COVERAGE=true`)
  - `tmp/result/mutation-test.json` — `{ "killed": <int>, "survived": <int>, "timedout": <int>, "noCoverage": <int>, "score": <number> }` (`score` is `killed / (killed+survived+timedout+noCoverage) * 100`, rounded to 1 decimal, `"0.0"` when nothing was tested)
- **Native report** — the tool's own HTML report, kept as-is (not normalized) under `tmp/report/<kind>/` (`tests/`, `coverage/`, `mutation/`), for a human to open directly.

A `mutation-test` run must still exit with the underlying mutation tool's own exit code after writing `tmp/result/mutation-test.json` — normalizing the result is a side effect, not a reason to swallow a real failure.

A reference implementation of this contract (Makefile + normalization scripts) exists for .NET and TypeScript — see each stack's own solution skill under [# Scope](#scope) for the concrete `Makefile` and scripts.

# Rule

## MUST
- Give every new or changed behavior/validation rule at least one Cucumber/Gherkin scenario, written in `Given/When/Then` form, before or alongside its implementation.
  - Risk: without a scenario, the acceptance criteria for the rule exist only in a person's head or an external ticket, drifting from what the code actually does as it evolves.
  - Fix: add or extend a `.feature` file with a `Given/When/Then` scenario for the rule before or alongside implementing it.
- Implement step definitions that call the production validation code directly — never re-implement the rule's logic inside a step definition.
  - Violation: a step definition for "Then the request is rejected" hand-computes whether the input should be valid instead of calling the real validator.
  - Risk: the scenario can pass even after the real validator is broken, because the step definition has its own, drifted copy of the rule.
  - Fix: call the production validation function/endpoint from the step definition and assert on its actual result.
- Expose `make cucumber-test`, `make mutation-test`, and `make result-page`, with the `WITH_CODE_COVERAGE` / `ONLY_DELTA` + `DELTA_BASE` toggles described in [# Make command contract](#make-command-contract) — a caller must never need to know the stack's native test/coverage/mutation CLI.
  - Violation: a CI workflow or a developer runs `dotnet stryker ...`/`mutmut run ...`/`stryker run ...` directly instead of `make mutation-test`.
  - Risk: any consumer of the project (CI, another developer, a script) now needs to know the stack's native tooling, defeating the point of a uniform contract; switching mutation-testing tools later becomes a breaking change for every caller.
  - Fix: wrap every stack tool behind the `make cucumber-test`/`make mutation-test`/`make result-page` targets, and never call the underlying tool directly outside the `Makefile`.
- Normalize each `*-test` target's result into `tmp/result/<name>.json` using the schema above, and keep the tool's native report under `tmp/report/<kind>/`.
  - Risk: without a normalized result file, `make result-page` and badge generation have no stack-independent data to read and would have to parse each tool's native report format instead.
  - Fix: write the small JSON schema from [# Make command contract](#make-command-contract) alongside the tool's native report.
- Keep code coverage at or above the project's floor (see [code-coverage](../code-coverage.skill.md)) whenever `coverage-test.json` is produced — coverage is necessary, not sufficient, on its own.
  - Violation: CI enforces "coverage >= 80%" and nothing else.
  - Risk: a test can execute every branch of a validation rule while asserting nothing about the outcome (`NotNull`-only, `expect(x).toBeTruthy()`) and still pass the gate — [no-test-theater](../no-test-theater.skill/no-test-theater.skill.md)'s "coverage theater" survives undetected.
  - Fix: pair the coverage floor with mutation testing, so a weak assertion shows up as a surviving mutant instead of a passing gate.
- Treat any surviving mutant in changed business-logic/validation code as a review-blocking finding unless explicitly justified in the review.
  - Risk: a surviving mutant marks a gap no assertion protects against — a real regression in that exact spot would also go undetected.
  - Fix: strengthen the assertion until the mutant is killed, or record an explicit justification in the review for why it is acceptable.
- Record the chosen Cucumber implementation, coverage tool, and mutation-testing tool for each stack as an ADR in that stack's own solution skill — do not leave the tool choice implicit.
  - Risk: without a recorded ADR, a future contributor cannot tell whether a different tool was considered and rejected, or never considered at all, and may re-litigate or silently diverge from the choice.
  - Fix: add an ADR to the stack's solution skill recording the selected tools and the reasoning.
- Never let a step definition stub/mock away the exact behavior a scenario exists to prove.
  - Risk: the scenario stays green but no longer proves the behavior it claims to — its assertion has been replaced by a stand-in.
  - Fix: exercise the real behavior end-to-end in the step definition; only stub true external dependencies (network, clock, filesystem), never the behavior under test itself.
- Never require a caller of `make cucumber-test`/`make mutation-test` to pass stack-specific flags beyond the toggles defined in this skill's contract.
  - Risk: every caller (CI workflow, developer, another script) now needs stack-specific knowledge to invoke the targets correctly, defeating the point of a uniform contract.
  - Fix: keep the accepted flags limited to `WITH_CODE_COVERAGE`/`ONLY_DELTA`/`DELTA_BASE`; anything stack-specific belongs inside the `Makefile`, not in the caller's invocation.
- Never improvise a multi-repository or multi-language shared-spec setup from this skill alone.
  - Violation: a project with a single frontend and a single backend in one repository extracts its Cucumber `.feature` files into a separate published package "in case we need it later."
  - Risk: the team now maintains cross-repository versioning, publishing, and CI wiring for a case that does not exist yet.
  - Fix: keep `.feature` files inside the one repository until there are genuinely two or more independently-released implementations that must conform to the same spec (see [# Scope](#scope)).

## SHOULD
- Reuse a scenario's Gherkin title as the behavior-claim source for the test name, matching [no-test-theater](../no-test-theater.skill/no-test-theater.skill.md)'s naming rule.
- Keep one `.feature` file per business capability/rule, not one giant file covering unrelated rules.

# Check list
- [ ] Every new/changed behavior has a Cucumber/Gherkin scenario in `Given/When/Then` form.
- [ ] Every step definition calls production code; none re-implements the rule it proves.
- [ ] `make cucumber-test`, `make mutation-test`, `make result-page` exist and support the `WITH_CODE_COVERAGE`/`ONLY_DELTA`/`DELTA_BASE` toggles.
- [ ] `tmp/result/*.json` follows the schema in [# Make command contract](#make-command-contract); `tmp/report/<kind>/` holds the tool's native report.
- [ ] No surviving mutant exists in changed business-logic/validation code without an explicit justification in the review.
- [ ] The stack-specific tool choice (Cucumber implementation, coverage tool, mutation-testing tool) is recorded as an ADR in that stack's solution skill.
