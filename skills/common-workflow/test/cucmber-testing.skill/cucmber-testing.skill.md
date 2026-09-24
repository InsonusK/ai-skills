---
name: cucmber-testing
description: Language-independent rules for writing and organizing Cucumber/Gherkin scenarios and their step definitions — generic comparators, expected-data placement, ordering, logging, and BDD editor setup
whenToUse: when writing or reviewing a `.feature` file or its step definitions, when deciding whether an assertion step is reusable across scenarios, or when configuring an editor/devcontainer for Cucumber
updated: 20260924
tags:
  - stack
  - concern/testing/bdd
  - concern/testing
  - cucumber

---

# Goal
- Every `.feature` scenario written as an explicit input/expected-result matrix, never a narrative walkthrough.
- Every assertion step implemented as a generic comparator that reads its expected data from the feature file, reused across scenarios instead of duplicated per domain object.
- A scenario that cannot run yet tagged `@todo` with its reason, and excluded from the executed suite, never reported as a fake pass.
- Every scenario classified by exactly one type tag, so the test report shows which kinds of behavior are covered and which are missing.
- A VSCode workspace with the Cucumber extension declared in the devcontainer and `cucumber.glue`/`cucumber.features` set in `.vscode/settings.json`.

# Scope
This skill covers language-independent Cucumber/Gherkin authoring: scenario structure, step-definition organization, and editor setup. It does not cover the four-target Makefile/report contract (see [solution-conformance-testing](../solution-conformance-testing.skill/solution-conformance-testing.skill.md)) or any runner-specific mechanism (log capture, step-index generation, tag-filter syntax) — those live in a stack-specific companion skill named `cucmber-testing-in-{stack}` (e.g. `cucmber-testing-in-go`, `cucmber-testing-in-dotnet`, `cucmber-testing-in-python`, `cucmber-testing-in-typescript`). Ask the user which one to load for the project's actual stack rather than loading all of them.

# Core Principle
- **Every case is a scenario** - The only test code that is not a Cucumber scenario is the single runner entry point that executes the suite.
- **Test your own code** - An assertion proves something about the code under test, never about a bug or quirk of an underlying library.
- **Comparators are generic, expectations live in Gherkin** - A step definition compares an actual result against data that came from the feature file; it never hardcodes a domain-specific expectation.
- **A red scenario beats a fake-green one** - A scenario that cannot run yet must be visibly excluded, never silently counted as passing.

# Rule

## MUST

### One scenario, one runner
Write every test case as a Cucumber scenario; the only exception is the one runner entry point wired to the stack's native test tool to execute the suite.
- Violation: a plain unit-test-framework test written to cover behavior that a `.feature` scenario could express instead.
- Risk: test cases split across two systems (Cucumber scenarios and ad hoc tests) defeats the one readable report this approach exists to produce.
- Fix: express the case as a scenario; if the runner itself needs a smoke test, that is the one allowed exception.

### Delete unreachable code instead of testing around it
Delete code that cannot be reached through the public API rather than writing a scenario or unit test just to cover it.
- Violation: a scenario or test written only to execute a nil-guard or branch that no real caller can trigger.
- Risk: the test suite grows to cover code that provides no behavior guarantee, and the false coverage hides that the code is actually dead.
- Fix: remove the unreachable code; if it later becomes reachable, add its scenario then.

### Tag unrunnable scenarios @todo and verify exclusion
Tag a scenario that cannot run yet (missing fixture or dependency, or planned but not implemented) `@todo`, put its reason in a `# todo: <reason>` comment on the line directly above its tags, exclude `@todo` from the executed run, and confirm the runner reports it as excluded rather than passed.
- Violation: leaving an unimplemented scenario in the executed run where a runner treats "skipped" as "passed"; or a bare `@todo` with no `# todo:` reason.
- Risk: a fake-green scenario hides that the case is not actually verified, and nobody investigates it again; a `@todo` without a reason cannot be told apart from a forgotten case in the report.
- Fix: tag it `@todo` with a `# todo:` reason, filter `@todo` out of the run, and check the stack-specific skill for how that runner reports exclusion versus a pass.
```gherkin
  # todo: needs a fake clock to trigger the expiry
  @todo @error
  Scenario: Expired token is rejected
```

