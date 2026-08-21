---
description: Create the {Module}.Application project
name: "{Module}.Application"
element_kind: project
change_kind: create
tags:
  - solution/module-structure
  - element/module-application-csproj
---

# Goals
- Hold every command, handler, and validator for this module.

# Structure

## Project Structure
```
/{Module}.Application
  /Commands
  /Validators
```

# Allowed Dependencies
- `{Module}.Domain`

# Rule

## MUST
- Reference `{Module}.Domain` and no other module's `{OtherModule}.Domain`.
  - Risk: a direct cross-module domain reference creates hidden coupling between bounded contexts.
  - Fix: only reference this module's own `{Module}.Domain`.
