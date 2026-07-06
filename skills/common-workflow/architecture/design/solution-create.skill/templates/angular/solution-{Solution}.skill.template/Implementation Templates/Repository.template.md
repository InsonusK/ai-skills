---
description: Short description what must be made while creation or change in repository
element_kind: # repository | project | class
change_kind: # create | extend
# - create if solution creates a new repository-level template.
# - extend if solution extends an existing repository-level template.
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for the repository, remove the section or add a note that no changes are introduced.

# Structure

## Project Structure
```hint
Define how solution EXTENDS repository structure.
```
```example
/src
  /app
    /host
```

## Directory and file skills
```hint
Define how solution EXTENDS repository directory and files.
```
```example
| Directory | file | Description        |
| ----------------- | ------------------ |
| /src/app          | project description |
```

| Directory | file | Description |
| ----------------- | ----------- |
|                   |             |

# Rules
```hint
Define how solution EXTENDS repository MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
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
Describe concrete wrong ways to apply the solution at repository level and their consequences.
Each item must tell the agent what NOT to do, why it is harmful, and what to do instead.

Format:
- **{What NOT to do}**
  - Consequence: {negative consequence}
  - Instead: {correct alternative}
```
```example
- **Place environment-specific configuration in source code**
  - Consequence: hard to maintain across environments
  - Instead: use environment files and Angular CLI configurations
```

# Unittest TestCases
```hint
Define how solution EXTENDS repository integration unit tests.

RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] WHEN build application THEN no compilation errors
```