### One type tag per scenario
Give every scenario exactly one type tag — `@happy`, `@boundary`, `@negative`, `@error`, `@concurrency`, `@security`, or `@regression` — on the scenario itself or inherited from its `Feature`/`Rule`; in a `Scenario Outline` whose rows exercise different types, split the rows into separately tagged `Examples:` blocks.
- Violation: an untagged scenario, a scenario with both `@happy` and `@negative`, or one `Examples:` table mixing valid and invalid inputs under a single tag.
- Risk: the test report cannot show that, for example, a feature has only happy-path scenarios — the gap stays invisible until a bug finds it.
- Fix: tag each scenario (or each `Examples:` block) with the one type it exercises; the report lists anything without exactly one type tag as `untyped`.
```gherkin
  Scenario Outline: Check a URL
    ...
    @happy
    Examples: well-formed
      | input              | outcome |
      | https://a.example  | valid   |

    @negative
    Examples: malformed
      | input      | outcome |
      | not-a-url  | invalid |
```

### Generic comparator steps
Implement an assertion step as a generic comparator — it reads the actual result and compares it against expected data supplied by the scenario (a data table, `Scenario Outline` parameter, or argument), never against a value baked into the step's code.
- Violation: `Then object 7 is a Goal named "Goal1"` — a step whose text and implementation are both specific to one domain object, requiring its own audit.
- Risk: one bespoke step per domain object multiplies the number of steps that must be individually verified for correctness, instead of a handful of comparators audited once.
- Fix: write `Then the result is exactly:` backed by a step that walks the data table's columns/rows against the actual result; keep the concrete expectations in the feature file.

### Expected values live in the feature file
Put every expected value in the feature file's data table, `Scenario Outline` `Examples:` table, or step parameter — never hardcode it inside step-definition code.
- Risk: an expectation hidden in code is invisible to whoever reads only the `.feature` file, and changing it requires touching code instead of the scenario.
- Fix: pass expected values as step arguments or table data; keep step code generic over them.

### Assert structured responses by deserializing
Assert a structured response (JSON or another serialized object) by deserializing it into a typed structure and comparing fields, never by substring or raw-text matching.
- Risk: substring matching passes on coincidental text matches and misses structural differences (extra/missing fields, wrong types).
- Fix: deserialize the response into the stack's native structure, then assert its fields.

### Order-independent collections are sets
Assert a collection whose order is not guaranteed (e.g. related items, children) as an unordered set, or by locating one specific element by name/id — never assume list order.
- Risk: a scenario that assumes an incidental order becomes flaky the moment the underlying implementation reorders results without changing behavior.
- Fix: compare as sets, or assert a single named element instead of the whole ordered list.

### Steps log action and observation
Give every step that has a body a log line stating what it did and what it observed, placed so it appears next to that step in the runner's output rather than detached from it.
- Violation: a step that only asserts, leaving no trace of what was actually checked when a later step in the same scenario fails.
- Risk: a failing scenario shows only the final assertion, not the path that led to it, making the failure hard to diagnose from the report alone.
- Fix: log `action → observed result` (e.g. `opened "file.xml" → root "Model", 15 element(s)`); see the stack-specific skill for how to keep the line attached to its step in that runner's output.

### Organize step files by domain concept
Group step-definition files by the domain concept they operate on (e.g. `connection`, `query`, `result`), not by Given/When/Then phase, since one step definition matches its text regardless of which keyword introduced it.
- Risk: splitting by phase instead of concept scatters a single concept's steps across multiple files with no matcher benefit.
- Fix: one file per domain concept; a step used in only one `.feature` file lives beside that feature, a step shared across features lives in its concept's file.

### Keep Gherkin plain domain language
Never encode step numbering (`G1`, `W1`) or "shared/local" markers into a step's Gherkin text.
- Risk: numbering or scope markers turn the feature file into an implementation artifact instead of the readable domain language it exists to be.
- Fix: write steps as plain domain sentences; reuse comes from parameterization, not from naming conventions in the text.

### Reuse steps through parameterization
Make a step reusable by parameterizing it (e.g. `I run the query {string}` for every query), not by manually deduplicating near-identical steps or maintaining a hand-written step catalog.
- Risk: near-duplicate steps drift apart over time, and a manual catalog goes stale the moment a step is added or renamed without updating it.
- Fix: parameterize the step; if the runner can list/generate its registered steps, use that instead of a hand-maintained index.

