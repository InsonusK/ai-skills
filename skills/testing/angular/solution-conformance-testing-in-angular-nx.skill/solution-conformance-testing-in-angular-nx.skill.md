---
name: solution-conformance-testing-in-angular-nx
description: Refines Angular conformance testing for an Nx repository with project aggregation and affected check runs.
whenToUse: Add or maintain conformance testing in an Nx workspace created with the official Angular tooling (`@nx/angular`), holding Angular applications and their libraries in one repository.
domain: skill
type: architecture
version: 20261010100000
updated: 20261009
tags:
  - skill/architecture/solution
  - solution/conformance-testing-in-angular-nx
  - stack/typescript
  - framework/angular
  - concern/testing
creates:
  - tools/testing/nx-kind.mjs
  - tools/testing/nx-env.sh
  - cucumber.mjs
  - stryker.conf.json
  - .nxignore
extends:
  - package.json
  - "{Project}/vite.config.mts"
  - "{E2eProject}/playwright.config.mts"
  - "{Project}/tsconfig.app.json"
  - "{Project}/tsconfig.lib.json"
  - tools/testing/kinds/unit.sh
  - tools/testing/kinds/components.sh
  - tools/testing/kinds/ui.sh
depends_on:
  - "[Angular conformance testing](../solution-conformance-testing-in-angular.skill/solution-conformance-testing-in-angular.skill.md)"
adr:
  - "[Project aggregation](adr/project-aggregation.md)"
  - "[Delta selection](adr/delta-selection.md)"
  - "[Fresh runner evidence](adr/fresh-runner-evidence.md)"
---

# Goal
- Execute the four inherited kinds across an [Nx workspace](glossary/nx.md) and assemble one repository report.
- Preserve project identity and complete scenario inventory, including logic implemented in applications.

# Core Principle
- **The workspace is the declaration** — a kind reads the projects and their standard targets from Nx; no project carries a testing declaration of its own.
- **Standard commands keep working** — `nx test {project}`, `nx e2e {project}` and `npx cucumber-js` run the same tests with the same configuration as `make`.
- **Inherited semantics** — Keep the base assertion rules, evidence adapter and Make contract; replace only project selection and aggregation.

