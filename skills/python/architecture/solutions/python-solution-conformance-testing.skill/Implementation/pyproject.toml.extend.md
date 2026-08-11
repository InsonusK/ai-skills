---
description: Declare behave, coverage, and mutmut as dev dependencies and configure coverage to include the features/ directory
project_name: "{Package}"
name: "pyproject.toml"
element_kind: project
change_kind: extend
---

# Goals
- Make `behave`, `coverage`, and `mutmut` available to CI and local development.
- Make `coverage` measure both `test/` and `features/` in one combined report.

# Core Principles
- Dev-only tooling stays under `[project.optional-dependencies].dev` (or the project's existing dev-dependency group); it is never a runtime dependency of the package.

# Implementation changes
```code example
[project.optional-dependencies]
dev = [
    "behave",
    "coverage",
    "mutmut",
]

[tool.coverage.run]
source = ["{package}"]
branch = true

[tool.coverage.report]
fail_under = 80
```

# Rule changes

## MUST
- Add `behave`, `coverage`, `mutmut` to the dev dependency group, not to the package's runtime dependencies.
  - Violation: adding `behave`/`mutmut` as runtime dependencies.
  - Risk: end users installing the package pull in test-only tooling.
  - Fix: keep them under the dev optional-dependency group only.
- Configure `[tool.coverage.run] source` to point at the package, so both `test/` and `features/` runs contribute to the same coverage data.
  - Risk: if `source` doesn't cover the package, coverage numbers silently exclude part of what the gate is supposed to measure.
  - Fix: point `[tool.coverage.run] source` at the package under test.

# Check list
- [ ] `behave`, `coverage`, `mutmut` are listed under the dev dependency group.
- [ ] `[tool.coverage.run] source` covers the package under test.
