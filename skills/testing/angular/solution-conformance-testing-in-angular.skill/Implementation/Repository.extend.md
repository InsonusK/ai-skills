---
description: Add Angular component and browser kinds to the inherited report contract.
element_kind: repository
change_kind: extend
updated: 20261008
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

Copy [playwright.ui.config.ts](../templates/playwright.ui.config.ts) to the project root; replace only `{SourceRoot}`, `{BaseUrl}`, `{ServeCommand}` string literals with the real source root, owned loopback URL and Angular serve command. [Playwright](../glossary/playwright.md)'s other brace expressions are native snapshot placeholders; leave them intact. The serve command must select the intended application, bind the chosen port and stay alive until Playwright stops it.

For a multi-project workspace, append the intended application name to the `ng test` command through a separately recorded project adapter decision since its delivered command assumes a single application.

Extend README Shields endpoint links with `badges/components.json` and `badges/ui.json` beside the inherited badges. Their destination links open `reports/components/` and `reports/ui/` under the published report root.

The [complete example Makefile](../example/Makefile) exposes the inherited public contract. Its `init` target prepares dependencies; testing targets come exclusively from the shared include. [Example README](../example/README.md) documents the caller commands and report destination.

# Rules
## MUST
### Preserve discovery and caller paths
Use the existing Makefile include, shared core scripts and supplied `TEST_KIND_DIR` without adding public make variables or changing the assembler.
- Risk: language-specific wiring leaks into CI or one kind overwrites another kind's evidence.
- Fix: let filenames and `# badges:` declare the kinds; use their existing automatic `test-kind-components` and `test-kind-ui` targets.

### Prepare dependencies before running kinds
Install from the committed lockfile and provision the matching Chromium browser before invoking these kinds.
- Risk: implicit downloads change versions or a missing runtime gets mistaken for a successful skip.
- Fix: run `npm ci` and the pinned local Playwright `install chromium` command as environment preparation; retain a nonzero kind result when prerequisites are absent.

### Preserve native evidence
Keep native JSON, runner logs, coverage, traces and screenshot diffs inside the owning kind directory and publish the assembled report without dropping its subdirectories.
- Risk: a badge has no inspectable proof or the HTML links lose their assets.
- Fix: copy the delivered assets unchanged and verify links after `make test-report`.
