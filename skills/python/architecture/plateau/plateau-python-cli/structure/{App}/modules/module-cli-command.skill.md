---
name: module-cli-command
description: Create argparse wiring for one CLI subcommand
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
- Declare one subcommand and its arguments.
- Convert parsed arguments into typed Python values.
- Call `command/{command}.py` and print the result.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.cli.{Command}.py.create.md|{App}.cli.{Command}.py.create]]

# Core Principles
- Apply ONE plateau template per class/module.
- This module knows nothing about the business logic; it only translates terminal input to Python types.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.cli.{Command}.py.create.md|{App}.cli.{Command}.py.create]]

# Naming convention

| use case | element name pattern | element name | file name pattern | file name |
| -------- | -------------------- | ------------ | ----------------- | --------- |
| Subcommand wiring | - | - | cli/{command}.py | cli/backup.py |

# Implementation

```python
# Skill: module-cli-command
# Plateau: plateau-python-cli
# Version: 20260710003814

import argparse
import logging

import command.backup as backup_command

logger = logging.getLogger(__name__)


def register(subparsers: argparse._SubParsersAction) -> None:
    parser = subparsers.add_parser("backup", help="Create a backup.")
    parser.add_argument("--source", required=True, help="Path to back up.")
    parser.add_argument("--destination", required=True, help="Backup destination.")
    # Store this module's `run` in the parsed args so cli.py can dispatch to it.
    parser.set_defaults(run=run)


def run(args: argparse.Namespace) -> int:
    logger.info("Starting backup from '%s' to '%s'", args.source, args.destination)
    result = backup_command.run(source=args.source, destination=args.destination)
    print(result.message)
    return result.exit_code
```

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.cli.{Command}.py.create.md|{App}.cli.{Command}.py.create]]

# Rules

## MUST
- Convert raw `Namespace` attributes into named/typed arguments before calling the Command.
- Return an integer exit code.

## SHOULD NOT
- Validate business rules.

## MUST NOT
- Call `print` for logging.
- Perform business logic directly.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.cli.{Command}.py.create.md|{App}.cli.{Command}.py.create]]

# Anti-patterns

- **Apply SEVERAL plateau template per class/module**
  - Consequence: conflicting responsibilities and inconsistent generated code.
  - Instead: apply exactly one plateau module template per file.
- **Parse arguments inside the Command**
  - Consequence: the Command becomes tied to `argparse` and cannot be reused.
  - Instead: do all parsing here and pass typed values to the Command.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.cli.{Command}.py.create.md|{App}.cli.{Command}.py.create]]

# Check list

- [ ] `register` adds the subcommand to the parent parser.
- [ ] `run` extracts typed values and calls the Command.
- [ ] Result is printed to the terminal.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.cli.{Command}.py.create.md|{App}.cli.{Command}.py.create]]

# Unittest TestCases

- [ ] WHEN `register` is called THEN the subcommand is added to the parent parser.
- [ ] WHEN `run` is called with valid args THEN it delegates to the matching Command and returns its exit code.
- [ ] WHEN `run` is called THEN it prints the command result message.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.cli.{Command}.py.create.md|{App}.cli.{Command}.py.create]]
