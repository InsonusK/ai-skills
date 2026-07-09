---
name: solution-test
description: Define how to organize and write tests in Python projects so that test structure mirrors source structure
domain: python
type: architecture
version: 
tags:
  - skill/architecture/solution
  - python
  - testing
  - architecture
triggers:
  - Set up tests for a new Python project
  - Add tests for a new or existing Python module
  - Refactor a disorganized test suite
creates:
  - "test/{Module}_test.py"
  - "test/{Package}/{Module}_test.py"
  - "src/{Package}/{Module}/test/{Module}_test.py"
extends:
  - "pyproject.toml"
depends_on:
adr:
  - "[[./adr/test-location.md|Test location decision]]"
---

# Goal
- Define a consistent approach to organizing tests in Python projects.
- Keep test structure aligned with source structure so tests are easy to find and maintain.
- Ensure tests are excluded from installed packages.

# Capabilities
- Tests are located predictably relative to the code they verify.
- Developers can navigate between source and tests without searching.
- Tests are excluded from installed packages.
- Large projects can optionally keep tests next to the tested code while still avoiding packaging side effects.

# Core Principles
- **Test structure mirrors source structure**: every source module has a corresponding test module in the same relative path.
- **One test module per source module**: do not mix tests for multiple modules in one file.
- **Test files use the `_test.py` suffix**: for a source module `{Module}.py`, the test module is `{Module}_test.py`.
- **Tests are not part of the installed package**: build configuration must exclude test directories and test modules from distribution.

# Adr
- [[./adr/test-location.md|Test location decision]]
  - Default: mirror source structure in a top-level `test/` directory.
  - Alternative allowed for large projects: co-located `test/` directory next to the tested module, with explicit packaging exclusion.

# Requirements
PYTHON STANDARD LIBRARY:
- `unittest`
  - Default test runner supported by the structure.

PYPI:
- `pytest` (optional)
  - Can be used instead of `unittest` without changing the directory layout.

BUILD TOOL CONFIGURATION:
- `setuptools` via `pyproject.toml`
  - `[tool.setuptools.packages.find]` must exclude test directories and test modules from the package.

# Template Skill Mutations
PROJECT:
- [[./Implementation/test.{Module}_test.py.create.md|test/{Module}_test.py]] - create - unit tests for a root-level source module
- [[./Implementation/test.{Package}.{Module}_test.py.create.md|test/{Package}/{Module}_test.py]] - create - unit tests for a source module inside a package
- [[./Implementation/src.{Package}.{Module}.test.{Module}_test.py.create.md|src/{Package}/{Module}/test/{Module}_test.py]] - create - co-located unit tests for a module in a large project
- [[./Implementation/pyproject.toml.extend.md|pyproject.toml]] - extend - exclude test directories and test modules from the installed package

# Workflow
## Add tests for a new source module
1. Locate the source module under `src/{Package}/{Module}.py`.
2. Create the matching test path under `test/{Package}/{Module}_test.py`.
3. Import the module under test and write focused unit tests.
4. Run the test module with `python -m unittest test.{Package}.{Module}_test` or `pytest test/{Package}/{Module}_test.py`.

## Use co-located tests in a large project
1. Confirm the project is large enough to benefit from co-location.
2. Create `test/` next to the tested module, e.g. `src/{Package}/{Module}/test/{Module}_test.py`.
3. Update `pyproject.toml` to exclude `*test*` directories from package discovery.
4. Run tests with the chosen runner.

# Rules
## MUST
- Mirror source structure in the `test/` directory.
- Name test files with the `_test.py` suffix.
- Exclude test directories and test modules from the installed package via build configuration.
- Write one test module per source module.

## SHOULD
- Use the top-level `test/` directory as the default layout.
- Use co-located `test/` directories only in large projects where navigation overhead is significant.

## SHOULD NOT
- Mix tests for multiple source modules in a single test file.
- Place test logic inside the source package.

## MUST NOT
- Include test files or test directories in the installed package.
- Put all tests in a single flat `tests/` directory without structure.

# Anti-patterns
- **Put all tests in a flat `tests/` directory**
  - Consequence: it becomes hard to understand which test covers which module; developers must read each test to find the matching source.
  - Instead: mirror the source structure inside `test/` so every source module has a predictable test location.
- **Name test files without the `_test.py` suffix**
  - Consequence: test runners may not discover the files automatically.
  - Instead: always use `{Module}_test.py`.
- **Ship tests as part of the installed package**
  - Consequence: installed package size grows and test code may run in production environments.
  - Instead: configure `pyproject.toml` to exclude test directories and modules from package discovery.
- **Co-locate tests without excluding them from packaging**
  - Consequence: tests are installed alongside production code.
  - Instead: add explicit exclusion rules before adopting co-located tests.

# Check list
- [ ] Test directory structure mirrors source structure.
- [ ] Each source module has a corresponding test module.
- [ ] Test files use the `_test.py` suffix.
- [ ] Tests are excluded from the installed package via `pyproject.toml`.
- [ ] No tests are placed in a single flat directory without structure.
