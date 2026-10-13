---
name: repository-shape-selection
description: Choose the entry skill by repository shape.
problem: How does an agent select Angular test delivery for application, library and Nx repositories?
decision: Choose the entry skill by repository shape.
updated: 20261009
tags:
  - solution/conformance-testing-in-angular
  - stack/typescript
  - framework/angular
  - concern/documentation
  - concern/documentation/adr
---

# Problem
How does an agent select Angular test delivery for application, library and Nx repositories?

# Selected variant
[Three repository shapes](#three-repository-shapes)

# Searched variants
## Three repository shapes
**Selected.**

### Description
Owner-decided: one application uses this base directly, even when consuming packages from external repositories. One publishable library and an Nx workspace use two refinements which inherit spec/result rules and replace runner selection/configuration. The task names are solution-conformance-testing-in-angular-library and solution-conformance-testing-in-angular-nx.

### Benefits
- One report contract per repository and one unambiguous entry skill.

### Costs
- Refinements must keep their configuration and runnable examples aligned with shared assets.

## One skill per Nx project role
### Description
Select separate delivery for each application or package inside a workspace.

### Benefits
- Role-specific commands appear simpler individually.

### Costs
- Competes with the workspace-wide run/report contract and misses application-owned logic.
