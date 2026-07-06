---
description: Short description what must be made while creation or change in module/class
project_name: # The project/package in which the module/class is located
name: # Module or class name
element_kind: # repository | project | class
change_kind: # create | extend
# - create if solution creates a new module/class template. Name of the module/class must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing module/class template. Link to the module/class must be added into the `extends` property in the header of the solution.
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this module/class, remove the section or add a note that no changes are introduced.

# Goals
```hint
Define how solution EXTENDS module/class goal.
MUST:
- show all added Goals
RECOMMENDATION:
- Prefer bullet list
```
```example
- Prevent duplicate creation via uniqueness check
```

# Core Principles
```hint
Define how solution EXTENDS module/class core principles.
MUST:
- show all added Core Principles
RECOMMENDATION:
- Prefer bullet list
```
```example
- Functions are stateless and reusable
```

# Naming convention
```hint
Module/class naming convention. Fill table:
- use case - when apply naming convention
- class name pattern - mask of class name. Example: Is{Rule}
- class name - example of class name. Example: IsEven
- file name pattern - file name pattern. Example: {module}.py
- file name - example of file name. Example: is_even.py
```

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
|          |                    |            |                   |           |

# Implementation changes
```hint
Define how solution EXTENDS module/class implementation.
```
```example
[[Module skill]] must ...
```
```code example
class SomeEntity:
    def __init__(self, id: int, guid: UUID) -> None:
        self._id = id
        self._guid = guid
```

# Rule changes
```hint
Define how solution EXTENDS module/class MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
Only add a subblock for categories where this solution introduces new rules.
If a category has no new rules, skip it — do not write an empty subblock.

MUST:
- show all added Rules
```

## MUST
```example
- Command must be a typed function or class
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
Describe concrete wrong ways to implement this module/class and their consequences.
Each item must tell the agent what NOT to do, why it is harmful, and what to do instead.

Format:
- **{What NOT to do}**
  - Consequence: {negative consequence}
  - Instead: {correct alternative}
```
```example
- **Use mutable default arguments**
  - Consequence: shared state between calls causes subtle bugs
  - Instead: use `None` as default and initialize mutable values inside the function
```

# Check list
```hint
Define how solution EXTENDS module/class check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] Module has a typed public API
```

# Unittest TestCases
```hint
Define how solution EXTENDS module/class unit tests.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] WHEN call command with event THEN
  - [ ] event fills domain event in entity
  - [ ] handler catches event and processes it
```
