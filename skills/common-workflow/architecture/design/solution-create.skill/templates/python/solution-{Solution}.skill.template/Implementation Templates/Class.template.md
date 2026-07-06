---
description: Short description what must be made while creation or change in a file that contains a class
project_name: # The package/project in which the file is located
name: # Class name
element_kind: class
change_kind: # create | extend
# - create if solution creates a new class template. Name of the class/file must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing class template. Link to the class/file must be added into the `extends` property in the header of the solution.
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this class/file, remove the section or add a note that no changes are introduced.

# Goals
```hint
Define how solution EXTENDS class goal.
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
Define how solution EXTENDS class core principles.
MUST:
- show all added Core Principles
RECOMMENDATION:
- Prefer bullet list
```
```example
- Classes encapsulate behavior and state
```

# Naming convention
```hint
Class and file naming convention. Fill table:
- use case - when apply naming convention
- class name pattern - mask of class name. Example: Is{Rule}
- class name - example of class name. Example: IsEven
- file name pattern - file name pattern. Example: {module_underscore}.py
- file name - example of file name. Example: is_even.py
```

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
|          |                    |            |                   |           |

# Implementation changes
```hint
Define how solution EXTENDS class implementation.
```
```example
[[Class skill]] must ...
```
```code example
class SomeEntity:
    def __init__(self, entity_id: int) -> None:
        self._id = entity_id
```

# Rule changes
```hint
Define how solution EXTENDS class/file MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
Only add a subblock for categories where this solution introduces new rules.
If a category has no new rules, skip it — do not write an empty subblock.

MUST:
- show all added Rules
```

## MUST
```example
- Class must expose a typed public interface
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
Describe concrete wrong ways to implement this class/file and their consequences.
Each item must tell the agent what NOT to do, why it is harmful, and what to do instead.

Format:
- **{What NOT to do}**
  - Consequence: {negative consequence}
  - Instead: {correct alternative}
```
```example
- **Use mutable default arguments in constructors**
  - Consequence: shared state between instances causes subtle bugs
  - Instead: use `None` as default and initialize mutable values inside `__init__`
```

# Check list
```hint
Define how solution EXTENDS class/file check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] Class has a typed public API
```

# Unittest TestCases
```hint
Define how solution EXTENDS class unit tests.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] WHEN create entity with valid data THEN entity is initialized
```
