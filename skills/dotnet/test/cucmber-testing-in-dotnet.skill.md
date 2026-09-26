---
name: cucmber-testing-in-dotnet
description: .NET/Reqnroll-specific rules for Cucumber testing — hook-based logging via ITestOutputHelper/ScenarioContext, binding-class layout, and VSCode glue configuration
whenToUse: when writing or reviewing Reqnroll (or SpecFlow) scenarios or step bindings in a .NET project
updated: 20260913
tags:
  - stack/dotnet
  - concern/testing/bdd
  - concern/testing
  - cucumber
  - reqnroll

---

# Goal
- Every `.feature` file's scenarios executed through Reqnroll, with the underlying xUnit/NUnit test runner treated as the single entry point per test project, never hand-written tests duplicating a scenario.
- Every `[Binding]` class's step methods logging via the runner's captured output (`ITestOutputHelper` for xUnit), never `Console.WriteLine`.
- One `[Binding]` class per domain concept, generic comparator steps kept separate from per-operation action steps.

# Scope
This skill adds .NET/Reqnroll-specific mechanics on top of [cucmber-testing](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md) — apply both together; this skill only covers what Reqnroll and .NET add.

# Core Principle
- **The test runner is the one exception** - The xUnit/NUnit test method Reqnroll generates per scenario is the runner entry point; every case a human writes is a `.feature` scenario, per [One scenario, one runner](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#one-scenario-one-runner).
- **Captured output keeps logs attached to their scenario** - `ITestOutputHelper` (or NUnit's `TestContext.Out`) ties a log line to the currently running test; `Console.WriteLine` does not reliably surface inside a test-runner report.

# Rule

## MUST

### Log through the test runner's captured output
Inject and use `ITestOutputHelper` (xUnit) or `TestContext.Out` (NUnit) for a step's action/observation log, never `Console.WriteLine` or `Debug.WriteLine`.
- Violation: a step logging via `Console.WriteLine` instead of the injected output helper.
- Risk: `Console.WriteLine` output is not reliably captured per-test by the runner, so the log line either disappears or is not attached to the failing scenario, defeating [Steps log action and observation](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#steps-log-action-and-observation).
- Fix: constructor-inject `ITestOutputHelper` into the `[Binding]` class (Reqnroll/xUnit resolves it per scenario) and log through it.

### One [Binding] class per domain concept
Name binding classes by domain concept (`ConnectionSteps`, `QuerySteps`, `ResultSteps`, ...), and keep generic comparator steps (e.g. "the result is exactly") in their own binding class, separate from per-operation action steps.
- Risk: mixing comparators and actions in one binding class makes the comparator harder to find and audit once, per [Generic comparator steps](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#generic-comparator-steps).
- Fix: split by concept; a `ScenarioContext`/`ObjectContainer`-shared fixture ("world") for cross-step state lives in its own binding-independent class.

### Share scenario state via ScenarioContext or an injected context class
Pass state between step methods through Reqnroll's `ScenarioContext` (or a POCO injected via its context-injection/`[Binding]` constructor), never through static fields.
- Violation: a static field on a binding class holding the last query result for the next step to read.
- Risk: a static field leaks state between scenarios that Reqnroll otherwise runs isolated, causing order-dependent flakiness.
- Fix: use `ScenarioContext.Get<T>()`/`Set<T>()`, or a plain class Reqnroll injects into every binding class sharing that scenario's execution.

### Tag @todo scenarios and exclude them from the run
Tag a not-yet-runnable scenario `@todo` and exclude it via the test runner's category/trait filter (e.g. `dotnet test --filter "Category!=todo"` with Reqnroll's tag-to-trait mapping), confirming it is reported as skipped, not passed.
- Risk: an unfiltered `@todo` scenario either fails the build (if its step is undefined) or, worse, passes on an incomplete implementation, contradicting [Tag unrunnable scenarios @todo and verify exclusion](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#tag-unrunnable-scenarios-todo-and-verify-exclusion).
- Fix: map the `@todo` Gherkin tag to a runner category/trait and filter it out of the default run.

## SHOULD

### Configure the VSCode Cucumber glue for .NET
When applying [Configure the Cucumber editor extension](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#configure-the-cucumber-editor-extension), use:
```json
{
  "cucumber.glue": ["**/*Steps.cs", "**/Steps/**/*.cs"],
  "cucumber.features": ["**/*.feature"]
}
```

# Check list
- [ ] Every step logs through `ITestOutputHelper`/`TestContext.Out`, never `Console.WriteLine`.
- [ ] Binding classes are grouped by domain concept; generic comparator steps sit in their own binding class.
- [ ] Cross-step state travels through `ScenarioContext` or context injection, never a static field.
- [ ] `@todo`-tagged scenarios are mapped to a runner category/trait and excluded from the default run, confirmed as skipped rather than passing.
- [ ] `cucumber.glue` in `.vscode/settings.json` matches this skill's .NET glob when proposed to the user.
