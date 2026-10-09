---
name: angular-test-kinds
description: Selects native runners and the report boundary for Angular component and browser tests.
version: 20261008210000
updated: 20261008
tags:
  - solution/conformance-testing-in-angular
  - concern/documentation
  - concern/documentation/adr
  - stack/typescript
  - framework/angular
---
# Context
The TypeScript parent covers framework-independent contracts through Cucumber, c8 and Stryker; it explicitly delegates component/browser tests to framework kinds. Angular needs template compilation and [TestBed](../glossary/testbed.md); browser interactions need a served application. The shared core discovers kinds and assembles their evidence without runner-specific changes.

# Selected variant
[Native Angular builder and Playwright kinds](#native-angular-builder-and-playwright-kinds).

# Variants
## Native Angular builder and Playwright kinds
**Selected.**

Description: Angular CLI compiles and initializes TestBed for [Vitest](../glossary/vitest.md) under `components`; Playwright runs browser and optional visual assertions under `ui` using [Playwright](../glossary/playwright.md). Their JSON adapters publish distinct native evidence through the inherited contract.

Benefits: framework-owned compilation, distinct test discovery and coverage scopes, native browser artifacts, no duplicated report assembler or Cucumber wrappers.

Costs: two additional runner dependencies and a browser installation; the delivered CLI adapter targets version 22 and needs an explicit adaptation for older executors.

## Wrap every UI assertion in Cucumber steps
Description: run TestBed and browser lifecycles behind the existing step runner.

Benefits: one narrative scenario inventory for all tests.

Costs: custom template compilation/browser orchestration and fixtures; step counts conflate domain specification with UI execution and native artifacts require extra wiring.

## Replace the TypeScript suite with a framework test runner
Description: move business scenarios into Vitest and use Playwright alongside it.

Benefits: fewer runtime entry points.

Costs: loses inherited executable Gherkin/living documentation and changes the requested extension into a replacement.

# Consequences
Domain coverage/mutation remain inherited and scope-limited, including the delta selector that otherwise overrides Stryker’s configured `mutate` paths; native component coverage belongs to the component report. Both added kinds run full suites in check/report modes and fail visibly on missing evidence. Visual baselines are reviewed source artifacts, never automatically accepted by a normal run.

# Sources
- [Angular testing](https://angular.dev/guide/testing): native Vitest builder and TestBed setup.
- [Angular test command](https://angular.dev/cli/test): runner config, inclusion, JSON reporting and coverage options.
- [Vitest coverage configuration](https://vitest.dev/config/coverage): report directory and reporters.
- [Playwright reporters](https://playwright.dev/docs/test-reporters), [web server](https://playwright.dev/docs/test-webserver), [screenshots](https://playwright.dev/docs/test-snapshots): native evidence, owned server and reviewed expectations.