### Never overwrite the input fixture
When a scenario's steps produce output (a mutation, a write), load the input fixture read-only and write the result to a separate, scenario-specific location declared explicitly inside that scenario — never in a shared `Background`.
- Violation: two scenarios sharing one output location via `Background`, so the second scenario's run overwrites the first's result.
- Risk: overwriting the fixture destroys the known-good input for every other scenario; sharing an output location across scenarios leaves only the last scenario's state to inspect on failure.
- Fix: declare the fixture and its scenario-specific output inside the scenario itself; give a failing/negative scenario its own output target too, so it never clobbers a passing scenario's result.

### Assert persisted output via reload
When the artifact under test is persisted (saved to a file, a document, a database row), assert the result by reloading it, not only by inspecting the in-memory object that produced it.
- Risk: an in-memory-only assertion can pass while the actual persisted round-trip is broken (serialization bug, lost field, corrupted encoding).
- Fix: reload the persisted artifact and assert against the reloaded value.

### Stub the service behind a thin adapter
When testing a thin adapter/tool layer that sits in front of a service (e.g. an API or CLI-tool layer), stub or spy the service's interface instead of exercising its real implementation; assert which method was called with which arguments, the shape of the response the adapter produced, and that a service error is translated into the adapter's own error form.
- Risk: exercising the real service from an adapter-layer scenario re-tests the service's own logic instead of the adapter's translation responsibility, and couples the adapter's tests to the service's infrastructure (files, databases, network).
- Fix: inject a stub/spy implementation of the service interface, configure its canonical response or error per scenario, and assert the adapter's translation of it.

### Configure the Cucumber editor extension
When working in VSCode, recommend to the user: installing the `CucumberOpen.cucumber-official` extension and adding it to `.devcontainer/devcontainer.json`'s `customizations.vscode.extensions`, and adding to `.vscode/settings.json`:
```json
{
  "cucumber.glue": ["<stack-specific glue patterns>"],
  "cucumber.features": ["**/*.feature"]
}
```
- Risk: without the extension and `cucumber.glue` configured, step definitions in the IDE show as unresolved ("undefined step") even when a matching step exists, making the feature file harder to navigate and author.
- Fix: propose the extension/devcontainer/settings change to the user; get the `cucumber.glue` glob for the project's stack from the matching stack-specific skill listed in [# Scope](#scope).

## SHOULD

### Prefer a generated step index over a manual one
Use the runner's own step-listing/generation facility, when it has one, instead of maintaining a hand-written index of step definitions.

# Check list
- [ ] Every scenario is structured as an input/expected-result matrix (data table or `Examples:`), not a narrative.
- [ ] No unit-test-framework test duplicates what a `.feature` scenario could express, other than the one runner entry point.
- [ ] No test exists solely to cover code unreachable through the public API.
- [ ] Every not-yet-runnable scenario is tagged `@todo` with a `# todo:` reason, filtered out of the executed run, and confirmed excluded rather than reported as passing.
- [ ] Every scenario (or `Examples:` block) carries exactly one type tag: `@happy`, `@boundary`, `@negative`, `@error`, `@concurrency`, `@security`, `@regression`.
- [ ] Every assertion step is a generic comparator reading expected data from the feature file — no domain-specific hardcoded step.
- [ ] No expected value is hardcoded in step-definition code.
- [ ] Structured responses are asserted via deserialization, never substring matching.
- [ ] Order-independent collections are asserted as sets or by named element, never by assumed order.
- [ ] Every step with a body logs its action and observation, attached to that step in the runner's output.
- [ ] Step-definition files are grouped by domain concept, not by Given/When/Then.
- [ ] No step numbering or shared/local marker appears in Gherkin text.
- [ ] Reused steps are parameterized, not duplicated or manually catalogued.
- [ ] No scenario overwrites an input fixture; each output-producing scenario declares its own output target inside the scenario.
- [ ] Persisted output is asserted via reload, not only via the in-memory object.
- [ ] Adapter/tool-layer scenarios stub the service interface instead of exercising its real implementation.
- [ ] The Cucumber VSCode extension, devcontainer entry, and `cucumber.glue`/`cucumber.features` settings have been proposed to the user when working in VSCode.
