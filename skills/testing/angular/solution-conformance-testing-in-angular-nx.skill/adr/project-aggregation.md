---
name: project-aggregation
description: Aggregate explicit project suites through the inherited result adapter.
problem: How should one repository expose results from several Angular projects?
decision: Aggregate explicit project suites through the inherited result adapter.
updated: 20261009
tags:
  - solution/conformance-testing-in-angular-nx
  - stack/typescript
  - framework/angular
  - concern/documentation
  - concern/documentation/adr
---

# Problem
How should one repository expose results from several Angular projects?

# Selected variant
[Explicit applicability with kind aggregation](#explicit-applicability-with-kind-aggregation)

# Searched variants
## Explicit applicability with kind aggregation
**Selected.**

### Description
Declare applicable kinds in every project. Collect isolated fresh results and prefix native test names with project identity. Keep the existing five repository badge names; domain-only libraries explicitly declare no component/UI suite.

### Benefits
- Project omissions and empty declared suites fail; report users can inspect each project.

### Costs
- Metadata and dependency edges must be maintained.

## One badge per project
### Description
Expose each project as its own kind/badge.

### Benefits
- Immediate project visibility in README.

### Costs
- The number of public kinds depends on the workspace and duplicates report assembly.
