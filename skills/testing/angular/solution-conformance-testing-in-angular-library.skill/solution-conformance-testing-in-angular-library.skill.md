---
name: solution-conformance-testing-in-angular-library
description: Refines Angular conformance testing for one publishable library with a demo application for browser tests.
whenToUse: Add or maintain conformance testing in a non-Nx repository whose product is one Angular library consumed by applications in other repositories.
domain: skill
type: architecture
version: 20261009210100
updated: 20261009
tags:
  - skill/architecture/solution
  - solution/conformance-testing-in-angular-library
  - stack/typescript
  - framework/angular
  - concern/testing
creates:
  - projects/demo
extends:
  - angular.json
  - package.json
  - tsconfig.spec.json
  - cucumber.mjs
  - stryker.conf.json
  - tools/testing/kinds/components.sh
  - playwright.ui.config.ts
depends_on:
  - "[Angular conformance testing](../solution-conformance-testing-in-angular.skill/solution-conformance-testing-in-angular.skill.md)"
adr:
  - "[Demo host](adr/demo-host.md)"
---

# Goal
- Run the inherited four kinds and assemble one report for an Angular library repository.
- Keep tests out of the published package while testing its real component and framework-independent logic.

# Core Principle
- The library owns its scenarios and native specs; `projects/demo` supplies the application boundary needed by browser tests.
- This refinement replaces project selection and configuration; it inherits the result adapter, spec rules, glossary and shared Make contract.

# Boundaries
- The supplied adapter targets Angular CLI 22 with one library and a demo host. Nx workspaces use a separate refinement.
- Installing Chromium system libraries needs root or a prepared image; the example container build is not verified here.

# Adr
- [Demo host](adr/demo-host.md): use the existing `projects/demo` convention and compile TestBed tests against its application build target.

# Template Skill Mutations
- [Repository](Implementation/Repository.extend.md): select the library for components and the demo host for browser tests.
- [Library package](Implementation/Library.package.extend.md): scope scenarios/mutation, compile and inspect the published package.

# Workflow
1. Apply the Angular base, then the two mutations above; retain its component and UI spec rules.
2. Run `make init && make test-and-report`; open `tmp/testing/report/index.html`.
3. Build the library and run the package inspection in the example verification script.

# Ground truth
[The runnable library example](examples/README.md) was verified on 2026-10-09 with Node 24.21.0, Angular 22, Vitest 5, [ng-packagr](glossary/ng-packagr.md) 22 and Playwright 1.64/Chromium:
- `make init && make test-and-report`: exit 0; 25/25 domain scenarios, domain line coverage 98.11%, mutation 90.2%, components 4/4, UI 4/4 including the unchanged reviewed screenshot baseline.
- Dependency-cold run (`npm ci` reinstalls node_modules), then all four kinds and report: 35.3 seconds on a host with warmed npm/browser/system-library caches. This is not a first-download measurement.
- `run-example.sh`: exit 0 in report, caller-selected check directories and delta mutation; complete living-doc tags/reasons and report links checked. In the task worktree delta mutation skipped because no domain file changed relative to HEAD~1.
- `check-library.py`: ng-packagr build succeeds; `npm pack --dry-run --json` lists 4 production files and no spec, step or feature file.
- `angular-failure-check.py`: deliberately failing component and UI assertions, missing Chromium, missing native data and empty native results all exit nonzero with red badges and inspectable reports. Specs were restored from saved copies.
- Not verified: Docker/devcontainer image build (no Docker), external registry publication/installation, other Angular versions, first-time network downloads on an empty package/browser cache.

# Rule
## MUST
### Apply inherited spec and reporting rules
Apply the [Angular base](../solution-conformance-testing-in-angular.skill/solution-conformance-testing-in-angular.skill.md) and this refinement's mutations without replacing its result adapter or spec rules.
- Risk: private copies of shared behavior drift from the application shape.
- Fix: copy shared assets unchanged and use only the library-specific component command and configuration here.

### Test the library through a demo host
Keep browser specs beside the library component and point the inherited [Playwright template](../solution-conformance-testing-in-angular.skill/templates/playwright.ui.config.ts) at that source root and `ng serve demo`.
- Risk: an unserved library has no real browser boundary to assert.
- Fix: build a host that imports the public library entry point; retain the inherited free port and read-only baseline policy.

### Exclude tests from publication
Exclude `test/`, `features/` and `spec/` from production compilation and verify the built package with `npm pack --dry-run --json`.
- Risk: package consumers receive test dependencies or private scenarios.
- Fix: publish only ng-packagr output and fail inspection if any test file is present.

# Check list
- [ ] The four kinds use the unchanged caller contract and shared evidence adapter.
- [ ] Cucumber steps and features lie beside the library logic; native specs lie beside the component in `spec/`.
- [ ] A failing component/browser assertion, empty suite and missing result produce a red kind.
- [ ] The demo uses the public library entry point and receives a free port each run.
- [ ] Package inspection proves no spec, step or feature is shipped.
- [ ] `run-example.sh` passes report, check, living-doc and report-link assertions.
