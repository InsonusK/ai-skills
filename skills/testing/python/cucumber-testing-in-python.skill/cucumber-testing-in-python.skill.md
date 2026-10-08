---
name: cucumber-testing-in-python
description: Python/pytest-bdd-specific rules for Cucumber testing — where feature files and their tests live, step modules and shared steps, scenario state in fixtures, @todo exclusion, step logging, and VSCode glue/parameterTypes configuration
whenToUse: when writing or reviewing pytest-bdd scenarios or step definitions in a Python project, or when deciding where a `.feature` file or a test module goes
updated: 20261008
tags:
  - stack/python
  - concern/testing/bdd
  - concern/testing
  - cucumber
  - pytest-bdd

adr:
  - "[[skills/testing/python/cucumber-testing-in-python.skill/adr/test-location|Test location]]"
---

# Goal
- Every `.feature` file in a `features/` folder beside the code it specifies, run by a step module in the `test/` folder next to it.
- Every scenario executed through `pytest-bdd` in the project's one `pytest` run, with no hand-written test duplicating a scenario.
- Every step function printing its action/observation, visible when the scenario fails.
- `.vscode/settings.json` carrying `cucumber.glue` and a `cucumber.parameterTypes` entry for every placeholder used in `parsers.parse()`.

# Scope
This skill adds Python/pytest-bdd-specific mechanics on top of [cucumber-testing](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md) — apply both together; this skill only covers what Python's Cucumber tooling adds. The `pyproject.toml` settings the layout needs — packaging, collection, coverage, mutation — are delivered by [solution-conformance-testing-in-python](skills/testing/python/solution-conformance-testing-in-python.skill/solution-conformance-testing-in-python.skill.md).

