---
name: test location
description: Where TypeScript feature files and steps sit relative to production code
problem: Keep specifications discoverable beside their implementation without shipping or mutating test code.
decision: Co-located features and test folders, production-only build and package
tags:
  - stack/typescript
  - concern/testing/bdd
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Root feature and step trees hide which module owns a behavior. Importing every step from the public root also forces internal module functions to become package exports solely for testing.

# Selected variant
[[#Co-located features and tests]]

# Searched variants

## Co-located features and tests

**Selected.**

### Description
Keep `{package}/features/{rule}.feature` and `{package}/test/{rule}.steps.ts`. Internal behavior steps import adjacent real modules; contract scenarios import the public entry point. The production build excludes `**/test/**`, npm publishes `dist/`, and coverage/mutation exclude tests.

### Benefits
A reader finds implementation, specification and bindings together; submodules own their scenarios and can exercise internal functions without expanding the public API.
### Costs
Runner globs and production build exclusions must stay in sync; shared steps are global to cucumber-js, so duplicate expressions must be consolidated even across folders. Package contents must be inspected after building.

## Root features and steps importing only the public index

### Description
Keep root `features/` and `features/step-definitions/`; steps always import the public entry point.

### Benefits
A simple runner configuration and an automatic boundary between source and tests.
### Costs
The specification is detached from its module, internal testing creates unnecessary exports, and moving a module requires finding tests in another tree.
