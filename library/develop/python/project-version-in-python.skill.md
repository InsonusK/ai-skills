---
name: project-version-in-python
description: The version of a Python application — the literal project.version of the root pyproject.toml, reported by the running code with importlib.metadata.version, never a __version__ or a version.py holding the number; a snapshot build rewrites the line in its own checkout before building
whenToUse: when a Python program must report its version, when you add or review the version of pyproject.toml, a __version__, or a version module, or when a Python build must carry another version than the recorded one
updated: 20261025
tags:
  - stack/python
  - concern/coding
  - versioning
---

# Goal
- **The version in Python** - A Python application that keeps every rule of [project-version](library/develop/core/project-version.skill.md); this skill details it for Python and restates none of its rules.

# Core Principle
- **The build backend carries it** - `pyproject.toml` is what the build backend reads, so the built and installed package carries the recorded version in its metadata, and the program reads it from there.

# Rule

## MUST

### Record the version as project.version
Write the version as a literal `version = "MAJOR.MINOR.PATCH"` at the start of a line in the `[project]` table of the root `pyproject.toml`.
- Violation: `dynamic = ["version"]` with the number taken from a git tag or a `__version__` attribute; the key indented or written without spaces.
- Risk: the tools that read the version find no literal; a snapshot build's rewrite of the line does not match it.
- Fix: remove `version` from `dynamic` and write the literal on its own line.

```toml
[project]
name = "todo"
version = "1.4.0"
```

### Report the version from the package metadata
Report the running version with `importlib.metadata.version("{package-name}")`, called where it is reported.

```python
from importlib.metadata import version

logger.info("starting", extra=attrs(version=version("todo")))
```

- Violation: `__version__ = "1.4.0"` in `__init__.py`; a `version.py` module — even one that only calls `importlib.metadata` — which reads as the place of the version.
- Risk: a literal is forgotten on a bump; a `version.py` invites the next person to write the number into it.
- Fix: call `importlib.metadata.version` where the version is reported; the program runs installed (`pip install -e .` in development), so the metadata exists.

### Override the version only in the build's checkout
Give a snapshot build its version by rewriting the `version = "…"` line in the build's own checkout before building (`{version}.dev{timestamp}`, PEP 440), and never commit the rewrite.
- Violation: a committed `pyproject.toml` with a `.dev` version; a build that sets the version through an environment variable the backend ignores.
- Risk: the next release reads a snapshot version, or the snapshot is built with the release version and refused by the registry.
- Fix: the rewrite step of the release action (`devops-release-package-in-python`), in its checkout only.

```bash
sed -i 's/^version = ".*"/version = "1.4.0.dev20261025120000"/' pyproject.toml   # in the build's checkout
python -m build                                                               # the package reports 1.4.0.dev20261025120000
```

# Check list
- [ ] `[project]` of the root `pyproject.toml` has a literal `version`, on its own line, and `version` is not in `dynamic`.
- [ ] No `__version__`, no `version.py`, and no other copy of the number exists in the source tree.
- [ ] The program reports `importlib.metadata.version("{package-name}")`.
