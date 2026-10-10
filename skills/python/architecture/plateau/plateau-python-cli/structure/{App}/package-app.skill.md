---
name: package-app
description: Define the base structure for a Python CLI application package in the plateau-python-cli plateau, with layered CLI, command, functions, service directories, and pip-installable packaging via pyproject.toml
domain: skill
type: template
plateau: plateau-python-cli
version: 20261008150000
tags:
  - skill/template/package
  - plateau/plateau-python-cli
  - stack/python
  - concern/architecture

created_by:
  - "[[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]]"
  - "[[skills/python/architecture/solutions/solution-cli-packaging.skill/solution-cli-packaging.skill.md|solution-cli-packaging]]"
---

# Goal
- Provide a home for the CLI application with a layered structure.
- Describe the package as installable via `pip`, including directly from a Git/GitHub URL, with a runnable console command.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.create.md|{App}.create]]
- [[skills/python/architecture/solutions/solution-cli-packaging.skill/solution-cli-packaging.skill.md|solution-cli-packaging]] - [[skills/python/architecture/solutions/solution-cli-packaging.skill/Implementation/pyproject.toml.create.md|pyproject.toml.create]]

# Core Principles
- Separate CLI, Command, Functions, and Service into their own directories.
- `pyproject.toml` is the single descriptor for build system and project metadata (see [[skills/python/architecture/solutions/solution-cli-packaging.skill/glossary/pyproject-toml.md|glossary: pyproject.toml]]).
- The console command declared in `[project.scripts]` points at the same `{App}.cli:main` built by solution-default-cli.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.create.md|{App}.create]]
- [[skills/python/architecture/solutions/solution-cli-packaging.skill/solution-cli-packaging.skill.md|solution-cli-packaging]] - [[skills/python/architecture/solutions/solution-cli-packaging.skill/Implementation/pyproject.toml.create.md|pyproject.toml.create]]

# Structure

## Repository place

```
/{App}
pyproject.toml
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
pyproject.toml
```

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.create.md|{App}.create]]
- [[skills/python/architecture/solutions/solution-cli-packaging.skill/solution-cli-packaging.skill.md|solution-cli-packaging]] - [[skills/python/architecture/solutions/solution-cli-packaging.skill/Implementation/pyproject.toml.create.md|pyproject.toml.create]]

## Package Metadata (pyproject.toml)

`pyproject.toml` declares the build system and project metadata, and exposes the CLI as a console command. See [[skills/python/architecture/solutions/solution-cli-packaging.skill/glossary/pyproject-toml.md|glossary: pyproject.toml]] for how `pip` uses this file, including to install directly from a Git/GitHub URL.

```toml
[build-system]
requires = ["setuptools>=61.0", "wheel"]
build-backend = "setuptools.build_meta"

[project]
name = "{app-name}"
version = "0.1.0"
description = "{Short description of the CLI application}"
requires-python = ">=3.9"
dependencies = [
]

[project.scripts]
{app-name} = "{App}.cli:main"

[tool.setuptools.packages.find]
where = ["."]
exclude = ["*.test", "*.test.*"]
```

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-cli-packaging.skill/solution-cli-packaging.skill.md|solution-cli-packaging]] - [[skills/python/architecture/solutions/solution-cli-packaging.skill/Implementation/pyproject.toml.create.md|pyproject.toml.create]]

## Directory and module skills

| `Directory|file` | Description | Pattern skill |
| ---------------- | ----------- | ------------- |
| /cli | Argparse wiring, one module per subcommand | [[./modules/module-cli-command.skill.md\|module-cli-command]] |
| cli.py | Entry point, builds parser, dispatches commands | [[./modules/module-cli.skill.md\|module-cli]] |
| /command | Business logic, one module per operation | [[./modules/module-command-command.skill.md\|module-command-command]] |
| /functions | Reusable pure helper functions | [[./modules/module-functions-function.skill.md\|module-functions-function]] |
| /service | Reusable stateful services | [[./modules/module-service-service.skill.md\|module-service-service]] |

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.create.md|{App}.create]]

