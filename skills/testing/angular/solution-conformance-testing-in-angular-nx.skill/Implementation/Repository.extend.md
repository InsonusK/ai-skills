---
description: Aggregate Nx projects under the inherited testing contract.
element_kind: repository
change_kind: extend
updated: 20261009
tags:
  - solution/conformance-testing-in-angular-nx
  - element/testing-repository
  - stack/typescript
  - framework/angular
  - concern/testing
---

# Goal
- Supply project selection, native execution and aggregation for an [Nx workspace](../glossary/nx.md).

# Mutations
Apply the [Angular repository setup](../../solution-conformance-testing-in-angular.skill/Implementation/Repository.extend.md). No plateau or alternate assertion framework is applied.

Copy verbatim from `assets/`: [nx-kind.mjs](../assets/tools/testing/nx-kind.mjs), [nx-project.mjs](../assets/tools/testing/nx-project.mjs), [nx-env.sh](../assets/tools/testing/nx-env.sh) to matching `tools/testing/` paths; copy [unit.sh](../assets/tools/testing/kinds/unit.sh), [components.sh](../assets/tools/testing/kinds/components.sh) and [ui.sh](../assets/tools/testing/kinds/ui.sh) over those kinds. Keep the base Angular result adapter and TypeScript mutation kind unchanged. Install a locked `nx` dependency matching the workspace (example: 23.3.0).

Merge [nx.json](../assets/nx.json) target cache settings; keep existing plugins and dependency inference. Adapt source globs in [cucumber.mjs](../assets/cucumber.mjs) and [stryker.conf.json](../assets/stryker.conf.json), and package `c8.include`, to every project with logic. A Cucumber project run selects that project's features while loading shared support; a mutation run runs all features. Explicit domain patterns exclude specs and steps.

Use [Vitest configuration](../assets/vitest.components.config.mts) unchanged. Instantiate the base Playwright config template as [the Nx config](../assets/playwright.ui.config.ts): source selection comes from internal project metadata, and the server runs the project's `uiHost:serve` target on the inherited free port. The orchestration supplies internal project variables; add no caller variables.

# Rule
## MUST
### Retain evidence while aggregating
Keep per-project JSON, logs, coverage and browser artifacts inside the kind; aggregate native JSON with project-prefixed test names before calling the inherited adapter.
- Risk: an overall badge hides which project failed.
- Fix: use the supplied aggregation and publish project links in native reports; link the readable project selection/count/evidence page from living documentation and retain `projects.json` inside the tests report for affected/inapplicable decisions.

### Keep caches within the kind
Use `nx-env.sh` and per-project cache paths, preserving the already-installed browser location before relocating runner caches.
- Risk: custom output directories still write transient state elsewhere, or Playwright starts looking for Chromium in an empty relocated cache.
- Fix: keep the supplied environment wiring and require preparation through `make init`.
