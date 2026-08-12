---
name: module-service-service
description: Create a reusable service for CLI commands
domain: skill
type: template
plateau: plateau-python-cli
version: 20260710003814
tags:
  - skill/template/module
  - plateau/plateau-python-cli
  - stack/python
  - concern/architecture

created_by:
  - "[[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]]"
---

# Goal
- Encapsulate stateful or dependency-heavy behavior used by Commands.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.service.{Service}.py.create.md|{App}.service.{Service}.py.create]]

# Core Principles
- Apply ONE plateau template per class/module.
- Services encapsulate a single responsibility.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.service.{Service}.py.create.md|{App}.service.{Service}.py.create]]

# Naming convention

| use case | element name pattern | element name | file name pattern | file name |
| -------- | -------------------- | ------------ | ----------------- | --------- |
| Service class | `{Service}` | BackupService | service/{service}.py | service/backup_service.py |

# Implementation

```python
# Skill: module-service-service
# Plateau: plateau-python-cli
# Version: 20260710003814

import logging
import shutil
from pathlib import Path

logger = logging.getLogger(__name__)


class BackupService:
    def __init__(self, destination: Path) -> None:
        self.destination = destination

    def backup(self, files: list[Path]) -> None:
        logger.info("Backing up %d files to %s", len(files), self.destination)
        self.destination.mkdir(parents=True, exist_ok=True)
        for file in files:
            target = self.destination / file.name
            logger.debug("Copying %s to %s", file, target)
            shutil.copy2(file, target)
```

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.service.{Service}.py.create.md|{App}.service.{Service}.py.create]]

# Rules

## MUST
- Services should encapsulate a single responsibility.
- Use logging.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.service.{Service}.py.create.md|{App}.service.{Service}.py.create]]

# Anti-patterns

- **Apply SEVERAL plateau template per class/module**
  - Consequence: conflicting responsibilities and inconsistent generated code.
  - Instead: apply exactly one plateau module template per file.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.service.{Service}.py.create.md|{App}.service.{Service}.py.create]]

# Check list

- [ ] Service has a single responsibility.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.service.{Service}.py.create.md|{App}.service.{Service}.py.create]]

# Unittest TestCases

- [ ] WHEN `backup` is called with a list of files THEN all files are copied to the destination.
- [ ] WHEN `backup` is called THEN missing destination directories are created.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.service.{Service}.py.create.md|{App}.service.{Service}.py.create]]