## Python Dependencies

| Package | Version constraint | Purpose |
| ------- | ------------------ | ------- |
| setuptools | >= 61.0 | Build system for package discovery |
| wheel | - | Build-time dependency required by `[build-system].requires` to produce an installable wheel |

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-cli-packaging.skill/solution-cli-packaging.skill.md|solution-cli-packaging]] - [[skills/python/architecture/solutions/solution-cli-packaging.skill/Implementation/pyproject.toml.create.md|pyproject.toml.create]]

## What Does NOT Belong Here

- A second CLI entry point that bypasses `{App}.cli:main` (e.g. a duplicate script outside `[project.scripts]`).

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-cli-packaging.skill/solution-cli-packaging.skill.md|solution-cli-packaging]] - [[skills/python/architecture/solutions/solution-cli-packaging.skill/Implementation/pyproject.toml.create.md|pyproject.toml.create]]

## Allowed Dependencies

- Standard library only (`argparse`, `logging`, `sys`) for CLI runtime.
- `setuptools`, `wheel` as build-time dependencies; `build`, `twine` (optional, `dev` group) for cutting releases.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.create.md|{App}.create]]
- [[skills/python/architecture/solutions/solution-cli-packaging.skill/solution-cli-packaging.skill.md|solution-cli-packaging]] - [[skills/python/architecture/solutions/solution-cli-packaging.skill/Implementation/pyproject.toml.create.md|pyproject.toml.create]]

# Rules

## MUST
- Create `/cli`, `/command`, `/functions`, and `/service` directories.
- Declare `pyproject.toml` with `[build-system]`, `[project]`, and `[project.scripts]` pointing at `{App}.cli:main`.

## SHOULD
- Declare `[project.urls].Repository` in `pyproject.toml` so an installed package can be traced back to source.

## MUST NOT
- Introduce a second CLI entry point that bypasses `{App}.cli:main`.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.create.md|{App}.create]]
- [[skills/python/architecture/solutions/solution-cli-packaging.skill/solution-cli-packaging.skill.md|solution-cli-packaging]] - [[skills/python/architecture/solutions/solution-cli-packaging.skill/Implementation/pyproject.toml.create.md|pyproject.toml.create]]

# Anti-patterns

- **Place all code in a single script**
  - Consequence: CLI parsing, business logic, and reusable helpers become tightly coupled.
  - Instead: split code into the four layers.
- **Assume a bare `https://github.com/{org}/{repo}` URL installs with `pip`**
  - Consequence: `pip install` fails or installs the wrong thing, because `pip` needs an explicit `git+` prefix (or a direct archive URL) to recognize a VCS repository.
  - Instead: use `pip install git+https://github.com/{org}/{repo}.git`, per [[skills/python/architecture/solutions/solution-cli-packaging.skill/glossary/pyproject-toml.md|glossary: pyproject.toml]].

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.create.md|{App}.create]]
- [[skills/python/architecture/solutions/solution-cli-packaging.skill/solution-cli-packaging.skill.md|solution-cli-packaging]] - [[skills/python/architecture/solutions/solution-cli-packaging.skill/Implementation/pyproject.toml.create.md|pyproject.toml.create]]

# Check list

- [ ] `/cli`, `/command`, `/functions`, `/service` directories exist.
- [ ] `cli.py` is at the package root.
- [ ] `pyproject.toml` exists with `[build-system]`, `[project]`, and `[project.scripts]` pointing at `{App}.cli:main`.
- [ ] `pip install git+https://github.com/{org}/{app-name}.git` succeeds in a clean virtual environment.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] - [[skills/python/architecture/solutions/solution-default-cli.skill/Implementation/{App}.create.md|{App}.create]]
- [[skills/python/architecture/solutions/solution-cli-packaging.skill/solution-cli-packaging.skill.md|solution-cli-packaging]] - [[skills/python/architecture/solutions/solution-cli-packaging.skill/Implementation/pyproject.toml.create.md|pyproject.toml.create]]
