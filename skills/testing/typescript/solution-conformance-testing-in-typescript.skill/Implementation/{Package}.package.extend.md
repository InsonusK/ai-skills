---
description: Add the Cucumber, c8 and Stryker dependencies and their two config files to the package
name: "{Package}"
element_kind: package
change_kind: extend
tags:
  - solution/conformance-testing-in-typescript
  - element/package-package
---

# Goals
- Give `{Package}` the dependencies and the two config files `tools/testing/kinds/unit.sh` and `mutation.sh` run on.

# Core Principles
- `.feature` files live under `features/`, one file per business rule; step definitions live under `features/step-definitions/`, one file per feature file.
- Step definitions import from `src/index.ts`, never from an internal module path directly.

# Structure

## Project Structure
```
/{package-name}
  /src
    index.ts
    {rule}-validator.ts
  /features
    {rule}.feature
    /step-definitions
      {rule}.steps.ts
  package.json
  cucumber.mjs
  stryker.conf.json
```

## Files
- Copy verbatim to `cucumber.mjs`: [`assets/cucumber.mjs`](../assets/cucumber.mjs) — the default profile: feature paths, the `tsx/cjs` loader, the step definitions, `not @todo`.
- Copy verbatim to `stryker.conf.json`: [`assets/stryker.conf.json`](../assets/stryker.conf.json) — the `command` test runner calling `cucumber-js`, `src/**/*.ts` mutated; the package then owns its `thresholds`.

## Directory and class skills
| Directory | file   | Description           |
| ------------------- | --------------------- |
| /features | {rule}.feature | Gherkin scenarios for one business rule |
| /features/step-definitions | {rule}.steps.ts | Bindings that call the package's real exported API |

# npm Packages
| Package   | Version constraint | Purpose                |
| --------- | ------------------ | ---------------------- |
| @cucumber/cucumber | ^10 | Run Gherkin scenarios against step definitions |
| @stryker-mutator/core | ^8 | Mutation testing |
| tsx | latest stable | Load TypeScript step definitions in `@cucumber/cucumber` (`--require-module tsx/cjs`) |
| c8 | ^10 | Coverage of the `cucumber-js` run in `tools/testing/kinds/unit.sh` (`npx c8`) |

# What Does NOT Belong Here
- Gherkin `.feature` files shared with a non-TypeScript implementation of the same rule — those belong to the shared conformance-spec source, not to a copy inside this package.

# Rules

## MUST
- Keep `cucumber.mjs` as the one place that says which features and step definitions run, and `stryker.conf.json`'s `commandRunner` calling `npx cucumber-js` with no path of its own.
  - Violation: a `stryker.conf.json` whose command lists feature paths, or a second test runner beside `cucumber-js`.
  - Risk: mutation testing runs a different set of tests than `make test-kind-unit`, so a mutant "survives" against tests the unit kind never ran — or is killed by one it does not count.
  - Fix: `commandRunner.command` = `npx cucumber-js --format progress`; everything else comes from the default profile.
- List `c8` as a dev dependency.
  - Risk: `unit.sh` calls `npx c8`; without the dependency `npx` downloads whatever version is current on every run.
  - Fix: `"c8": "^10"` in `devDependencies`.
- Configure `cucumber-js` to load `.ts` step definitions through `tsx` (`--require-module tsx/cjs`) — never `ts-node`.
  - Risk: without a TypeScript loader configured, `cucumber-js` cannot import `.steps.ts` files and every scenario fails to run.
  - Fix: pass `--require-module tsx/cjs` (in `tools/testing/kinds/unit.sh` or `cucumber.mjs`); `ts-node` does not load under TypeScript 6+ and is no longer maintained.
- Never import a validator inside a step definition from anywhere other than `src/index.ts`.
  - Violation: importing `src/{rule}-validator.ts` directly from a step definition instead of `src/index.ts`.
  - Risk: the step definition depends on internal file layout that is free to change, defeating the purpose of a stable public API.
  - Fix: import only the symbols `index.ts` re-exports.

# Check list
- [ ] `package.json` lists `@cucumber/cucumber`, `tsx`, `c8`, `@stryker-mutator/core` under `devDependencies`.
- [ ] `npx cucumber-js` with no argument runs every scenario but the `@todo` ones; `stryker.conf.json` names no feature path.
