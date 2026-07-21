---
name: solution-cli-packaging
description: Define how a Python CLI application declares its project metadata, build system, and console entry point in pyproject.toml so it can be installed with pip, including directly from a Git/GitHub URL
domain: python
type: architecture
version:
tags:
  - skill/architecture/solution
  - python
  - cli
  - packaging
  - architecture
triggers:
  - Create pyproject.toml for a new Python CLI application
  - Add a console command entry point so `pip install` exposes a runnable CLI command
  - Make a CLI application installable via `pip install` from a GitHub repository
depends_on:
  - "[[../solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]]"
creates:
  - "pyproject.toml"
extends:
adr:
  - "[[./adr/build-backend-and-metadata-format.md|Build backend and metadata format]]"
---

# Goal
- Describe a Python CLI application as an installable package: name, version, dependencies, supported Python versions.
- Declare a standard build backend so `pip` can build the package from a local checkout, a source archive, or a Git/GitHub URL.
- Expose a runnable console command backed by the CLI entry point from [[../solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]].

# Capabilities
- `pip install .` and `pip install -e .` work out of the box for local development.
- `pip install git+https://github.com/{org}/{app-name}.git` installs the application directly from its GitHub repository, with no separate publishing step required.
- After install, the application is available as a named command on `PATH` (via `[project.scripts]`), not just as `python -m {App}.cli`.
- Runtime and development dependencies are declared once, in one file, instead of scattered across `setup.py`/`setup.cfg`/`requirements*.txt`.

# Core Principles
- **One file describes the package**: `pyproject.toml` is the single source of truth for build system and project metadata — no `setup.py`, no `setup.cfg`.
- **The CLI entry point is a plain function**: `[project.scripts]` points at `{App}.cli:main`, the same `main()` created by [[../solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]]; packaging never introduces a second, competing entry point.
- **Package discovery matches the real layout**: `[tool.setuptools.packages.find]` must describe where the code actually lives ({App} at repo root vs. a `src/` layout), not a default guess.
- **Runtime and development dependencies are separated**: production `dependencies` stay minimal; tools like `pytest`, `build`, `twine` live under `[project.optional-dependencies].dev`.

See [glossary: pyproject.toml](./glossary/pyproject-toml.md) for what this file is, why it exists, and how `pip` uses it to build and install a package — including from a Git/GitHub URL.

# Adr
- [[./adr/build-backend-and-metadata-format.md|Build backend and metadata format]]
  - Selected variant: PEP 621 `pyproject.toml` with the `setuptools.build_meta` backend, over Poetry or legacy `setup.py`/`setup.cfg`.

# Requirements
SOLUTION:
- [[../solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]]
  - [[../solution-default-cli.skill/Implementation/{App}.cli.py.create.md|cli.py]] - `[project.scripts]` must point at this module's `main()` function.

PYPI:
- `setuptools` (>=61.0)
  - Provides the `setuptools.build_meta` PEP 517 backend and `[tool.setuptools.packages.find]` package discovery.
- `wheel`
  - Build-time dependency declared in `[build-system].requires` so `pip` can produce a wheel.
- `build`, `twine` (optional, `dev` group)
  - Used to build and publish release artifacts (`python -m build`, `twine upload`).

STANDARD LIBRARY / TOOLING:
- `pip`
  - The build frontend that reads `[build-system]`, drives the PEP 517 build, and installs the resulting wheel — including from a Git/GitHub URL. See [glossary: pyproject.toml](./glossary/pyproject-toml.md).

# Template Skill Mutations
PROJECT:
- [[./Implementation/pyproject.toml.create.md|pyproject.toml]] - create - project metadata, build system, and console entry point for the CLI application

# Workflow
## Bootstrap packaging for a new CLI app
1. Confirm [[../solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] is already applied, so `{App}/cli.py` exposes an argument-optional `main()`.
2. Create `pyproject.toml` at the repository root from [[./Implementation/pyproject.toml.create.md|pyproject.toml.create]].
3. Fill in `name`, `description`, `requires-python`, and runtime `dependencies`.
4. Add a `[project.scripts]` entry mapping the console command name to `{App}.cli:main`.
5. Run `pip install -e .` locally and confirm the console command runs.
6. Push to GitHub and verify `pip install git+https://github.com/{org}/{app-name}.git` succeeds in a clean virtual environment.

## Install directly from GitHub
1. User runs `pip install git+https://github.com/{org}/{app-name}.git`.
2. `pip` fetches the repository and reads `[build-system]` from `pyproject.toml`.
3. `pip` installs the declared backend (`setuptools.build_meta`) into an isolated build environment and invokes it to build a wheel.
4. `pip` installs the wheel into the target environment and writes a console-script wrapper for each `[project.scripts]` entry.
5. The command declared in `[project.scripts]` is now available on `PATH`.

See [pip-install-from-github](./diagrams/pip-install-from-github.mmd).

## Add a new runtime dependency
1. Add the package (with a minimum version) to `[project].dependencies`.
2. Reinstall with `pip install -e .` to pick up the change locally.

## Cut a release
1. Bump `[project].version`.
2. Build artifacts with `python -m build` and publish with `twine upload` if the package is also distributed via PyPI.

# Rules
## MUST
- [[./Implementation/pyproject.toml.create.md#MUST|pyproject.toml.create]]

## SHOULD
- [[./Implementation/pyproject.toml.create.md#SHOULD|pyproject.toml.create]]

## MUST NOT
- [[./Implementation/pyproject.toml.create.md#MUST NOT|pyproject.toml.create]]
- Introduce a second CLI entry point (e.g. a duplicate `if __name__ == "__main__"` script) that bypasses `{App}.cli:main`.

# Anti-patterns
- [[./Implementation/pyproject.toml.create.md#Anti-patterns|pyproject.toml.create]]
- **Manage metadata in `setup.py`/`setup.cfg` alongside `pyproject.toml`**
  - Consequence: two sources of truth drift apart, and it becomes unclear which file `pip` actually reads for a given field.
  - Instead: keep all metadata in `pyproject.toml`'s `[project]` table, per the decision in [[./adr/build-backend-and-metadata-format.md|Build backend and metadata format]].
- **Assume a bare `https://github.com/{org}/{repo}` URL installs with `pip`**
  - Consequence: `pip install https://github.com/{org}/{repo}` fails or installs the wrong thing, because `pip` needs an explicit `git+` prefix (or a direct archive URL) to know it is fetching a VCS repository.
  - Instead: use `pip install git+https://github.com/{org}/{repo}.git`, as documented in [glossary: pyproject.toml](./glossary/pyproject-toml.md).

# Check list
- [ ] `pyproject.toml` exists at the repository root with `[build-system]`, `[project]`, and `[project.scripts]`.
- [ ] `[project.scripts]` points at `{App}.cli:main`, and `main()` accepts no required arguments.
- [ ] `pip install -e .` succeeds locally and the console command runs.
- [ ] `pip install git+https://github.com/{org}/{app-name}.git` succeeds in a clean virtual environment.
- [ ] Development-only tools live under `[project.optional-dependencies].dev`, not in `dependencies`.
