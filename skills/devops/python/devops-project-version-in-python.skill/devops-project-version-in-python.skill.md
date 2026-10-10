---
name: devops-project-version-in-python
description: Python implementation of devops-project-version — the version recorded as project.version in the root pyproject.toml, the read-version.sh that prints it, and the running code reading it from the installed package's metadata
whenToUse: when a Python project needs `make version`/`make version-check`, when you add or review the `version` of its `pyproject.toml` or `tools/version/read-version.sh`, or when Python code must report its own version
updated: 20261010
tags:
  - stack/python
  - concern/ci
  - versioning
---

# Goal
- `project.version` in the root `pyproject.toml` holding the project's version as a literal.
- `tools/version/read-version.sh`, an unchanged copy of this skill's asset.
- No second copy of the version number in the source tree.

# Core Principle
- This skill details [[skills/devops/core/devops-project-version.skill/devops-project-version.skill.md|devops-project-version]] for Python; apply both.
- `pyproject.toml` is what the build backend reads, so a built package carries the recorded version with no extra step.

# Rule

## MUST

### Record the version as project.version
Write the version as a literal `version = "…"` in the `[project]` table of the root `pyproject.toml`.
- Violation: `dynamic = ["version"]` with the number taken from a git tag or a `__version__` attribute.
- Risk: `read-version.sh` finds no `project.version`, and `make version` fails.
- Fix: remove `version` from `dynamic` and write the literal.

### Copy read-version.sh verbatim
Copy [[./assets/tools/version/read-version.sh|read-version.sh]] verbatim to `tools/version/read-version.sh`; do not modify it.
- Risk: a `grep` for `version =` also matches a `[tool.*]` table and prints a dependency's number.
- Fix: restore the file from the asset; it parses the file with `tomllib` and needs Python 3.11 or later.

### Read the running version from package metadata
Report the version at run time with `importlib.metadata.version("{package-name}")`.
- Violation: `__version__ = "1.4.0"` in `__init__.py`.
- Risk: the second number is forgotten on a bump, and the program reports the previous version.
- Fix: delete the literal and read the metadata of the installed package.

# Check list
- [ ] `[project]` in the root `pyproject.toml` has a literal `version`, and `version` is not in `dynamic`.
- [ ] `tools/version/read-version.sh` is byte-identical to this skill's asset.
- [ ] `make -s version` prints `project.version`.
- [ ] No `__version__` literal or other copy of the number exists in the source tree.
