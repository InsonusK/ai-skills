---
name: demo-host
description: Supply a real application boundary for one Angular library.
problem: A library has no serve target and its ng-packagr build cannot supply application compilation for native tests.
decision: Keep a projects/demo application consuming the library public entry point.
updated: 20261009
tags:
  - solution/conformance-testing-in-angular-library
  - stack/typescript
  - framework/angular
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Which browser host and compilation target should a publishable library use?

# Selected variant
[Projects demo application](#projects-demo-application)

# Searched variants
## Projects demo application
**Selected.**

### Description
Use the design-system catalog's `projects/demo` convention. The browser opens this host; the Angular component target selects the library specs with the demo's development application build as `buildTarget`.

### Benefits
- Reuses an existing convention and the base Playwright/TestBed evidence adapters.
- Public-entry imports demonstrate how an application consumes the component.

### Costs
- The repository must maintain a small host and its build configuration.

## Standalone browser harness
### Description
Serve hand-written HTML or a separate custom bundler for browser specs.

### Benefits
- Avoids a second Angular CLI project.

### Costs
- Introduces a third hosting convention and another compilation path.
- Does not provide the application's native Angular builder boundary.
