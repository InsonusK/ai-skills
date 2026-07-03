---
description: Short description what must be made while creation or change in project
name: # Project name
element_kind: # repository | project | class
change_kind: # create | extend
# - create if solution creates a new project template. Name of the project must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing project template. Link to the project must be added into the `extends` property in the header of the solution.
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this project, remove the section or add a note that no changes are introduced.

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
- Entities define consistency.
```

# Structure

## Project Structure
```hint
Define how solution EXTENDS project structure.
```
```example
/ProjectName
  /DirectoryName
    ClassesInDirectory.cs
```

## Directory and class skills
```hint
Define how solution EXTENDS project directory and files.
```
```example
| Directory | file   | Description           |
| ------------------- | --------------------- |
| /DirectoryName      | Directory description |
| ClassInDirectory.cs | Class description     |
```

| Directory | file | Description |
| ----------------- | ----------- |
|                   |             |

# NuGet Packages
```hint
Define how solution EXTENDS project NuGet dependencies.
```
```example
| Package   | Version constraint | Purpose                |
| --------- | ------------------ | ---------------------- |
| Ardalis   | >= 8.0             | SpecificationEvaluator |
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
- Commands - belong to [[Other csproj skill]]
```

# Allowed Dependencies
```hint
Define how solution EXTENDS allowed dependencies that project may have.
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
What means that solution applied wrong.
```
```example
- Domain service duplicates invariant already enforced in entity setter or method
```

# Check list
```hint
Define how solution EXTENDS project check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] `int Id` with `internal set` present
```
