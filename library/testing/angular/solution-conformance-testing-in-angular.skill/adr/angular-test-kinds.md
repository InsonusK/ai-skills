---
name: angular-test-kinds
description: Which runners test Angular components and the served application, and how their results reach the common report
problem: The TypeScript parent proves framework-independent rules through Cucumber and leaves component and browser tests to the UI framework; Angular needs template compilation, TestBed and a served application, and their results must reach the same report without changing the shared tools
decision: Two test kinds of their own - components (Angular CLI unit-test builder, Vitest, TestBed, jsdom) and ui (Playwright, Chromium) - each with its native evidence and its badge
tags:
  - solution/conformance-testing-in-angular
  - concern/documentation
  - concern/documentation/adr
  - stack/typescript
  - framework/angular
---

# Problem
The TypeScript parent covers framework-independent contracts through Cucumber, c8 and Stryker, and delegates component and browser tests to the UI framework's own solution. Angular needs template compilation and [TestBed](../glossary/testbed.md) for a component test; a browser interaction needs the served application. The shared core discovers kinds by file name and assembles their evidence, so the choice is which runners to use and whether their tests become kinds of their own.

# Selected variant
**Selected variant:** [[#Native Angular builder and Playwright kinds]]

# Searched variants

## Native Angular builder and Playwright kinds

**Selected.**

### Description
The Angular CLI compiles the specs and initializes TestBed for [Vitest](../glossary/vitest.md) under the `components` kind; [Playwright](../glossary/playwright.md) runs browser and optional screenshot assertions under the `ui` kind. An adapter turns each runner's native JSON into the kind's result, badge and report page. Both kinds run their whole suite in a `check` and in a `report` run. Domain coverage and mutation stay with the inherited `unit` and `mutation` kinds, limited to the framework-independent modules; component coverage belongs to the component report.

### Benefits
- The framework owns compilation; each runner discovers its own files and measures its own coverage scope.
- Native browser evidence - traces, screenshots, diffs - is published as it is.
- No second report builder and no Cucumber wrapper around TestBed or the browser.

### Costs
- Two more runner dependencies and a browser installation.
- The delivered setup targets Angular CLI 22; an older executor needs its own adapter.
- Native specs are not in the living doc: it lists Gherkin scenarios only.

## Wrap every UI assertion in Cucumber steps

### Description
Run TestBed and the browser lifecycle behind the existing step runner.

### Benefits
- One scenario inventory for every test.

### Costs
- Custom template compilation, browser orchestration and fixtures inside step code.
- Step counts mix the domain specification with UI execution; native artifacts need extra wiring.

## Replace the TypeScript suite with a framework test runner

### Description
Move the business scenarios into Vitest and run Playwright beside it.

### Benefits
- Fewer runtime entry points.

### Costs
- Loses the executable Gherkin and its living doc; the extension becomes a replacement of its parent.

# Sources
- [Angular testing](https://angular.dev/guide/testing) and the [Angular test command](https://angular.dev/cli/test): the Vitest builder, TestBed setup, runner config, JSON reporting, coverage.
- [Vitest coverage configuration](https://vitest.dev/config/coverage).
- Playwright [reporters](https://playwright.dev/docs/test-reporters), [web server](https://playwright.dev/docs/test-webserver), [screenshots](https://playwright.dev/docs/test-snapshots).
