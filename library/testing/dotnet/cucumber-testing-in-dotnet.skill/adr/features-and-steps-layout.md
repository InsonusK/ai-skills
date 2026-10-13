---
name: features-and-steps-layout
description: One .NET test-project layout
problem: The catalog and testing skill specify different folders for the same feature files and bindings
decision: Features and Steps owned by the testing skill
tags:
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The catalog and testing skill specify different folders for the same feature files and bindings.

# Selected variant
[Features and Steps owned by the testing skill](#features-and-steps-owned-by-the-testing-skill)

# Searched variants

## Features and Steps owned by the testing skill
**Selected.**

### Description
The owner selected `features/` and `Steps/` on 2026-10-09. This skill owns the convention; catalog skills link it and retain their layer-specific constraints.

### Benefits
- A single owner prevents conflicting instructions across the testing skills and architecture catalog.

### Costs
- Existing catalog projects and incoming skill links must be updated.

## Catalog-specific folders

### Description
Keep the older catalog convention and its duplicated generic testing instructions.

### Benefits
- Existing catalog examples need no migration.

### Costs
- Applying the catalog alongside the testing skill produces conflicting layouts and duplicated rules that drift.
