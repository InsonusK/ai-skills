---
name: cucmber-testing-in-python
description: Python/behave-or-pytest-bdd-specific rules for Cucumber testing — step logging via caplog/capsys, step-module layout, and VSCode glue/parameterTypes configuration
whenToUse: when writing or reviewing behave or pytest-bdd scenarios or step definitions in a Python project
updated: 20260913
tags:
  - stack/python
  - concern/testing/bdd
  - concern/testing
  - cucumber
  - behave
  - pytest-bdd

---

# Goal
- Every `.feature` file executed through `behave` (or `pytest-bdd` under `pytest`), with no hand-written `unittest`/`pytest` test duplicating a scenario, other than the single runner entry point.
- Every step function printing its action/observation to stdout (behave) or using `caplog`/`capsys`-visible output (pytest-bdd), never a silently swallowed log call.
- `.vscode/settings.json` carrying both `cucumber.glue` and, when using `pytest-bdd`, `cucumber.parameterTypes` for every non-Cucumber-Expression placeholder used in `parsers.parse()`.

# Scope
This skill adds Python/behave/pytest-bdd-specific mechanics on top of [cucmber-testing](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md) — apply both together; this skill only covers what Python's Cucumber tooling adds.

# Core Principle
- **The runner is the one exception** - `behave`'s CLI run, or the single `pytest` invocation collecting `pytest-bdd` scenarios, is the runner entry point; every other case is a `.feature` scenario, per [One scenario, one runner](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#one-scenario-one-runner).
- **The editor only knows Cucumber Expression types** - The VSCode extension resolves a step's placeholders against Cucumber Expression built-ins and `cucumber.parameterTypes`; it does not know `parsers.parse()`'s `{name}`/`{name:type}` syntax, so an unregistered placeholder reports "Undefined parameter type" even though the step is implemented.

# Rule

## MUST

### Register every pytest-bdd parse() placeholder as a parameterType
For every `{placeholder}` used in a `pytest-bdd` step via `parsers.parse()`, add a matching entry to `.vscode/settings.json`'s `cucumber.parameterTypes`, giving it a `name` and a `regexp` that accepts the values the step actually receives.
```json
{
  "cucumber.parameterTypes": [
    { "name": "title", "regexp": ".*" },
    { "name": "authors", "regexp": ".*" },
    { "name": "answer", "regexp": ".*" }
  ]
}
```
- Violation: a step written with `parsers.parse('the title is "{title}"')` with no corresponding `{ "name": "title", ... }` entry in `cucumber.parameterTypes`.
- Risk: the extension cannot compile the step's expression and reports "Undefined parameter type", which surfaces to the author as "Undefined step" even though the step is implemented and passes when run — the defect is invisible until someone checks the extension's output panel.
- Fix: add one `cucumber.parameterTypes` entry per distinct placeholder name used across the project's `parsers.parse()` steps; keep the list in sync whenever a new placeholder name is introduced.

### One step module per domain concept
Name step modules by domain concept (`steps/connection_steps.py`, `steps/query_steps.py`, `steps/result_steps.py`, ...), and keep generic comparator steps in their own module, separate from per-operation action steps.
- Risk: mixing comparators and actions in one module makes the comparator harder to find and audit once, per [Generic comparator steps](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#generic-comparator-steps).
- Fix: split step modules by concept; `behave` auto-loads every module under `steps/`, `pytest-bdd` collects step modules imported by the test file that calls `scenarios(...)`.

### Share scenario state via context/fixtures, never module globals
Pass state between step functions through `behave`'s `context` object, or a `pytest` fixture shared by the step functions (`pytest-bdd`), never a module-level global variable.
- Violation: a module-level `_last_result` variable set by one step and read by another.
- Risk: a module global leaks state across scenarios that should run isolated, causing order-dependent flakiness under parallel or repeated runs.
- Fix: store state on `context` (behave) or in a fixture injected into both steps (pytest-bdd).

### Tag @todo scenarios and exclude them from the run
Tag a not-yet-runnable scenario `@todo`, exclude it via `behave --tags=-todo` or `pytest-bdd`'s `pytest.mark.skip`/tag-to-marker mapping with `-m "not todo"`, and confirm it is reported as skipped, not passed.
- Risk: an unfiltered `@todo` scenario either fails the run (undefined step) or passes on an incomplete implementation, contradicting [Tag unrunnable scenarios @todo and verify exclusion](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#tag-unrunnable-scenarios-todo-and-verify-exclusion).
- Fix: filter `@todo` out of the default run and check the runner's summary counts the scenario as skipped.

### Print or log step output so it is visible on failure
Have every step with a body print its action/observation (behave: plain `print()`, since behave surfaces stdout per step; pytest-bdd: `print()` combined with running `pytest` without `-s` suppression, or `caplog` for logger-based output), matching [Steps log action and observation](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#steps-log-action-and-observation).
- Risk: a step that only asserts leaves no trace of what it checked when a later step in the same scenario fails.
- Fix: log `action → observed result`; run `pytest -v` (or rely on `pytest`'s default capture-on-failure) so the log surfaces on a failing scenario.

## SHOULD

### Configure the VSCode Cucumber glue for Python
When applying [Configure the Cucumber editor extension](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#configure-the-cucumber-editor-extension), use:
```json
{
  "cucumber.glue": ["**/steps/*.py", "**/test_*.py"],
  "cucumber.features": ["**/*.feature"]
}
```
and add [Register every pytest-bdd parse() placeholder as a parameterType](#register-every-pytest-bdd-parse-placeholder-as-a-parametertype)'s `cucumber.parameterTypes` block alongside it when the project uses `pytest-bdd`.

# Check list
- [ ] Every `pytest-bdd` `parsers.parse()` placeholder has a matching `cucumber.parameterTypes` entry in `.vscode/settings.json`.
- [ ] Step modules are grouped by domain concept; generic comparator steps sit in their own module.
- [ ] Cross-step state travels through `context` (behave) or a shared fixture (pytest-bdd), never a module-level global.
- [ ] `@todo`-tagged scenarios are excluded from the default run via tag/marker filtering, confirmed as skipped rather than passing.
- [ ] Every step with a body prints or logs its action/observation, visible when the scenario fails.
- [ ] `cucumber.glue` (and `cucumber.parameterTypes` for pytest-bdd) in `.vscode/settings.json` matches this skill's Python configuration when proposed to the user.
