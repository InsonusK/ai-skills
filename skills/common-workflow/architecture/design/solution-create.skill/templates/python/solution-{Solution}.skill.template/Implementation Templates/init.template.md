---
description: Create or change a Python package __init__.py file
project_name: # The package/project in which the file is located
name: # Package path, e.g. cli or command
element_kind: init
change_kind: # create | extend
# - create if solution creates a new package. The package must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing package, for example by adding re-exports. Link to the package must be added into the `extends` property in the header of the solution.
tags:
  - solution/{solution-name}
  - element/{element-name}
  # solution/{solution-name}: the owning solution name without the `solution-` prefix, kebab-case.
  # element/{element-name}: the package init file path in kebab-case, no braces or dots
  # (e.g. {App}/cli/__init__.py -> element/app-cli-init-py).
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
Define how solution EXTENDS package rules. Follow the Rule-section baseline in [[skills/common-workflow/skill-design.skill/skill-design.skill.md|skill-design]]:
- Use only ## MUST, ## SHOULD, ## MAY subblocks — never ## MUST NOT/## SHOULD NOT headings.
- Express a prohibition as a negatively-phrased bullet ("Never ...", "Do not ...") inside ## MUST or ## SHOULD, at whichever strength it actually carries.
- Never add a separate # Anti-patterns section: convert each would-be anti-pattern into a negative bullet with nested `Risk:` (the consequence) and `Fix:` (the correct alternative).
- Every ## MUST bullet carries a nested `Risk:` and `Fix:` (`Violation:` is optional); ## SHOULD bullets carry the elaboration only when the rule is non-obvious; ## MAY bullets never carry it.
- Only add a subblock for categories where this solution introduces new rules.
- If a category has no new rules, skip it — do not write an empty subblock.

MUST:
- show all added Rules
```

## MUST
```example
- `__init__.py` must exist in every package directory
  - Risk: package discovery becomes fragile and tooling may fail.
  - Fix: add an explicit `__init__.py` to every package directory.
- Never put business logic in `__init__.py`.
  - Risk: import-time side effects and blurred package responsibilities.
  - Fix: keep `__init__.py` to imports and re-exports only.
```

## SHOULD
```example
- Re-export public members explicitly through `__all__`
```

## MAY
```example
- ...
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
