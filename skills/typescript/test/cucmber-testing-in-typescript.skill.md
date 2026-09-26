---
name: cucmber-testing-in-typescript
description: TypeScript/@cucumber/cucumber-specific rules for Cucumber testing — World-based state sharing, step-file layout, custom parameter types, and VSCode glue configuration
whenToUse: when writing or reviewing cucumber-js scenarios or step definitions in a TypeScript/JavaScript project
updated: 20260913
tags:
  - stack/typescript
  - concern/testing/bdd
  - concern/testing
  - cucumber
  - cucumber-js

---

# Goal
- Every `.feature` file executed through `@cucumber/cucumber`'s CLI runner, with no hand-written test-framework (Jest/Vitest/Mocha) test duplicating a scenario, other than the single runner entry point.
- Every step definition logging via `this.attach(...)` or `console.log`, visible in the runner's own output next to its step.
- Cross-step state carried on a custom `World` class, never a module-level variable.

# Scope
This skill adds TypeScript/`@cucumber/cucumber`-specific mechanics on top of [cucmber-testing](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md) — apply both together; this skill only covers what cucumber-js adds.

# Core Principle
- **The CLI run is the one exception** - The `cucumber-js` CLI invocation is the runner entry point; every other case is a `.feature` scenario, per [One scenario, one runner](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#one-scenario-one-runner).
- **World is the unit of isolation** - cucumber-js instantiates a fresh `World` per scenario; state that must survive between a scenario's steps belongs there, exactly because nothing else is reset between scenarios automatically.

# Rule

## MUST

### Share scenario state through a custom World
Define a custom `World` class (via `setWorldConstructor`) holding all cross-step state for a scenario, and access it as `this` inside step definitions — never a module-level `let`/`var`.
- Violation: a step definition file with a module-level `let lastResult: Result` written by one step and read by another.
- Risk: a module-level variable is shared across every scenario in the process, so parallel or repeated runs leak state between scenarios that should be isolated.
- Fix: put the state on the `World` instance; cucumber-js constructs a new one per scenario.

### Log through World.attach or console output the runner captures
Log a step's action/observation via `this.attach(message, 'text/plain')` or `console.log`, so it appears in the runner's own report next to that step.
- Risk: a step that only asserts leaves no trace of what it checked when a later step in the same scenario fails, contradicting [Steps log action and observation](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#steps-log-action-and-observation).
- Fix: call `this.attach(...)` (shows inline in the `html`/`progress` formatters) or `console.log` for the `progress`/`summary` formatter output.

### One step file per domain concept
Name step files by domain concept (`connection.steps.ts`, `query.steps.ts`, `result.steps.ts`, ...), and keep generic comparator steps in their own file, separate from per-operation action steps.
- Risk: mixing comparators and actions in one file makes the comparator harder to find and audit once, per [Generic comparator steps](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#generic-comparator-steps).
- Fix: split step files by concept under the configured `require`/`import` glob (e.g. `features/step-definitions/*.steps.ts`).

### Tag @todo scenarios and exclude them from the run
Tag a not-yet-runnable scenario `@todo`, exclude it via cucumber-js's `--tags 'not @todo'` (or the `tags` field in `cucumber.js`/`.cucumberrc`), and confirm the runner reports it as skipped, not passed.
- Risk: an unfiltered `@todo` scenario either fails the run (undefined step) or passes on an incomplete implementation, contradicting [Tag unrunnable scenarios @todo and verify exclusion](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#tag-unrunnable-scenarios-todo-and-verify-exclusion).
- Fix: set `tags: 'not @todo'` in the runner config and confirm the summary counts the scenario as skipped.

### Register a custom parameter type instead of parsing inside the step
When a step's placeholder needs a shape beyond cucumber-js's built-ins (`{string}`, `{int}`, `{float}`), register it with `defineParameterType` (giving it a `name` and `regexp`) rather than accepting a raw `{string}` and parsing it manually inside the step body.
- Risk: parsing a structured value by hand inside every step that needs it duplicates the parsing logic and produces a domain-specific step instead of a generic comparator, contradicting [Generic comparator steps](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#generic-comparator-steps).
- Fix: `defineParameterType({ name: 'isoDate', regexp: /\d{4}-\d{2}-\d{2}/, transformer: (s) => new Date(s) })`, then use `{isoDate}` in the step's Cucumber Expression.

## SHOULD

### Configure the VSCode Cucumber glue for TypeScript
When applying [Configure the Cucumber editor extension](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#configure-the-cucumber-editor-extension), use:
```json
{
  "cucumber.glue": ["**/step-definitions/**/*.ts", "**/*.steps.ts"],
  "cucumber.features": ["**/*.feature"]
}
```
List any `defineParameterType` custom type under `cucumber.parameterTypes` too, so the extension resolves it the same way it resolves a built-in.

# Check list
- [ ] Cross-step state lives on a custom `World`, never a module-level variable.
- [ ] Every step with a body logs via `this.attach(...)` or `console.log`, visible in the runner's report.
- [ ] Step files are grouped by domain concept; generic comparator steps sit in their own file.
- [ ] `@todo`-tagged scenarios are excluded via the runner's tag filter, confirmed as skipped rather than passing.
- [ ] A structured placeholder is a registered `defineParameterType`, not parsed by hand inside the step.
- [ ] `cucumber.glue`/`cucumber.parameterTypes` in `.vscode/settings.json` matches this skill's TypeScript configuration when proposed to the user.
