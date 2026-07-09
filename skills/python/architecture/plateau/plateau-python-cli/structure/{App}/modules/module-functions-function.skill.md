---
name: module-functions-function
description: Create reusable helper functions for CLI commands
domain: skill
type: template
plateau: plateau-python-cli
version: 20260710003814
tags:
  - skill/template/module
  - plateau/plateau-python-cli
created_by:
  - "[[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]]"
---

# Goal
- Provide pure, reusable helper functions used by Commands.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.functions.{Function}.py.create.md|{App}.functions.{Function}.py.create]]

# Core Principles
- Apply ONE plateau template per class/module.
- Functions are stateless and reusable.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.functions.{Function}.py.create.md|{App}.functions.{Function}.py.create]]

# Naming convention

| use case | element name pattern | element name | file name pattern | file name |
| -------- | -------------------- | ------------ | ----------------- | --------- |
| Helper function | - | - | functions/{function}.py | functions/helpers.py |

# Implementation

```python
# Skill: module-functions-function
# Plateau: plateau-python-cli
# Version: 20260710003814

import logging
from pathlib import Path

logger = logging.getLogger(__name__)


def list_files(directory: Path) -> list[Path]:
    logger.debug("Listing files in %s", directory)
    if not directory.is_dir():
        logger.warning("Directory does not exist or is not a directory: %s", directory)
        return []
    return [path for path in directory.rglob("*") if path.is_file()]
```

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.functions.{Function}.py.create.md|{App}.functions.{Function}.py.create]]

# Rules

## MUST
- Functions must be reusable and stateless where possible.
- Use logging instead of `print`.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.functions.{Function}.py.create.md|{App}.functions.{Function}.py.create]]

# Anti-patterns

- **Apply SEVERAL plateau template per class/module**
  - Consequence: conflicting responsibilities and inconsistent generated code.
  - Instead: apply exactly one plateau module template per file.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.functions.{Function}.py.create.md|{App}.functions.{Function}.py.create]]

# Check list

- [ ] Functions are stateless and reusable.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.functions.{Function}.py.create.md|{App}.functions.{Function}.py.create]]

# Unittest TestCases

- [ ] WHEN `list_files` is called with an existing directory THEN it returns all files recursively.
- [ ] WHEN `list_files` is called with a missing path THEN it returns an empty list and logs a warning.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.functions.{Function}.py.create.md|{App}.functions.{Function}.py.create]]
