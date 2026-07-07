---
description: Create or change a Python package __init__.py file
project_name: # The package/project in which the file is located
name: # Package path, e.g. cli or command
element_kind: init
change_kind: # create | extend
# - create if solution creates a new package. The package must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing package, for example by adding re-exports. Link to the package must be added into the `extends` property in the header of the solution.
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this `__init__.py`, remove the section or add a note that no changes are introduced.

# Goals
```hint
Define how solution EXTENDS package goal.
MUST:
- show all added goals
RECOMMENDATION:
- Prefer bullet list
```
```example
- Make `cli` a Python package so submodules can be imported
```

# Core Principles
```hint
Define how solution EXTENDS package core principles.
MUST:
- show all added Core Principles
RECOMMENDATION:
- Prefer bullet list
```
```example
- `__init__.py` is present in every package directory
```

# Naming convention
```hint
Package naming convention. Fill table:
- use case - when apply naming convention
- package path pattern - mask of package path. Example: {app}.{layer}
- package path - example of package path. Example: my_app.cli
- file name - always __init__.py
```

| use case | package path pattern | package path | file name |
| -------- | -------------------- | ------------ | --------- |
|          |                      |              |           |

# Implementation changes
```hint
Define how solution EXTENDS package implementation.
```
```example
[[Package init]] must ...
```
```code example
# {App}/cli/__init__.py
```
```code example
# {App}/command/__init__.py
from .backup import BackupResult

__all__ = ["BackupResult"]
```

# Rule changes
```hint
Define how solution EXTENDS package MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
Only add a subblock for categories where this solution introduces new rules.
If a category has no new rules, skip it — do not write an empty subblock.

MUST:
- show all added Rules
```

## MUST
```example
- `__init__.py` must exist in every package directory
```

## SHOULD
```example
- Re-export public members explicitly through `__all__`
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
- Put business logic in `__init__.py`
```

# Anti-patterns
```hint
Describe concrete wrong ways to implement this package init and their consequences.
Each item must tell the agent what NOT to do, why it is harmful, and what to do instead.

Format:
- **{What NOT to do}**
  - Consequence: {negative consequence}
  - Instead: {correct alternative}
```
```example
- **Omit `__init__.py` to rely on implicit namespace packages**
  - Consequence: package discovery becomes fragile and tooling may fail
  - Instead: add an explicit `__init__.py` to every package directory
```

# Check list
```hint
Define how solution EXTENDS package check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] `{App}/{Package}/__init__.py` exists
```
