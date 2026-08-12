---
name: module-command-command
description: Create one business operation for a Python CLI application
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
- Implement one operation using typed parameters.
- Validate input, orchestrate services/functions, and return a result.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.command.{Command}.py.create.md|{App}.command.{Command}.py.create]]

# Core Principles
- Apply ONE plateau template per class/module.
- Commands are the application's capabilities. They contain the key business logic.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.command.{Command}.py.create.md|{App}.command.{Command}.py.create]]

# Naming convention

| use case | element name pattern | element name | file name pattern | file name |
| -------- | -------------------- | ------------ | ----------------- | --------- |
| Operation | - | - | command/{command}.py | command/backup.py |

# Implementation

```python
# Skill: module-command-command
# Plateau: plateau-python-cli
# Version: 20260710003814

import logging
from dataclasses import dataclass
from pathlib import Path

import functions.helpers as helpers
import service.backup_service as backup_service

logger = logging.getLogger(__name__)


@dataclass
class BackupResult:
    message: str
    exit_code: int


def run(source: str, destination: str) -> BackupResult:
    logger.info("Backup command invoked for source '%s'", source)
    source_path = Path(source)
    destination_path = Path(destination)

    if not source_path.exists():
        logger.error("Source path does not exist: %s", source)
        return BackupResult(message=f"Source not found: {source}", exit_code=1)

    files = helpers.list_files(source_path)
    logger.info("Found %d files to back up", len(files))

    service = backup_service.BackupService(destination_path)
    try:
        service.backup(files)
    except Exception as exc:
        logger.critical("Backup failed: %s", exc, exc_info=True)
        return BackupResult(message=f"Backup failed: {exc}", exit_code=1)

    return BackupResult(message=f"Backup completed: {len(files)} files", exit_code=0)
```

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.command.{Command}.py.create.md|{App}.command.{Command}.py.create]]

# Rules

## MUST
- Accept typed parameters.
- Validate business preconditions.
- Log workflow milestones, warnings, errors, and caught exceptions.
- Return a result object with an exit code.

## SHOULD NOT
- Parse command-line arguments.

## MUST NOT
- Use `print`.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.command.{Command}.py.create.md|{App}.command.{Command}.py.create]]

# Anti-patterns

- **Apply SEVERAL plateau template per class/module**
  - Consequence: conflicting responsibilities and inconsistent generated code.
  - Instead: apply exactly one plateau module template per file.
- **Put CLI parsing here**
  - Consequence: the command cannot be unit-tested without `argparse`.
  - Instead: receive plain Python types.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.command.{Command}.py.create.md|{App}.command.{Command}.py.create]]

# Check list

- [ ] Parameters are typed.
- [ ] Preconditions are validated.
- [ ] Result includes an exit code.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.command.{Command}.py.create.md|{App}.command.{Command}.py.create]]

# Unittest TestCases

- [ ] WHEN run with valid paths THEN it returns a success result with exit code `0`.
- [ ] WHEN source path does not exist THEN it returns a failure result with exit code `1`.
- [ ] WHEN the service raises an exception THEN it logs the error and returns exit code `1`.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.command.{Command}.py.create.md|{App}.command.{Command}.py.create]]
