---
name: test-layout-owned-by-testing-skill
description: Rule tests use the stack-owned layout
problem: VP4 rule test templates retain the old feature and binding folders
decision: Use the stack-owned layout for rule tests
tags:
  - solution/domain-shared-rules
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem
VP4 rule test templates retain the old feature and binding folders.

# Selected variant
[Use the stack-owned layout for rule tests](#use-the-stack-owned-layout-for-rule-tests)

# Searched variants

## Use the stack-owned layout for rule tests
**Selected.**

### Description
Domain.Rules.Tests applies cucumber-testing-in-dotnet; VP4 retains its shared-spec links, classification scope and direct rule entry points.

### Benefits
- A single owner prevents conflicting instructions across the testing skills and architecture catalog.

### Costs
- Linked feature metadata and derived plateau templates need updating.

## Retain VP4-specific folders

### Description
Keep the older catalog convention and its duplicated generic testing instructions.

### Benefits
- Existing catalog examples need no migration.

### Costs
- Applying the catalog alongside the testing skill produces conflicting layouts and duplicated rules that drift.
