---
name: module-cli
description: Create the CLI entry point for a Python CLI application
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
- Build the argument parser and register subcommands.
- Configure logging before any command runs.
- Dispatch to the correct CLI subcommand module.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.cli.py.create.md|{App}.cli.py.create]]

# Core Principles
- Apply ONE plateau template per class/module.
- `cli.py` is the only file aware of all available subcommands.
- Logging configuration belongs here, not in individual commands.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.cli.py.create.md|{App}.cli.py.create]]

# Naming convention

| use case | element name pattern | element name | file name pattern | file name |
| -------- | -------------------- | ------------ | ----------------- | --------- |
| Entry point | - | - | cli.py | cli.py |

# Implementation

```python
# Skill: module-cli
# Plateau: plateau-python-cli
# Version: 20260710003814

import argparse
import logging
import sys

import cli.backup as backup_cli


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(prog="{App}")
    parser.add_argument(
        "--debug",
        action="store_true",
        default=False,
        help="Enable debug logging.",
    )

    subparsers = parser.add_subparsers(dest="command", required=True)
    backup_cli.register(subparsers)

    args = parser.parse_args(argv)

    logging.basicConfig(
        level=logging.DEBUG if args.debug else logging.INFO,
        format="%(asctime)s - %(name)s - %(levelname)s - %(message)s",
    )

    # `args.run` was set by the active subcommand (`parser.set_defaults(run=run)`).
    # This keeps cli.py free of per-command `if` branches.
    return args.run(args)


if __name__ == "__main__":
    sys.exit(main())
```

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.cli.py.create.md|{App}.cli.py.create]]

# Rules

## MUST
- Accept `--debug` and configure logging before invoking any command.
- Register every subcommand in `cli.py`.
- Use `sys.exit` to return the command's exit code.

## SHOULD NOT
- Contain business logic.

## MUST NOT
- Use `print` for output.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.cli.py.create.md|{App}.cli.py.create]]

# Anti-patterns

- **Apply SEVERAL plateau template per class/module**
  - Consequence: conflicting responsibilities and inconsistent generated code.
  - Instead: apply exactly one plateau module template per file.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.cli.py.create.md|{App}.cli.py.create]]

# Check list

- [ ] `--debug` flag is defined.
- [ ] Logging is configured before `args.run(args)`.
- [ ] All subcommands are registered.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.cli.py.create.md|{App}.cli.py.create]]

# Unittest TestCases

- [ ] WHEN `main` is called with a valid subcommand THEN it returns the command exit code.
- [ ] WHEN `--debug` is passed THEN the root logger level is set to `DEBUG`.
- [ ] WHEN an unknown subcommand is passed THEN `argparse` exits with code `2`.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.cli.py.create.md|{App}.cli.py.create]]
