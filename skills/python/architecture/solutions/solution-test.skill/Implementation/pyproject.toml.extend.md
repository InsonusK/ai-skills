---
description: Exclude test directories and test modules from the installed package
project_name: "{Project}"
name: "pyproject.toml"
element_kind: config
change_kind: extend
---

# Goals
- Prevent test directories and test modules from being included in the distributed package.
- Keep production and test code separate at build time.

# Core Principles
- Test code must not be installed alongside production code.
- Exclusion rules must cover both top-level `test/` directories and co-located `test/` directories.

# Implementation changes
Extend `pyproject.toml` with package discovery rules that exclude tests:

```toml
[build-system]
requires = ["setuptools>=61.0"]
build-backend = "setuptools.build_meta"

[tool.setuptools.packages.find]
where = ["src"]
exclude = [
    "test",
    "test.*",
    "*test*",
    "tests",
    "tests.*",
]
```

For projects that do not use a `src/` layout, adjust `where` accordingly:

```toml
[tool.setuptools.packages.find]
where = ["."]
exclude = [
    "test",
    "test.*",
    "*test*",
    "tests",
    "tests.*",
]
```

# Rules

## MUST
- Add explicit exclusion rules for test directories and test modules.
- Cover both `test/` and `tests/` naming conventions.
- Cover co-located `test/` directories via the `*test*` wildcard.

## SHOULD
- Use the `src/` layout to simplify package discovery rules.

## MUST NOT
- Rely on default discovery without exclusion rules when tests are co-located with source.

# Anti-patterns
- **Ship tests because no exclusion rule exists**
  - Consequence: package size increases and test dependencies may leak into production.
  - Instead: define `[tool.setuptools.packages.find]` with explicit exclusions.

# Check list
- [ ] `pyproject.toml` contains `[tool.setuptools.packages.find]`.
- [ ] `exclude` lists `test`, `test.*`, `*test*`, `tests`, and `tests.*`.
- [ ] A build dry-run confirms tests are not included in the wheel.
