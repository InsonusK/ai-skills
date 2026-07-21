---
name: build-backend-and-metadata-format
description: Which tool describes and builds a Python CLI application's package metadata
problem: Decide how a CLI application declares its project metadata, build system, and console entry point, and which tool builds the installable package from it
decision: Use a single pyproject.toml with the PEP 621 [project] table and the standard-library-adjacent setuptools.build_meta backend
---

# Problem

A Python CLI application needs one authoritative place to declare its name, version, dependencies, and console command, and one build backend that `pip` invokes to turn a source checkout (local, or fetched from a Git/GitHub URL) into an installable package. Several competing tools solve this (`setuptools` + `pyproject.toml`, Poetry, Hatch, or the legacy `setup.py`/`setup.cfg` pair), and the choice affects what contributors must have installed, how much tooling-specific syntax they must learn, and how portable the project is to other environments.

# Selected variant

**Selected variant:** [[#PEP 621 pyproject.toml with setuptools.build_meta]]

Use one `pyproject.toml` at the repository root: a `[build-system]` table naming `setuptools.build_meta` as the backend, and a standard `[project]` table (PEP 621) for name, version, dependencies, and `[project.scripts]`. This requires no extra tool beyond `pip`/`setuptools`/`wheel`, keeps the layered CLI structure from [[../../solution-default-cli.skill/solution-default-cli.skill.md|solution-default-cli]] framework-agnostic, and matches the exclusion rules solution-test already contributes to `pyproject.toml`.

# Searched variants

## PEP 621 pyproject.toml with setuptools.build_meta

### Description
A single `pyproject.toml` file with a `[build-system]` table (`setuptools.build_meta`) and a `[project]` table following PEP 621's standardized field names (`name`, `version`, `dependencies`, `[project.scripts]`, ...).

### Benefits
- No extra CLI tool to install or teach; `pip`, `setuptools`, and `wheel` are already assumed.
- `[project]` field names are a cross-tool standard (PEP 621), not `setuptools`-proprietary, so the file stays readable even if the backend changes later.
- Works unmodified with `pip install git+https://...` and with plain `pip install .`.

### Costs
- `setuptools.packages.find` still needs an explicit `where`/`exclude` configuration; it does not infer the layout as confidently as some newer backends.
- Contributors used to Poetry's `poetry add`/`poetry publish` workflow need to fall back to plain `pip`/`build`/`twine` commands.

## Poetry ([tool.poetry] + poetry-core backend)

### Description
Declare metadata under Poetry's own `[tool.poetry]` table (pre-PEP 621 style, though modern Poetry also supports `[project]`), and use `poetry-core` as the PEP 517 backend. Dependency management, virtualenv creation, and publishing are done through the `poetry` CLI.

### Benefits
- Integrated dependency resolver and lock file (`poetry.lock`) for reproducible installs.
- Single CLI (`poetry add`, `poetry build`, `poetry publish`) covers most packaging tasks.

### Costs
- Requires contributors to install Poetry itself, an extra dependency beyond `pip`.
- Historically used a proprietary `[tool.poetry]` schema instead of the standard `[project]` table, adding a translation step for anyone reading the file who only knows PEP 621.
- No benefit for this plateau: CLI applications built here have few dependencies and do not need lock-file-level resolution.

## Legacy setup.py / setup.cfg

### Description
Describe metadata in `setup.cfg` (or imperative Python in `setup.py`), with `pyproject.toml` present only minimally (or absent) to satisfy older `pip` versions.

### Benefits
- Long-established pattern; abundant historical documentation and examples.

### Costs
- `setup.py` executes arbitrary Python during install, which is exactly what PEP 517/518 were introduced to move away from.
- Metadata is split across `setup.cfg`/`setup.py` instead of one file, and lacks PEP 621's standardized field names.
- No advantage over `pyproject.toml` for a newly created plateau with no legacy constraint forcing this choice.