# Boundaries
- The workspace is the one the official tooling makes: `create-nx-workspace --preset=angular-monorepo`, projects from `@nx/angular` and `@nx/js` generators, component tests on Vitest through `@analogjs/vitest-angular` (the generators' `vitest-analog`), browser tests in an `-e2e` project on `@nx/playwright`. Verified with Nx 23.2 and Angular 22.1.
- Browser specs live in the application's `-e2e` project, where Nx puts them — not in a `spec/` folder beside a component as in the single-application base. Component specs stay in `spec/`, scenarios and steps in `features/` and `test/`.
- Mutation testing covers the framework-independent logic the scenarios exercise. Component classes are tested by the `components` kind and are not mutated.
- Chromium installation needs system libraries. Docker images, remote CI execution and other Angular/Nx versions are not verified here.

# Adr
- [Project aggregation](adr/project-aggregation.md): applicability read from the workspace, one run per project, one aggregated result; the base's five badge names.
- [Delta selection](adr/delta-selection.md): affected projects in a `check` run with `DELTA_BASE`; mutation keeps changed-file selection inside the configured patterns.
- [Fresh runner evidence](adr/fresh-runner-evidence.md): every Nx call of a kind skips the task cache; a missing project result is a failure.

# Template Skill Mutations
- [Repository](Implementation/Repository.extend.md): install Nx orchestration and the three replacement kinds.
- [Project](Implementation/Project.extend.md): declare applicability, native targets, dependencies and domain scopes.

# Workflow
1. Create projects with the official generators; apply the Angular base's spec rules and these two mutations.
2. Run `make init && make test-and-report`; open `tmp/testing/report/index.html` and the project evidence linked inside component/UI reports.
3. For a delta check, run `make test-and-report TEST_RUN_PURPOSE=check DELTA_BASE=<commit>`; read which projects were selected, unaffected or without the target in each kind's evidence.

# Ground truth
[The runnable Nx example](example/README.md) is a workspace made by the official generators — one application, its e2e project, an Angular library and a logic-only library — with no testing declaration in any `project.json`. Verified on 2026-10-10 with Node 24.21, Nx 23.2, Angular 22.1, Vitest 4.1 and Playwright with Chromium:
- `make init && make test-and-report`: exit 0; 31/31 scenarios (formatter 4, linkcheck 25, portal 2), line coverage 98.24%, mutation score 91.5%, components 5/5 through `nx run {project}:test`, UI 5/5 through `nx run portal-e2e:e2e` including the reviewed screenshot. The living documentation holds 29 inventory entries with all tags and exclusion reasons.
- `run-example.sh` from a clean checkout — `npm ci`, a report run, a check run with caller-chosen directories, delta mutation: exit 0 in 1 min 51 s, with the npm and browser downloads already cached on the host. A first download was not measured.
- `check-nx.py`, on a two-commit copy that changes `libs/formatter`: `unit` runs formatter and portal, `components` runs portal, `ui` runs portal-e2e, mutation covers the one changed file; linkcheck does not run and its scenarios stay in the inventory as `not-run`. The dependencies come from the imports — no project declares one by hand.
- A component expectation changed between two runs makes the second run red, named for its project: the cache does not answer.
- A project with the `test` target and no spec, and a project whose scenarios are all excluded, each fail their kind and are named. A `check` run with nothing affected skips the kind with its reason and keeps the whole scenario inventory.
- `nx test linkcheck` run by hand executes the same specs.
- Not verified: the `.devcontainer` image build (no Docker here); a workflow on GitHub; a first run with empty npm and browser caches; other Nx or Angular versions; a workspace with several applications and e2e projects; Jest or Cypress as the generators' test runners.
# Rule
## MUST
### Preserve inherited test semantics
Apply the [Angular base](../solution-conformance-testing-in-angular.skill/solution-conformance-testing-in-angular.skill.md), its spec rules, glossary and result adapter before replacing the selection/configuration with this refinement.
- Risk: project roles become competing testing contracts.
- Fix: keep the same four kinds, shared tools and result schema with one repository report.

### Read applicability from the workspace
Let the kinds find their projects — a `.feature` file for `unit`, the standard `test` target for `components`, the standard `e2e` target for `ui` — and add no testing block or testing target to a `project.json`.
- Violation: a `metadata.testing` block, or a `conformance-unit` target beside the generated `test`.
- Risk: hand-written declarations drift from what the generators and plugins maintain, and every new project needs them repeated.
- Fix: generate the project with the official generator; give it specs, and the kind picks it up. A project with the target and no spec fails its kind.

### Keep scenarios beside all logic
Put `features/` and adjacent `test/` beside framework-independent code in applications and libraries alike, and keep them — with `spec/` — out of every production `tsconfig`.
- Risk: application logic escapes the suite and the published living documentation.
- Fix: cover every project in Cucumber discovery and in domain coverage/mutation patterns, keeping Angular specs in adjacent `spec/`; add `src/**/spec/**`, `src/**/test/**` and `src/**/features/**` to the `exclude` of each `tsconfig.app.json` / `tsconfig.lib.json`, or the application build compiles the step files.

### Select affected targets only for delta checks
Select `nx affected` for unit/components/UI only when purpose is `check` and `DELTA_BASE` names a commit; run all applicable projects otherwise.
- Risk: incomplete report runs or changed tests represented as complete workspace evidence.
- Fix: write the decision into mode and project inventory; an empty affected selection explicitly skips, while a selected empty suite fails.

### Retain mutation's file delta
Keep the TypeScript parent's `mutation.sh` unchanged and express all application/library domain scopes in Stryker `mutate` patterns.
- Risk: mutation silently becomes whole-project rather than changed-file testing in checks.
- Fix: run all configured domain files for reports and only changed matching files for delta checks, as decided in [Delta selection](adr/delta-selection.md).

### Require fresh project evidence
Skip the Nx task cache in every call a kind makes and validate a fresh native result for every selected project.
- Risk: replayed outputs hide an unexecuted suite or a failed target.
- Fix: keep `--skip-nx-cache` in the supplied orchestration and the per-project result directories; leave the targets' own cache settings as generated.

# Check list
- [ ] No `project.json` holds a testing declaration; `nx test` and `nx e2e` run the same specs as the kinds.
- [ ] Every selected project has fresh runner output; an empty/missing declared suite is red.
- [ ] Application and library scenarios appear with all tags in the complete living documentation.
- [ ] Components/UI aggregate native evidence with project identity and one badge each.
- [ ] A two-commit fixture proves only affected applicable projects ran in delta checks.
- [ ] Changed failing specs prove cached tasks cannot answer the second run.
- [ ] Mutation stays byte-identical to the parent and covers all declared domain sources.
- [ ] `run-example.sh` passes and caller-selected directories hold transient output.
