---
description: Short description what must be made while creation or change in repository
element_kind: # repository | package | class | module | index
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
/packages
  /{package-name}
```

## Directory and class skills
```hint
Define how solution EXTENDS repository directory and files.
```
```example
| Directory | file | Description        |
| ----------------- | ------------------ |
| /packages/{package-name} | package description |
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
- Every package must declare `test`, `coverage` and `mutation` scripts in `package.json`
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
- **Place business logic in repository-level scripts**
  - Consequence: repositories start enforcing business rules instead of just orchestrating build/test tooling
  - Instead: keep business/validation logic inside packages
```

# Unittest TestCases
```hint
Define how solution EXTENDS repository integration unit tests.

RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] WHEN `npm run mutation` runs at repository root THEN it runs mutation testing for every package
```
