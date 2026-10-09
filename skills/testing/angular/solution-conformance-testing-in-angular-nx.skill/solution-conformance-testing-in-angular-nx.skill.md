---
name: solution-conformance-testing-in-angular-nx
description: Refines Angular conformance testing for an Nx repository with project aggregation and affected check runs.
whenToUse: Add or maintain conformance testing in an Nx workspace containing Angular applications and their libraries in one repository.
domain: skill
type: architecture
version: 20261009210100
updated: 20261009
tags:
  - skill/architecture/solution
  - solution/conformance-testing-in-angular-nx
  - stack/typescript
  - framework/angular
  - concern/testing
creates:
  - tools/testing/nx-kind.mjs
  - tools/testing/nx-project.mjs
  - tools/testing/nx-env.sh
extends:
  - nx.json
  - "{Project}/project.json"
  - cucumber.mjs
  - stryker.conf.json
  - tools/testing/kinds/unit.sh
  - tools/testing/kinds/components.sh
  - tools/testing/kinds/ui.sh
  - playwright.ui.config.ts
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
- Test applicability is explicit for each project; a missing target or empty applicable suite fails.
- **Inherited semantics** — Keep the base assertion rules, evidence adapter and Make contract; replace only project selection and aggregation.

# Boundaries
- The example uses Nx 23 core with Angular 22 native builders in `project.json`; existing Nx Angular executors can be retained behind the supplied native-target boundary with a verified option/output adapter.
- Project dependency edges must be accurate. The small example declares `implicitDependencies` explicitly; a production workspace may infer them through its installed Nx plugins.
- Chromium installation needs system libraries. Docker images, remote CI execution and other Angular/Nx version combinations are not verified here.

# Adr
- [Project aggregation](adr/project-aggregation.md): explicit applicability, isolated evidence and inherited normalization; keep the base's five badge names, rather than adding badges for projects.
- [Delta selection](adr/delta-selection.md): affected unit/component/UI targets in delta check runs; mutation keeps changed-file selection inside configured domain patterns.
- [Fresh runner evidence](adr/fresh-runner-evidence.md): bypass all task caches and reject missing fresh project outputs.

# Template Skill Mutations
- [Repository](Implementation/Repository.extend.md): install Nx orchestration and the three replacement kinds.
- [Project](Implementation/Project.extend.md): declare applicability, native targets, dependencies and domain scopes.

# Workflow
1. Apply the Angular base and these two mutations; author specs under its inherited rules.
2. Run `make init && make test-and-report`; open `tmp/testing/report/index.html` and the project evidence linked inside component/UI reports.
3. For a delta check, run `make test-and-report TEST_RUN_PURPOSE=check DELTA_BASE=<commit>`; read the selected/unaffected/inapplicable project inventory in each kind's evidence.

# Ground truth
[The runnable Nx example](example/README.md) was verified on 2026-10-09 with Node 24.21.0, Nx 23.3.0, Angular 22, Vitest 5 and Playwright 1.64/Chromium:
- `make init && make test-and-report`: exit 0; 31/31 domain scenarios (formatter 4, linkcheck 25, portal 2), line coverage 98.24%, mutation 91.5%, components 5/5 and UI 5/5 across the application and Angular library. Living documentation contains 29 inventory entries (outline Examples blocks), all tags and exclusion reasons.
- Dependency-cold npm-ci initialization plus full report: 56.4 seconds on a host with warmed npm/browser/system-library caches. This is not a first-download measurement and is below ten minutes.
- `run-example.sh`: exit 0 for report and caller-chosen check paths; living documentation, relative report links and mutation skip without a base verified. No output appears in default tmp/public during the caller-chosen check.
- `check-nx.py`: a two-commit isolated copy changes formatter source. Unit executes formatter+portal (6 scenarios), components/UI execute portal only; mutation JSON contains only the changed formatter file. Unaffected linkcheck scenarios remain visible as not-run.
- A correct component run followed by a wrong portal expectation exits nonzero with a red aggregated badge and project-prefixed failure. Nx output confirms cache bypass; all targets and nested commands disable local/remote cache.
- A declared empty unit project, a zero-exit component target publishing no result, and a missing declared target each fail visibly with a red badge. A zero-affected unit run retains the complete scenario inventory and explicitly skips without a badge/report.
- A temporary `*.a11y.spec.ts` browser suite executes under the UI kind, proving the catalog suffix is discovered. Normal runs leave the inherited screenshot baseline unchanged.
- Not verified: Docker/devcontainer image build (no Docker); GitHub workflows/remote execution; first-time downloads with empty npm/browser caches; other Angular/Nx versions; existing `@nx/angular` executor variants; automatic dependency-inference plugins; multiple distinct application hosts in one run.

# Rule
## MUST
### Preserve inherited test semantics
Apply the [Angular base](../solution-conformance-testing-in-angular.skill/solution-conformance-testing-in-angular.skill.md), its spec rules, glossary and result adapter before replacing the selection/configuration with this refinement.
- Risk: project roles become competing testing contracts.
- Fix: keep the same four kinds, shared tools and result schema with one repository report.

### Declare applicability for every project
Declare `metadata.testing.unit`, `components` and `ui` as booleans in every project and supply `conformance-<kind>` targets for each true value.
- Risk: a project silently disappears from a green workspace run.
- Fix: reject absent metadata, absent targets, empty applicable suites and missing output; list explicit false values as inapplicable in the evidence.

### Keep scenarios beside all logic
Put `features/` and adjacent `test/` beside framework-independent code in applications and libraries alike.
- Risk: application logic escapes the suite and the published living documentation.
- Fix: cover every project in Cucumber discovery and in domain coverage/mutation patterns, keeping Angular specs in adjacent `spec/`.

### Select affected targets only for delta checks
Select `nx affected` for unit/components/UI only when purpose is `check` and `DELTA_BASE` names a commit; run all applicable projects otherwise.
- Risk: incomplete report runs or changed tests represented as complete workspace evidence.
- Fix: write the decision into mode and project inventory; an empty affected selection explicitly skips, while a selected empty suite fails.

### Retain mutation's file delta
Keep the TypeScript parent's `mutation.sh` unchanged and express all application/library domain scopes in Stryker `mutate` patterns.
- Risk: mutation silently becomes whole-project rather than changed-file testing in checks.
- Fix: run all configured domain files for reports and only changed matching files for delta checks, as decided in [Delta selection](adr/delta-selection.md).

### Require fresh project evidence
Disable local/remote Nx task cache use and validate fresh native JSON and project exit status for every selected project.
- Risk: replayed outputs hide an unexecuted suite or a failed target.
- Fix: use the supplied cache flags, isolated kind/project directories and freshness checks; verify a changed failing spec makes the next run red.

# Check list
- [ ] All projects declare applicability and accurate dependency edges.
- [ ] Every selected project has fresh runner output; an empty/missing declared suite is red.
- [ ] Application and library scenarios appear with all tags in the complete living documentation.
- [ ] Components/UI aggregate native evidence with project identity and one badge each.
- [ ] A two-commit fixture proves only affected applicable projects ran in delta checks.
- [ ] Changed failing specs prove cached tasks cannot answer the second run.
- [ ] Mutation stays byte-identical to the parent and covers all declared domain sources.
- [ ] `run-example.sh` passes and caller-selected directories hold transient output.
