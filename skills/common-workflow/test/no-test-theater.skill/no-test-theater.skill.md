---
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
- Make coverage gaps visible before code review — through type-tagged scenarios and the scenario report of [solution-conformance-testing](skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#scenario-report) — instead of relying on line/branch coverage percentages alone.
- Stop silent weakening of an existing test (removed assert, raised timeout, added `Skip`) to force it green.

# Scope
This skill defines the language-agnostic protocol, mandatory test properties, and banned patterns. Language-specific rules live in their own skills and extend for:
- for xUnit/.NET.
- for pytest/Python.
- for TestBed/Jasmine/Jest.

This skill does not define test naming/folder structure for a specific stack or when to write tests before vs. after implementation (see [test-driven-development](skills/common-workflow/test/test-driven-development.skill/test-driven-development.skill.md)).

# Core Principle
- A test's job is to prove a specific behavior claim, not to raise a coverage number.
- The test name is the claim; the assert is the proof. If the assert does not prove what the name claims, the test is theater — even if it passes and even if it executes the target code.
- When the expected behavior for a scenario is unclear, ask the user. Never invent a plausible-looking but unverified assert to fill the gap.

# Workflow: the scenarios-first protocol
1. **Before writing any step definition or test code**, enumerate scenarios for the touched functionality — happy, boundary, negative, error, concurrency, security — and write each into its `.feature` file with its type tag; a scenario not implemented yet is tagged `@todo` with a `# todo:` reason, per [cucmber-testing](skills/common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md).
2. Write the step definitions and production code; remove `@todo` from each scenario as it becomes runnable.
3. **After writing tests**, in a separate pass — not the same pass that wrote the tests — run `make test-and-report` and read the report of [solution-conformance-testing](skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#scenario-report):
   - `public/scenarios/` — no `untyped` or `missing` entry for the changed features, and every `todo` entry of type `happy`/`negative`/`error` has a reason.
   - `public/coverage/` and `public/mutation/` — every changed public method and branch is reached by a scenario, and no surviving mutant in changed code is left without an explanation.
4. If any existing test was weakened while making tests pass (removed assert, raised timeout, added `Skip`/`xfail`/`.only`/`@todo`), call it out as an explicit, justified point in the PR description.

# Rule

## MUST
- Enumerate scenarios (happy/boundary/negative/error/concurrency/security) before writing step code and record each as a type-tagged `.feature` entry, `@todo` with a reason until it runs.
- After writing tests, in a separate pass, read the scenario, coverage, and mutation report for the changed code — per step 3 of the workflow.
- Make each test's assert prove exactly what the test name claims — do not add extra "just in case" mocks or setup that hide real dependencies.
- Give each test exactly one logical reason to fail; split a test that can fail for several unrelated reasons into separate tests.
- Name each test as a behavior claim: Given is embedded in the test's context/fixture, When is the action, Then is the expected outcome, and the name reflects it.
- Assert a specific error type/code/status in negative and error-path tests — not just "an exception was thrown" or "it's not successful".
- Tag a scenario `@todo` with a `# todo: needs clarification — <question>` reason and explicitly ask the user when the expected behavior is unclear.
- Call out any reduction of an existing test's strictness (removed assert, raised timeout, added `Skip`/`xfail`/`.only`/`@todo`) as an explicit, justified point in the PR description.
- Never consider "tests written" done, or merge a PR, while the scenario report shows a `todo` happy/negative/error entry without a reason, or any `untyped`/`missing` entry, for the changed features.
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
- [ ] Every scenario identified before writing tests is a type-tagged `.feature` entry; the scenario report shows no `untyped` or `missing` entry for the changed features.
- [ ] Every `todo` entry of type happy/negative/error has a `# todo:` reason.
- [ ] No test in this change asserts only `NotNull`/truthy, forces a mock to succeed in an error-path test, swallows an exception instead of asserting it, or relies solely on a snapshot for complex logic.
- [ ] Negative/error tests assert a specific error type/code, not just "something went wrong".
- [ ] Coverage and mutation reports show every changed public method/branch reached by a scenario.
- [ ] Mutation testing (where configured) showed no new surviving mutants in changed files without an explanation.
- [ ] Any weakened existing test is called out explicitly in the PR description with a reason.
