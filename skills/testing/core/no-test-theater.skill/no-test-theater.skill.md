---
version: 20261010120000
name: no-test-theater
description: Prevents "coverage theater" — tests that execute code but do not verify real behavior. Defines the scenarios-first protocol (tagged .feature entries before step code, a separate review pass over the scenario/coverage/mutation report after), mandatory test properties, and banned weak-test patterns.
whenToUse: When writing new unit/integration tests, reviewing existing tests, or changing an existing test's assertions, mocks, timeouts, or skip/xfail state — for any language.
tags:
  - concern/testing
  - quality
  - stack

---

# Goal
- Prevent tests that execute production code but never verify the resulting behavior ("coverage theater").
- Make coverage gaps visible before code review — through category-tagged scenarios and the living doc of [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#scenario-inventory) — instead of relying on line/branch coverage percentages alone.
- Stop silent weakening of an existing test (removed assert, raised timeout, added `Skip`) to force it green.

# Scope
This skill defines the language-agnostic protocol, mandatory test properties, and banned patterns. A stack whose test framework needs rules of its own adds them in a companion skill named `no-test-theater-in-{stack}`; a stack without one follows this skill alone.

This skill does not define test naming/folder structure for a specific stack or when to write tests before vs. after implementation (see [test-driven-development](skills/testing/core/test-driven-development.skill/test-driven-development.skill.md)).

# Core Principle
- A test's job is to prove a specific behavior claim, not to raise a coverage number.
- The test name is the claim; the assert is the proof. If the assert does not prove what the name claims, the test is theater — even if it passes and even if it executes the target code.
- When the expected behavior for a scenario is unclear, ask the user. Never invent a plausible-looking but unverified assert to fill the gap.

# Workflow: the scenarios-first protocol
1. **Before writing any step definition or test code**, enumerate scenarios for the touched functionality — happy, boundary, negative, error, concurrency, security — and write each into its `.feature` file with its `@category/…` tag, under a `Feature:` that carries its `@type/…` tag; a scenario not implemented yet is tagged `@status/todo` with a `# todo:` reason, per [cucumber-testing](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md).
2. Write the step definitions and production code; remove `@status/todo` from each scenario as it becomes runnable.
3. **After writing tests**, in a separate pass — not the same pass that wrote the tests — run `make test-and-report` and read the report of [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#scenario-inventory):
   - `reports/tests/livingdoc/` in the report (`tmp/testing/report/` by default) — no entry of the changed features without a type or a category, none `not-run`, and every `todo` or `broken` entry has a reason.
   - `reports/coverage/` and `reports/mutation/` — every changed public method and branch is reached by a scenario, and no surviving mutant in changed code is left without an explanation.
4. If any existing test was weakened while making tests pass (removed assert, raised timeout, added `Skip`/`xfail`/`.only`/`@status/todo`/`@status/broken`), call it out as an explicit, justified point in the PR description.

# Rule

## MUST
- Enumerate scenarios (happy/boundary/negative/error/concurrency/security) before writing step code and record each as a category-tagged `.feature` entry, `@status/todo` with a reason until it runs.
- After writing tests, in a separate pass, read the scenario, coverage, and mutation report for the changed code — per step 3 of the workflow.
- Make each test's assert prove exactly what the test name claims — do not add extra "just in case" mocks or setup that hide real dependencies.
- Give each test exactly one logical reason to fail; split a test that can fail for several unrelated reasons into separate tests.
- Name each test as a behavior claim: Given is embedded in the test's context/fixture, When is the action, Then is the expected outcome, and the name reflects it.
- Assert a specific error type/code/status in negative and error-path tests — not just "an exception was thrown" or "it's not successful".
- Assert, for a validation failure, every invalid field with its message — not only that the result is invalid, and not only how many errors there are.
- Keep line coverage of the code at 80% or above, read from `reports/coverage/` — a floor, never a proof: an error branch is covered by its own scenario like any other, not left to the uncovered remainder.
- Review branch coverage, not line coverage alone — a line count does not show that only one side of a condition ran.
- Give every inbound endpoint a happy scenario and at least one applicable error scenario through its real boundary — routing, authentication, serialization — asserting the full response.
- Assert, in a scenario of a use case that orchestrates several components, both the full response and the order of the calls when the order is part of the contract — not only that each call happened.
- Tag a scenario `@status/todo` with a `# todo: needs clarification — <question>` reason and explicitly ask the user when the expected behavior is unclear.
- Tag a scenario that fails over a defect you cannot fix now `@status/broken` with a `# broken:` reason — never delete it, weaken its assertion, or leave it red.
- Never add `@status/validated`, and remove it from every scenario whose text, examples or steps you changed, naming those scenarios in your report — only a person validates.
- Call out any reduction of an existing test's strictness (removed assert, raised timeout, added `Skip`/`xfail`/`.only`/`@status/todo`) as an explicit, justified point in the PR description.
- Never consider "tests written" done, or merge a PR, while the living doc shows, for the changed features, a `todo` or `broken` entry without a reason, an entry without a type or a category, or a `not-run` entry.
- Never lower an existing test's strictness to make it pass without calling it out explicitly.
- Never assert only `NotNull` / "did not throw" when a concrete value or state can be checked instead — e.g. `Assert.NotNull(result)` on a response whose exact `Id`/`Status`/fields are known ahead of time stays green even if the returned value is completely wrong, as long as it is not null.
- Never force a mock to always return a success response inside a test whose stated purpose is to exercise an error path — a test named `Create_Returns503_WhenExternalServiceUnavailable` with a client mock still stubbed to succeed cannot fail for the reason its name claims to test.
- Never use `Assert.True(true)` / `expect(x).toBeTruthy()`-style assertions without checking a concrete result structure — the assertion cannot fail regardless of what the code under test does.
- Never swallow an exception with try/catch inside a test instead of using the framework's dedicated throw-assertion (`Assert.Throws`, `pytest.raises`, `expect().toThrow()`) — a bare `try { act(); } catch { }` passes whether or not the right exception was thrown.
- Never rely on a snapshot test as the only check of complex business logic — a snapshot diff shows *that* something changed, not whether the new output is *correct*, and wrong output gets silently accepted by re-recording the snapshot.

## SHOULD
- Prefer a `Scenario Outline` (or a parametrized test) over near-duplicate copy-pasted scenarios, but give each data variation that exercises a distinct behavior type its own tagged `Examples:` block — parametrization shortens the file, it must not hide a negative case inside a happy one. Five near-duplicate tests differing only in one input/output create an illusion of scenario coverage without adding real diversity, and multiply maintenance cost.
- Run mutation testing on business-logic modules where the project has it configured, and treat a drop in mutation score on changed files as a review trigger.

# Check list
- [ ] Every scenario identified before writing tests is a category-tagged `.feature` entry; the living doc shows no entry without a type or a category and no `not-run` entry for the changed features.
- [ ] Every `todo` entry of type happy/negative/error has a `# todo:` reason.
- [ ] No test in this change asserts only `NotNull`/truthy, forces a mock to succeed in an error-path test, swallows an exception instead of asserting it, or relies solely on a snapshot for complex logic.
- [ ] Negative/error tests assert a specific error type/code, not just "something went wrong".
- [ ] Line coverage is 80% or above.
- [ ] Coverage and mutation reports show every changed public method/branch reached by a scenario.
- [ ] Mutation testing (where configured) showed no new surviving mutants in changed files without an explanation.
- [ ] Any weakened existing test is called out explicitly in the PR description with a reason.
