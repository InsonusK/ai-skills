---
description: Add Cucumber, Vitest coverage, and Stryker scripts/config to the package
name: "{Package}"
element_kind: package
change_kind: extend
---

# Goals
- Give `{Package}` a `test`/`coverage`/`mutation` script triplet that runs Vitest unit tests together with the package's Cucumber scenarios.

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
  vitest.config.ts
  stryker.config.mjs
```

## Directory and class skills
| Directory | file   | Description           |
| ------------------- | --------------------- |
| /features | {rule}.feature | Gherkin scenarios for one business rule |
| /features/step-definitions | {rule}.steps.ts | Bindings that call the package's real exported API |

# npm Packages
| Package   | Version constraint | Purpose                |
| --------- | ------------------ | ---------------------- |
| @cucumber/cucumber | ^10 | Run Gherkin scenarios against step definitions |
| vitest | ^2 | Unit tests and coverage (`--coverage`, v8 provider) |
| @stryker-mutator/core | ^8 | Mutation testing |
| ts-node | latest stable | Load TypeScript step definitions in `@cucumber/cucumber` |

# What Does NOT Belong Here
- Gherkin `.feature` files shared with a non-TypeScript implementation of the same rule — those belong to the shared conformance-spec source, not to a copy inside this package.

# Rules

## MUST
- Add `test`, `coverage`, and `mutation` scripts to `package.json` that run Vitest and `cucumber-js`/Stryker respectively.
  - Risk: without these scripts, `make cucumber-test`/`make mutation-test` (implemented by `Repository.extend`'s scripts) have no consistent npm entry point to invoke.
  - Fix: define `test`, `coverage`, and `mutation` scripts in `package.json` that wrap Vitest and `cucumber-js`/Stryker respectively.
- Configure `cucumber.mjs` to load `.ts` step definitions (e.g. via `ts-node/esm` or `tsx`).
  - Risk: without a TypeScript loader configured, `cucumber-js` cannot import `.steps.ts` files and every scenario fails to run.
  - Fix: configure `cucumber.mjs` to load step definitions through `ts-node/esm` or `tsx`.
- Never import a validator inside a step definition from anywhere other than `src/index.ts`.
  - Violation: importing `src/{rule}-validator.ts` directly from a step definition instead of `src/index.ts`.
  - Risk: the step definition depends on internal file layout that is free to change, defeating the purpose of a stable public API.
  - Fix: import only the symbols `index.ts` re-exports.

# Check list
- [ ] `package.json` declares `test`, `coverage`, `mutation` scripts.
- [ ] `cucumber.mjs` can load `.ts` step definitions.
