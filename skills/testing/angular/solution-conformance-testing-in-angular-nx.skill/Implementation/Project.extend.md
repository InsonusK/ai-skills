---
description: Declare project applicability, native targets and dependency edges.
element_kind: project
change_kind: extend
updated: 20261009
tags:
  - solution/conformance-testing-in-angular-nx
  - element/testing-project
  - stack/typescript
  - framework/angular
  - concern/testing
---

# Goal
- Make every [Nx project's](../glossary/nx.md) test obligations explicit.

# Mutations
The Angular base and repository refinement are applied; no per-role testing skill or alternate spec framework is applied.

Use [the application config](../example/apps/portal/project.json), [Angular library config](../example/libs/linkcheck/project.json) and [domain library config](../example/libs/formatter/project.json) as concrete mutations. Merge native targets into existing `project.json`; do not introduce a parallel `angular.json`. Copy source/configuration shape, replacing project identifiers and paths for the real workspace.

Each project declares `metadata.testing.unit/components/ui` as true or false, plus `uiHost` naming a real application's `serve` target when UI applies. Every true kind has a `conformance-<kind>` target invoking `node tools/testing/nx-project.mjs <kind> <project>` through `nx:run-commands`, with `forwardAllArgs: false` and `cache: false`.

A component-bearing project also has `component-native`, using the Angular unit-test builder, its own spec tsconfig, the inherited Vitest runner config, native JSON reporter, coverage and an application development `buildTarget`. Existing compatible executors can be kept after checking their runtime options; the delivered command passes `outputFile` through Nx and skips cache. Native test/build/serve targets in the runnable example use `@angular/build` builders directly through Nx core.

Declare or infer dependency edges accurately: the example's portal depends on both libraries, so a formatter change selects the portal too. `false` is a deliberate applicability decision (the formatter has no DOM/browser boundary), not a workaround for missing tests. Native specs stay in `spec/`; features and steps stay beside the logic.

# Rule
## MUST
### Validate the project inventory
Run the full kinds after adding or changing a project, then check its selection record and evidence.
- Risk: inaccurate applicability or dependencies silently omits necessary tests from delta checks.
- Fix: add the required suite and target for true kinds; require a reason for false kinds and verify dependency propagation in the affected fixture.
