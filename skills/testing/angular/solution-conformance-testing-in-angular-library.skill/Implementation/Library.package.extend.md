---
description: Scope library domain testing and exclude tests from ng-packagr output.
element_kind: package
change_kind: extend
updated: 20261009
tags:
  - solution/conformance-testing-in-angular-library
  - element/library-package
  - stack/typescript
  - framework/angular
  - concern/testing
---

# Goal
- Test library logic independently of Angular and ship production files only.

# Mutations
The Angular and TypeScript parents are applied; no Nx or alternate test framework is applied.

Install a matching `ng-packagr` development dependency (the example locks Angular/ng-packagr 22), and commit the resulting package lockfile.

Configure [Cucumber](../assets/cucumber.mjs) for every library `features/` and adjacent `test/` directory. Configure [Stryker](../assets/stryker.conf.json) and package `c8.include` for the framework-independent library source, excluding steps/specs. Keep the TypeScript mutation script unchanged: its delta scope comes from these patterns.

Create the host and package configuration shown by [the example](../example/README.md). Use ng-packagr's partial compilation and a public entry point that exports the component and production logic. Its library `tsconfig` excludes `spec/`, `test/`, and `features/`; configure no test assets in `ng-package.json`. Keep the library manifest's version independent of the development workspace manifest.

# Rule
## MUST
### Inspect the actual package
Build the library before inspecting `dist/linkcheck` with `npm pack --dry-run --json`.
- Risk: inspecting the workspace root proves nothing about the package consumers install.
- Fix: run [check-library.py](../scripts/check-library.py), which builds and inspects the package without publishing it.