# Core Principle
- **The specification sits with its code** - A reader of a module finds what it must do one folder away, and a failing scenario names the package it belongs to.
- **The step module is the runner** - A step module calls `scenarios(...)` for its feature; `pytest` collects it like any test module, so there is no separate runner entry point, per [One scenario, one runner](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#one-scenario-one-runner).
- **The editor only knows Cucumber Expression types** - The VSCode extension resolves a step's placeholders against Cucumber Expression built-ins and `cucumber.parameterTypes`; it does not know `parsers.parse()`'s `{name}`/`{name:type}` syntax, so an unregistered placeholder reports "Undefined parameter type" even though the step is implemented.

# Rule

## MUST

### Features beside the code, tests in its test folder
Put a package's `.feature` files in a `features/` folder inside that package, and the modules that test it in a `test/` folder beside it — a step module named `{rule}_steps_test.py`, a plain test named `{module}_test.py` — with no `__init__.py` in `test/`. Decision recorded in [adr/test-location.md](./adr/test-location.md).
```
src/{package}/                    or {package}/ without a src/ layout
  {module}.py
  features/
    {rule}.feature
  test/
    {rule}_steps_test.py          scenarios("../features/{rule}.feature") and its steps
    conftest.py                   steps and fixtures shared by several features
    {module}_test.py              a plain test - only where a scenario would be unjustifiably complex
```
- Violation: a `features/` or `test/` tree at the repository root; a `test/__init__.py`.
- Risk: at the root a reader of the code does not find its specification, and a failing scenario does not say which package it belongs to; an `__init__.py` turns the tests into a regular subpackage that coverage measures as product code.
- Fix: move each feature next to the package it specifies, its step module into that package's `test/`; a subpackage with its own behavior gets its own `features/` and `test/`.

### One step module per feature, shared steps in conftest.py
Bind exactly one feature in each step module with `scenarios("../features/{rule}.feature")`, keep the steps only that feature uses in it, and put steps and fixtures several features of the package share — generic comparators included — into `test/conftest.py`.
- Violation: a step module with step functions and no `scenarios(...)` call; one step copied into two step modules.
- Risk: without `scenarios(...)` no scenario of the feature is collected — the run is green and the scenario report shows `missing`; a copied step drifts, per [Generic comparator steps](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#generic-comparator-steps).
- Fix: one `scenarios(...)` call per step module; `pytest-bdd` finds the steps of `conftest.py` for every module of that folder.

### Register every pytest-bdd parse() placeholder as a parameterType
For every `{placeholder}` used in a step via `parsers.parse()`, add a matching entry to `.vscode/settings.json`'s `cucumber.parameterTypes`, giving it a `name` and a `regexp` that accepts the values the step actually receives.
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

### Share scenario state via fixtures, never module globals
Pass state between step functions through a function-scoped `pytest` fixture every step takes as a parameter, never a module-level variable.
- Violation: a module-level `_last_result` variable set by one step and read by another.
- Risk: a module global leaks state across scenarios that should run isolated, causing order-dependent flakiness under parallel or repeated runs.
- Fix: a fixture (`world`) returning a fresh object per scenario, injected into every step that reads or writes state.

### Tag @todo scenarios and exclude them from the run
Tag a not-yet-runnable scenario `@todo`, exclude it with `pytest -m "not todo"` — `pytest-bdd` turns every tag into a marker — and register `todo` under `markers` in `pyproject.toml`.
- Risk: an unfiltered `@todo` scenario either fails the run (undefined step) or passes on an incomplete implementation, contradicting [Tag unrunnable scenarios @todo and verify exclusion](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#tag-unrunnable-scenarios-todo-and-verify-exclusion); an unregistered marker warns on every run.
- Fix: run with `-m "not todo"` and check that the summary counts the scenario as deselected, not passed.

### Print step output so it is visible on failure
Have every step with a body `print()` its action and what it observed, matching [Steps log action and observation](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#steps-log-action-and-observation).
- Risk: a step that only asserts leaves no trace of what it checked when a later step in the same scenario fails.
- Fix: print `action -> observed result`; `pytest` shows the captured output of a failing scenario, and `-s` shows it for a passing one.

### Emit classic Cucumber JSON
Have the run write **classic Cucumber JSON** with `--cucumberjson=<path>.json`.
- Risk: without the standard report the living-doc view has no input.
- Fix: pass `--cucumberjson` in the one `pytest` run; it is the input of the living doc only — it reports the outline's line for every `Examples:` row, so per-row results come from elsewhere.

## SHOULD

### Configure the VSCode Cucumber glue for Python
When applying [Configure the Cucumber editor extension](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#configure-the-cucumber-editor-extension), use:
```json
{
  "cucumber.glue": ["**/test/*_steps_test.py", "**/test/conftest.py"],
  "cucumber.features": ["**/features/*.feature"]
}
```
and add [Register every pytest-bdd parse() placeholder as a parameterType](#register-every-pytest-bdd-parse-placeholder-as-a-parametertype)'s `cucumber.parameterTypes` block alongside it.

# Check list
- [ ] Every `.feature` file sits in `{package}/features/`, its step module in `{package}/test/{rule}_steps_test.py`; the repository root has no `features/` or `test/` tree, and no `test/` folder has an `__init__.py`.
- [ ] Every step module calls `scenarios(...)` once; steps shared by several features are in `test/conftest.py`.
- [ ] Every `parsers.parse()` placeholder has a matching `cucumber.parameterTypes` entry in `.vscode/settings.json`.
- [ ] Cross-step state travels through a fixture, never a module-level global.
- [ ] `@todo`-tagged scenarios are excluded with `-m "not todo"`, confirmed as deselected rather than passing; `todo` is a registered marker.
- [ ] Every step with a body prints its action/observation, visible when the scenario fails.
- [ ] `cucumber.glue` and `cucumber.features` in `.vscode/settings.json` match this skill's Python configuration when proposed to the user.
- [ ] The run writes classic Cucumber JSON per [Emit classic Cucumber JSON](#emit-classic-cucumber-json).
