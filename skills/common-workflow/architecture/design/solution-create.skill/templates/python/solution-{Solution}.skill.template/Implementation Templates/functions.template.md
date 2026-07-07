---
description: Short description what must be made while creation or change in a file that contains standalone functions
project_name: # The package/project in which the file is located
name: # Module name
element_kind: functions
change_kind: # create | extend
# - create if solution creates a new functions module. Name of the module/file must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing functions module. Link to the module/file must be added into the `extends` property in the header of the solution.
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this module, remove the section or add a note that no changes are introduced.

# Goals
```hint
Define how solution EXTENDS module goal.
MUST:
- show all added Goals
RECOMMENDATION:
- Prefer bullet list
```
```example
- Provide reusable helper functions for command handlers
```

# Core Principles
```hint
Define how solution EXTENDS module core principles.
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
Module and function naming convention. Fill table:
- use case - when apply naming convention
- function name pattern - mask of function name. Example: is_{rule}
- function name - example of function name. Example: is_even
- file name pattern - file name pattern. Example: {module}.py
- file name - example of file name. Example: helpers.py
```

| use case | function name pattern | function name | file name pattern | file name |
| -------- | --------------------- | ------------- | ----------------- | --------- |
|          |                       |               |                   |           |

# Implementation changes
```hint
Define how solution EXTENDS module implementation.
```
```example
[[Functions module]] must ...
```
```code example
import logging
from pathlib import Path

logger = logging.getLogger(__name__)


def list_files(directory: Path) -> list[Path]:
    logger.debug("Listing files in %s", directory)
    return [path for path in directory.rglob("*") if path.is_file()]
```

# Rule changes
```hint
Define how solution EXTENDS module MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
Only add a subblock for categories where this solution introduces new rules.
If a category has no new rules, skip it — do not write an empty subblock.

MUST:
- show all added Rules
```

## MUST
```example
- Functions must be stateless where possible
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
Describe concrete wrong ways to implement this module and their consequences.
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
Define how solution EXTENDS module check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] Functions are stateless and reusable
```

# Unittest TestCases
```hint
Define how solution EXTENDS module unit tests.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] WHEN call helper with valid input THEN returns expected result
```
