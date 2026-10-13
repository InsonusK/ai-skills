---
description: Add project selection, per-project execution and aggregation to an Nx workspace on top of its standard targets
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
- Project selection, per-project execution and aggregation for an [Nx workspace](../glossary/nx.md), on top of the standard targets.

# Mutations
Apply the [Angular repository setup](../../solution-conformance-testing-in-angular.skill/Implementation/Repository.extend.md) for the shared `tools/testing/`, `tools/livingdoc/`, `Makefile` include and README badges. Its `vitest.components.config.mts` and root `playwright.ui.config.ts` are not used here: each project has its generated `vite.config.mts`, and the e2e project its own Playwright config.

Copy verbatim from `assets/`: [nx-kind.mjs](../assets/tools/testing/nx-kind.mjs) and [nx-env.sh](../assets/tools/testing/nx-env.sh) to `tools/testing/`; [unit.sh](../assets/tools/testing/kinds/unit.sh), [components.sh](../assets/tools/testing/kinds/components.sh) and [ui.sh](../assets/tools/testing/kinds/ui.sh) over those kinds. Keep the base Angular result adapter and the TypeScript `mutation.sh` unchanged.

Add `@cucumber/cucumber`, `tsx`, `c8` and `@stryker-mutator/core` to the workspace's dev dependencies. Copy [cucumber.mjs](../assets/cucumber.mjs) to the workspace root and adapt its globs to the workspace's project folders; it points `tsx` at `tsconfig.base.json`, so step files resolve the workspace's path aliases. Copy [stryker.conf.json](../assets/stryker.conf.json) and set `mutate`, and `c8.include` in `package.json`, to the framework-independent code of every project.

Create `.nxignore` with `tools/livingdoc`, `tmp` and `out`: the report tool has a `package.json` of its own, and Nx would list it as a project.

`make init` runs `npm ci` and installs Chromium with its system libraries, as in the base.

# Rule
## MUST
### Retain evidence while aggregating
Keep per-project JSON, logs, coverage and browser artifacts inside the kind; aggregate native JSON with project-prefixed test names before calling the inherited adapter.
- Risk: an overall badge hides which project failed.
- Fix: use the supplied aggregation and publish project links in native reports; link the project selection page from the living documentation.

### Keep caches within the kind
Use `nx-env.sh`, preserving the already-installed browser location before relocating runner caches.
- Risk: custom output directories still write transient state elsewhere, or Playwright looks for Chromium in an empty relocated cache.
- Fix: keep the supplied environment wiring and prepare through `make init`.

### Keep the report tool out of the project graph
List `tools/livingdoc` in `.nxignore`.
- Risk: Nx treats the report tool as a project; it has no tests, and its presence changes what `affected` returns.
- Fix: the `.nxignore` entry above.
