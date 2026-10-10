---
name: delta-selection
description: Use affected native and unit targets in delta checks while retaining mutation file scope.
problem: What does DELTA_BASE mean for each kind in an Nx repository?
decision: Use affected native and unit targets in delta checks while retaining mutation file scope.
updated: 20261009
tags:
  - solution/conformance-testing-in-angular-nx
  - stack/typescript
  - framework/angular
  - concern/documentation
  - concern/documentation/adr
---

# Problem
What does DELTA_BASE mean for each kind in an Nx repository?

# Selected variant
[Affected checks and full reports](#affected-checks-and-full-reports)

# Searched variants
## Affected checks and full reports
**Selected.**

### Description
The owner's rule (2026-10-09): a check on a pull request tests only the delta. `unit`, `components` and `ui` take the projects `nx show projects --affected --base` names in a `check` run with `DELTA_BASE`, and every applicable project otherwise. Nx derives the dependencies from the imports, so a changed library also selects the application and its e2e project. No affected project explicitly skips. Mutation runs all domain patterns for reports and only changed matching files for checks, using the unchanged TypeScript runner. The scenario inventory remains complete and unaffected scenarios stay not-run.

### Benefits
- Fast project-scoped checks and honest full reports without a new caller contract.

### Costs
- It relies on the dependency graph Nx infers; the kind mode and inventory must explain partial runs.

## All suites in every check
### Description
Run all applicable projects regardless of DELTA_BASE.

### Benefits
- Simpler scope, matching the base application native kinds.

### Costs
- Misses Nx affected behavior requested by the owner and repeats unchanged suites.
