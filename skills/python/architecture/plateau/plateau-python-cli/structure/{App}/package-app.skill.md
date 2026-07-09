---
name: package-app
description: Define the base structure for a Python CLI application package with layered CLI, command, functions, service directories and a mirrored test layout
domain: skill
type: template
plateau: plateau-python-cli
version: 20260710003814
tags:
  - skill/template/package
  - plateau/plateau-python-cli
created_by:
  - "[[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]]"
  - "[[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]]"
---

# Goal
- Provide a home for the CLI application with a layered structure.
- Define a consistent approach to organizing tests in Python projects.
- Keep test structure aligned with source structure so tests are easy to find and maintain.
- Ensure tests are excluded from installed packages.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.create.md|{App}.create]]
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]]

# Core Principles
- Separate CLI, Command, Functions, and Service into their own directories.
- Test structure mirrors source structure.
- One test module per source module.
- Test files use the `_test.py` suffix.
- Tests are not part of the installed package.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.create.md|{App}.create]]
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]]

# Structure

## Repository place

```
/{App}
```

## Package Structure

```
/{App}
  /cli
    __init__.py
    backup.py
  /command
    __init__.py
    backup.py
  /functions
    __init__.py
    helpers.py
  /service
    __init__.py
    backup_service.py
  cli.py
/test
  /{Package}
    __init__.py
    {Module}_test.py
  {Module}_test.py
```

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.create.md|{App}.create]]
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/test.{Module}_test.py.create.md|test.{Module}_test.py.create]]
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/test.{Package}.{Module}_test.py.create.md|test.{Package}.{Module}_test.py.create]]

## Directory and module skills

| `Directory|file` | Description | Pattern skill |
| ---------------- | ----------- | ------------- |
| /cli | Argparse wiring, one module per subcommand | [[./modules/module-cli-command.skill.md\|module-cli-command]] |
| cli.py | Entry point, builds parser, dispatches commands | [[./modules/module-cli.skill.md\|module-cli]] |
| /command | Business logic, one module per operation | [[./modules/module-command-command.skill.md\|module-command-command]] |
| /functions | Reusable pure helper functions | [[./modules/module-functions-function.skill.md\|module-functions-function]] |
| /service | Reusable stateful services | [[./modules/module-service-service.skill.md\|module-service-service]] |
| /test | Mirrored tests for source modules | [[./modules/module-test-module-test.skill.md\|module-test-module-test]] |

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.create.md|{App}.create]]
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/test.{Module}_test.py.create.md|test.{Module}_test.py.create]]

## Python Dependencies

| Package | Version constraint | Purpose |
| ------- | ------------------ | ------- |
| setuptools | >= 61.0 | Build system for package discovery and test exclusion |

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/pyproject.toml.extend.md|pyproject.toml.extend]]

## What Does NOT Belong Here

- Test files inside the installed package (must be excluded by `pyproject.toml`).

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/pyproject.toml.extend.md|pyproject.toml.extend]]

## Allowed Dependencies

- Standard library only (`argparse`, `logging`, `sys`) for CLI runtime.
- `unittest` for tests.
- `pytest` (optional) as alternative test runner.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.create.md|{App}.create]]
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]]

# Rules

## MUST
- Create `/cli`, `/command`, `/functions`, and `/service` directories.
- Mirror source structure in the `test/` directory.
- Name test files with the `_test.py` suffix.
- Exclude test directories and test modules from the installed package via build configuration.
- Write one test module per source module.

## SHOULD
- Use the top-level `test/` directory as the default layout.

## SHOULD NOT
- Mix tests for multiple source modules in a single test file.
- Place test logic inside the source package.

## MUST NOT
- Include test files or test directories in the installed package.
- Put all tests in a single flat `tests/` directory without structure.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.create.md|{App}.create]]
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/pyproject.toml.extend.md|pyproject.toml.extend]]
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]]

# Anti-patterns

- **Place all code in a single script**
  - Consequence: CLI parsing, business logic, and reusable helpers become tightly coupled.
  - Instead: split code into the four layers.
- **Put all tests in a flat `tests/` directory**
  - Consequence: it becomes hard to understand which test covers which module; developers must read each test to find the matching source.
  - Instead: mirror the source structure inside `test/` so every source module has a predictable test location.
- **Name test files without the `_test.py` suffix**
  - Consequence: test runners may not discover the files automatically.
  - Instead: always use `{Module}_test.py`.
- **Ship tests as part of the installed package**
  - Consequence: installed package size grows and test code may run in production environments.
  - Instead: configure `pyproject.toml` to exclude test directories and modules from package discovery.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.create.md|{App}.create]]
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/test.{Module}_test.py.create.md|test.{Module}_test.py.create]]
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/pyproject.toml.extend.md|pyproject.toml.extend]]
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]]

# Check list

- [ ] `/cli`, `/command`, `/functions`, `/service` directories exist.
- [ ] `cli.py` is at the package root.
- [ ] Test directory structure mirrors source structure.
- [ ] Each source module has a corresponding test module.
- [ ] Test files use the `_test.py` suffix.
- [ ] Tests are excluded from the installed package via `pyproject.toml`.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.create.md|{App}.create]]
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/pyproject.toml.extend.md|pyproject.toml.extend]]
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]]
