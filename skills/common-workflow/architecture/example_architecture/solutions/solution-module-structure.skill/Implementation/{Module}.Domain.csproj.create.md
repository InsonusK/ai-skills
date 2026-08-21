---
description: Create the {Module}.Domain project
name: "{Module}.Domain"
element_kind: project
change_kind: create
tags:
  - solution/module-structure
  - element/module-domain-csproj
---

# Goals
- Hold every value object and entity for this module.

# Structure

## Project Structure
```
/{Module}.Domain
  /ValueObjects
  /Entities
```

# Allowed Dependencies
- None. `{Module}.Domain` references no other project in this example.

# Rule

## MUST
- Add every new value object under `/ValueObjects` and every new entity under `/Entities`.
  - Risk: files scattered outside these folders are harder for an agent to discover by convention.
  - Fix: follow the fixed folder layout shown above.
