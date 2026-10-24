---
name: devops-project-version-in-python
description: Python implementation of devops-project-version — the read-version.sh that prints project.version of the root pyproject.toml, where project-version-in-python records the version
whenToUse: when a Python project needs `make version`/`make version-check`, when you add or review the `version` of its `pyproject.toml` or `tools/version/read-version.sh`, or when Python code must report its own version
updated: 20261024
tags:
  - stack/python
  - concern/ci
  - versioning
---

# Goal
- The version recorded per [project-version-in-python](library/develop/python/project-version-in-python.skill.md): the literal `project.version` of the root `pyproject.toml`.
- `tools/version/read-version.sh`, an unchanged copy of this skill's asset.

# Core Principle
- This skill details [[library/devops/core/devops-project-version.skill/devops-project-version.skill.md|devops-project-version]] for Python; apply both.
- **Reads what project-version-in-python records** - Where the version lives, how the program reports it, and how a snapshot overrides it are [project-version-in-python](library/develop/python/project-version-in-python.skill.md)'s; this skill only reads it.

# Rule

## MUST

### Copy read-version.sh verbatim
Copy [[./assets/tools/version/read-version.sh|read-version.sh]] verbatim to `tools/version/read-version.sh`; do not modify it.
- Risk: a `grep` for `version =` also matches a `[tool.*]` table and prints a dependency's number.
- Fix: restore the file from the asset; it parses the file with `tomllib` and needs Python 3.11 or later.

# Check list
- [ ] `tools/version/read-version.sh` is byte-identical to this skill's asset.
- [ ] `make -s version` prints `project.version`.
