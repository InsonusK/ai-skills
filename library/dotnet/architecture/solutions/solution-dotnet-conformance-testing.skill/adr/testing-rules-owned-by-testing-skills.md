---
name: testing-rules-owned-by-testing-skills
description: Generic testing rules delegated to testing skills
problem: Catalog test templates duplicate testing rules and conflict with the stack-owned folder convention
decision: Catalog keeps project selection and layer contracts
tags:
  - solution/dotnet-conformance-testing
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The previous test lists also contradicted their mirrored-boundary promise: Domain was treated as a dependency-free leaf, while production allows Interfaces/Shared, and Application contract dependencies were excluded. Resolve those lists by linking the production project's assembled Allowed Dependencies, as the owner's task requires.

Catalog test templates duplicate testing rules and conflict with the stack-owned folder convention.

# Selected variant
[Catalog keeps project selection and layer contracts](#catalog-keeps-project-selection-and-layer-contracts)

# Searched variants

## Catalog keeps project selection and layer contracts
**Selected.**

### Description
The catalog links the testing skills for layout, packages, runner behavior, bindings and assertion strength; it retains project selection, mirrored Allowed Dependencies and layer-specific step shapes.

### Benefits
- A single owner prevents conflicting instructions across the testing skills and architecture catalog.

### Costs
- Catalog consumers must load the linked testing skills.

## Repeat generic rules in the catalog

### Description
Keep the older catalog convention and its duplicated generic testing instructions.

### Benefits
- Existing catalog examples need no migration.

### Costs
- Applying the catalog alongside the testing skill produces conflicting layouts and duplicated rules that drift.
