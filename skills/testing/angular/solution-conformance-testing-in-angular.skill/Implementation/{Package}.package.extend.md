---
description: Configure the Angular compiler and separate testing source sets.
element_kind: package
change_kind: extend
updated: 20261008
tags:
  - solution/conformance-testing-in-angular
  - element/angular-package
  - stack/typescript
  - framework/angular
  - concern/testing
---

# Goal
- Compile component specs through Angular and isolate the three testing source sets.

# Mutations
Apply [TypeScript package setup](skills/testing/typescript/solution-conformance-testing-in-typescript.skill/Implementation/{Package}.package.extend.md); its Cucumber/domain tools remain in place. Do not apply a second Karma/Analog runner setup alongside the native Angular builder.

Add compatible locked dev dependencies from the root skill's Requirements. Configure the application's `angular.json` `test` target to use `@angular/build:unit-test` and its actual `tsconfig.spec.json`. Its development build target must compile the same application used by the UI server. Disable Angular persistent caching (`cli.cache.enabled: false`) in this delivered setup so test invocations do not create `.angular/cache` outside the owning kind; the supplied [Vitest](../glossary/vitest.md) cache is already routed through `TEST_KIND_DIR`.

Set `tsconfig.spec.json` to include component specs (`src/**/test/*.component.spec.ts`) and required declarations, with `vitest/globals` types; exclude `src/**/test/*.ui.spec.ts` and `src/**/test/*.steps.ts`. Keep browser specs and [Playwright](../glossary/playwright.md) config outside that compilation.

Exclude `**/test/**`, `**/features/**` and screenshots from the production compilation/package file list. Cucumber's inherited globs continue to load only `.steps.ts`; do not import a native spec from a step file.

Constrain the inherited c8 include and Stryker `mutate` paths to the framework-independent production modules actually exercised by Cucumber, preserving its test-source exclusions. In the project copy of `tools/testing/kinds/mutation.sh`, replace its `FILES=` assignment with [the domain delta selector](../templates/domain-delta-selection.sh), substituting only `{DomainRoot}` with that same framework-independent source directory (for example `src/domain`). The inherited check-mode `--mutate` overrides Stryker’s configured patterns, so both selections must agree; update the adjacent no-changes message to name the domain scope. Shared core files and the public caller contract stay unchanged. Angular components, templates and browser specs require their own mutation adapter if component mutation is later requested; component line coverage does not prove domain mutation adequacy.

# Rules
## MUST
### Keep compiler ownership explicit
Run component specs through Angular's unit-test builder and browser specs through Playwright.
- Risk: direct Vitest cannot compile Angular templates correctly or loads browser specs into [TestBed](../glossary/testbed.md).
- Fix: use the supplied distinct filename globs, tsconfig boundaries and Angular test target.

### Report distinct coverage scopes
Label native TestBed coverage as component coverage and inherited c8/Stryker evidence as the Cucumber-tested domain scope.
- Risk: one green percentage falsely implies the entire application or UI has been mutation-tested.
- Fix: inspect both runners' selected sources and retain the native component coverage under `reports/components/coverage/`.

### Execute the complete native suites
Run all component and UI tests in both purposes without filtering by changed filenames.
- Risk: a template, stylesheet or shared dependency changes behavior outside the edited file's neighborhood.
- Fix: retain the full-suite adapters and verify every expected spec is discovered.
