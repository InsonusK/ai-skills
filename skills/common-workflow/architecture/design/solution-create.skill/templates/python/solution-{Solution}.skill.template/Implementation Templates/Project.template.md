---
description: Short description what must be made while creation or change in project/package
name: # Project or package name
element_kind: # repository | project | class
change_kind: # create | extend
# - create if solution creates a new project/package template. Name of the project/package must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing project/package template. Link to the project/package must be added into the `extends` property in the header of the solution.
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this project/package, remove the section or add a note that no changes are introduced.

# Goals
```hint
Define how solution EXTENDS project goal.
MUST:
- show all added goals
RECOMMENDATION:
- Prefer bullet list
```
```example
- Encapsulate domain logic
```

# Core Principles
```hint
Define how solution EXTENDS project core principles.
MUST:
- show all added Core Principles
RECOMMENDATION:
- Prefer bullet list
```
```example
- Commands contain business logic
```

# Structure

## Project Structure
```hint
Define how solution EXTENDS project/package structure.
```
```example
/{App}
  /cli
    backup.py
  /command
    backup.py
  cli.py
```

## Directory and module skills
```hint
Define how solution EXTENDS project/package directory and files.
```
```example
| Directory | file   | Description           |
| ------------------- | --------------------- |
| /cli                | CLI wiring            |
| /command/backup.py  | Backup operation      |
```

| Directory | file | Description |
| ----------------- | ----------- |
|                   |             |

# Dependencies
```hint
Define how solution EXTENDS project dependencies (PyPI, standard library, or framework packages).
```
```example
| Package   | Version constraint | Purpose                |
| --------- | ------------------ | ---------------------- |
| pydantic  | >= 2.0             | Request DTO validation |
```

| Package | Version constraint | Purpose |
| ------- | ------------------ | ------- |
|         |                    |         |

# What Does NOT Belong Here
```hint
Define how solution EXTENDS project components which do not belong to it.
RECOMMENDATION:
- Prefer bullet list
```
```example
- Commands - belong to [[Other solution skill]]
```

# Allowed Dependencies
```hint
Define how solution EXTENDS allowed dependencies that project/package may have.
RECOMMENDATION:
- Prefer bullet list
ATTENTION:
- Solution should not change allowed dependencies. Confirm extension from user before adding.
```
```example
- [[Shared]]
```

# Rules
```hint
Define how solution EXTENDS project MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
Only add a subblock for categories where this solution introduces new rules.
If a category has no new rules, skip it — do not write an empty subblock.

MUST:
- show all added Rules
```

## MUST
```example
- ...
```

## SHOULD
```example
- ...
```

## MAY
```example
- ...
```

## SHOULD NOT
```example
- ...
```

## MUST NOT
```example
- ...
```

# Anti-patterns
```hint
Describe concrete wrong ways to apply the solution to this project/package and their consequences.
Each item must tell the agent what NOT to do, why it is harmful, and what to do instead.

Format:
- **{What NOT to do}**
  - Consequence: {negative consequence}
  - Instead: {correct alternative}
```
```example
- **Reference database connection directly from CLI layer**
  - Consequence: couples orchestration to persistence and breaks testability
  - Instead: use a repository/service from the command layer
```

# Check list
```hint
Define how solution EXTENDS project check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] `__init__.py` is present in every package
```
