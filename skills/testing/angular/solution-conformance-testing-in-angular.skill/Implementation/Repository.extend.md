---
description: Add Angular component and browser kinds to the inherited report contract.
element_kind: repository
change_kind: extend
updated: 20261009
tags:
  - solution/conformance-testing-in-angular
  - element/testing-repository
  - stack/typescript
  - framework/angular
  - concern/testing
---

# Goal
- Add two discovered kinds to the repository already configured by the TypeScript solution.

# Mutations
Apply [TypeScript repository setup](skills/testing/typescript/solution-conformance-testing-in-typescript.skill/Implementation/Repository.extend.md) first; no additional plateau or alternative runner setup is applied.

Copy these files verbatim from this skill's `assets/` to the corresponding project paths:
- [components.sh](../assets/tools/testing/kinds/components.sh) → `tools/testing/kinds/components.sh`.
- [ui.sh](../assets/tools/testing/kinds/ui.sh) → `tools/testing/kinds/ui.sh`.
- [angular-results.mjs](../assets/tools/testing/angular-results.mjs) → `tools/testing/angular-results.mjs`.
- [vitest.components.config.mts](../assets/vitest.components.config.mts) → `vitest.components.config.mts`.

Copy [playwright.ui.config.ts](../templates/playwright.ui.config.ts) to the project root; replace only the `{SourceRoot}` and `{ServeCommand}` string literals with the real source root and the Angular serve command of the intended application (for example `npm run start --`). [Playwright](../glossary/playwright.md)'s other brace expressions are native snapshot placeholders; leave them intact. The config appends `--host 127.0.0.1 --port` with the free port `ui.sh` picked for this run (`UI_TEST_PORT`), so the serve command names neither; it must stay alive until Playwright stops it.

Select the repository-shape refinement for a library or Nx workspace: `solution-conformance-testing-in-angular-library` or `solution-conformance-testing-in-angular-nx`. The direct command here assumes one application; refinements inherit these assets and replace selection/configuration.

The Playwright template discovers all existing browser layers under `spec/`: `.ui.spec.ts`, `.visual.spec.ts`, `.style-snapshot.spec.ts`, `.a11y.spec.ts`. Keep the same matcher when instantiating it; assertion rules remain the component/catalog responsibility.

Extend README Shields endpoint links with `badges/components.json` and `badges/ui.json` beside the inherited badges. Their destination links open `reports/components/` and `reports/ui/` under the published report root.

The [complete example Makefile](../example/Makefile) exposes the inherited public contract. Its `init` target prepares dependencies; testing targets come exclusively from the shared include. [Example README](../example/README.md) documents the caller commands and report destination.

# Rules
## MUST
### Preserve discovery and caller paths
Use the existing Makefile include, shared core scripts and supplied `TEST_KIND_DIR` without adding public make variables or changing the assembler.
- Risk: language-specific wiring leaks into CI or one kind overwrites another kind's evidence.
- Fix: let filenames and `# badges:` declare the kinds; use their existing automatic `test-kind-components` and `test-kind-ui` targets.

### Prepare dependencies before running kinds
Install from the committed lockfile and provision the matching Chromium browser and its system libraries before invoking these kinds.
- Risk: implicit downloads change versions or a missing runtime gets mistaken for a successful skip.
- Fix: run `npm ci` and the pinned local Playwright `install --with-deps chromium` command as environment preparation; on Linux this uses the system package manager and may require sudo; fail initialization when installation fails instead of proceeding to tests with missing libraries.

### Let the kind choose the port
Serve the application on the port `ui.sh` passes in `UI_TEST_PORT`; write no port number into the config or the serve command.
- Risk: with a fixed port, a run that starts while the server of the previous run on the same machine is still shutting down fails before its first assertion.
- Fix: keep the delivered `ui.sh` and config template; they take a free loopback port for every run.

### Preserve native evidence
Keep native JSON, runner logs, coverage, traces and screenshot diffs inside the owning kind directory and publish the assembled report without dropping its subdirectories.
- Risk: a badge has no inspectable proof or the HTML links lose their assets.
- Fix: copy the delivered assets unchanged and verify links after `make test-report`.
