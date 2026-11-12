---
name: features-and-steps-layout
description: One .NET test-project layout
problem: The catalog and testing skill specify different folders for the same feature files and bindings
decision: Features and Steps owned by the testing skill; under a folder per kind of what is tested when the production project has such folders (Commands, Specifications, Services), shared steps in Common/Steps
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

Amended by the owner on 2026-11-07: a test project whose production project has folders of their own kinds mirrors them, `features/` and `Steps/` under each (`Todo.Application.Tests/Commands/{features,Steps}`, `Specifications/{features,Steps}`, `Services/{features,Steps}`), and keeps the steps several folders share in `Common/Steps`. The scenarios of a command, of a specification, and of a service then stand beside the others of their kind, not in one list.

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
