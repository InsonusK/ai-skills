---
name: no-test-theater
description: Prevents "coverage theater" — tests that execute code but do not verify real behavior. Defines the before/after scenario trace-matrix protocol, mandatory test properties, and banned weak-test patterns.
whenToUse: When writing new unit/integration tests, reviewing existing tests, or changing an existing test's assertions, mocks, timeouts, or skip/xfail state — for any language.
tags:
  - concern/testing
  - quality
  - stack

---

# Goal
- Prevent tests that execute production code but never verify the resulting behavior ("coverage theater").
- Make coverage gaps visible before code review, through an explicit scenario trace matrix, instead of relying on line/branch coverage percentages alone.
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

# Workflow: the trace-matrix protocol
1. **Before writing any test**, enumerate scenarios for the touched functionality — happy, boundary, negative, error, concurrency, security — and add each as its own row in the module's trace matrix, status 🔧 planned. Use the [test-trace-matrix template](./templates/test-trace-matrix.template.md); create the file next to the code (e.g. `docs/test-trace-matrix.md` or `<module>/TESTS.md`) if it does not exist yet.
2. Write the tests.
3. **After writing tests**, in a separate pass — not the same pass that wrote the tests — do both:
   - Update every matrix row's status to ✅ covered / ⚠️ weak / ❌ not covered.
   - Walk the changed production code and fill the matrix's "Function → Test" reverse table for every changed public method or branch.
4. If any existing test was weakened while making tests pass (removed assert, raised timeout, added `Skip`/`xfail`/`.only`), call it out as an explicit, justified point in the PR description.
5. Update the trace matrix in the same commit/PR that adds or changes tests — a PR that changes tests without touching the matrix is incomplete.

# Rule

## MUST
- Enumerate scenarios (happy/boundary/negative/error/concurrency/security) before writing tests and record them as trace-matrix rows with status 🔧.
- Update the trace matrix in the same commit/PR that adds or changes tests.
- After writing tests, in a separate pass, update matrix statuses and fill the "Function → Test" reverse table for every changed public method/branch.
- Make each test's assert prove exactly what the test name claims — do not add extra "just in case" mocks or setup that hide real dependencies.
- Give each test exactly one logical reason to fail; split a test that can fail for several unrelated reasons into separate tests.
- Name each test as a behavior claim: Given is embedded in the test's context/fixture, When is the action, Then is the expected outcome, and the name reflects it.
- Assert a specific error type/code/status in negative and error-path tests — not just "an exception was thrown" or "it's not successful".
- Mark a scenario 🔧 "needs clarification" in the matrix and explicitly ask the user when the expected behavior is unclear.
- Call out any reduction of an existing test's strictness (removed assert, raised timeout, added `Skip`/`xfail`/`.only`) as an explicit, justified point in the PR description.
- Never consider "tests written" done, or merge a PR, while the trace matrix has ❌ for a happy/negative/error scenario without an explicit justification comment on that row.
- Never lower an existing test's strictness to make it pass without calling it out explicitly.
- Never assert only `NotNull` / "did not throw" when a concrete value or state can be checked instead — e.g. `Assert.NotNull(result)` on a response whose exact `Id`/`Status`/fields are known ahead of time stays green even if the returned value is completely wrong, as long as it is not null.
- Never force a mock to always return a success response inside a test whose stated purpose is to exercise an error path — a test named `Create_Returns503_WhenExternalServiceUnavailable` with a client mock still stubbed to succeed cannot fail for the reason its name claims to test.
- Never use `Assert.True(true)` / `expect(x).toBeTruthy()`-style assertions without checking a concrete result structure — the assertion cannot fail regardless of what the code under test does.
- Never swallow an exception with try/catch inside a test instead of using the framework's dedicated throw-assertion (`Assert.Throws`, `pytest.raises`, `expect().toThrow()`) — a bare `try { act(); } catch { }` passes whether or not the right exception was thrown.
- Never rely on a snapshot test as the only check of complex business logic — a snapshot diff shows *that* something changed, not whether the new output is *correct*, and wrong output gets silently accepted by re-recording the snapshot.

## SHOULD
- Prefer parametrized tests over near-duplicate copy-pasted tests, but still record each data variation as its own scenario row when it exercises a distinct behavior — parametrization shortens the file, not the matrix. Five near-duplicate tests differing only in one input/output create an illusion of scenario coverage without adding real diversity, and multiply maintenance cost.
- Run mutation testing on business-logic modules where the project has it configured, and treat a drop in mutation score on changed files as a review trigger.

# Check list
- [ ] Every scenario identified before writing tests appears in the trace matrix with status ✅/⚠️/❌ (not left at 🔧 without reason).
- [ ] Every ❌ row for happy/negative/error has an explicit justification comment.
- [ ] No test in this change asserts only `NotNull`/truthy, forces a mock to succeed in an error-path test, swallows an exception instead of asserting it, or relies solely on a snapshot for complex logic.
- [ ] Negative/error tests assert a specific error type/code, not just "something went wrong".
- [ ] The "Function → Test" reverse table is filled for every changed public method.
- [ ] Mutation testing (where configured) showed no new surviving mutants in changed files without an explanation.
- [ ] Any weakened existing test is called out explicitly in the PR description with a reason.
