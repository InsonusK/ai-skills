---
description: What a generated Nx project needs so the test kinds find and run its tests
element_kind: project
change_kind: extend
updated: 20261009
tags:
  - solution/conformance-testing-in-angular-nx
  - element/testing-project
  - stack/typescript
  - framework/angular
  - concern/testing
---

# Goal
- A project made by the official generator whose tests the kinds find and run.

# Mutations
Create the project with its generator — `nx g @nx/angular:application`, `nx g @nx/angular:library` (unit test runner `vitest-analog`), `nx g @nx/js:library` — and keep its `project.json` as generated. The examples: [application](../example/apps/portal/project.json), [its e2e project](../example/apps/portal-e2e/project.json), [Angular library](../example/libs/linkcheck/project.json), [logic-only library](../example/libs/formatter/project.json).

In a project with components, set `test.include` of its `vite.config.mts` to `['src/**/spec/*.component.spec.ts']` — see [the library's](../example/libs/linkcheck/vite.config.mts). Component specs follow the base skill's rules.

In the application's `-e2e` project, replace the generated `playwright.config.mts` with [the template](../templates/playwright.config.mts), putting the e2e project's name for `{E2eProject}` and the application's for `{HostProject}`. Browser specs go into its `src/` as `*.ui.spec.ts` (also `*.visual.spec.ts`, `*.style-snapshot.spec.ts`, `*.a11y.spec.ts`), screenshot baselines into `src/__screenshots__/`. Run by `make`, the config writes below the kind directory and serves the application on the free port it is given; run as `nx e2e`, it falls back to `tmp/` and port 4200.

Beside framework-independent code — in a library or in the application — add `features/` and `test/` by the TypeScript parent's rules. Add `src/**/spec/**`, `src/**/test/**` and `src/**/features/**` to the `exclude` of the project's `tsconfig.app.json` or `tsconfig.lib.json`.

A project needs nothing else: no testing metadata, no extra target, no hand-written dependency. Nx reads the dependencies from the imports of the path aliases in `tsconfig.base.json`.

# Rule
## MUST
### Keep the generated project configuration
Leave `project.json` and the plugin-inferred targets as the generator made them.
- Risk: a renamed or hand-rolled `test` or `e2e` target is not found by the kind, and the project drops out of its report without an error.
- Fix: keep the target names `test` and `e2e`; check the project list a kind writes to `result/projects.json` after adding a project.

### Keep tests out of the production build
Exclude `spec/`, `test/` and `features/` in every production `tsconfig`.
- Risk: the application build compiles Cucumber step files and fails on their loose typing, or ships them.
- Fix: the three `exclude` patterns above, in each project.
