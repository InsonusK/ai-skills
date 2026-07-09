---
name: test-location
description: Decide where to place test files in Python projects
problem: Should tests be placed in a top-level test directory mirroring src, or co-located next to the tested code?
decision: Use a top-level test directory mirroring src by default; allow co-located test directories for large projects when tests are excluded from packaging.
---

# Problem
Decide where and how to place test files in Python projects so that they are easy to find and maintain, while ensuring they are not included in installed packages.

# Selected variant
**Selected variant:** [[#Top-level test directory mirroring src structure]]
- Default approach for all Python projects.
- Co-located tests are allowed only when explicitly needed and properly excluded from packaging.

# Searched variants

## Top-level test directory mirroring src structure (selected)

### Description
Keep a top-level `test/` directory whose internal structure mirrors the `src/` directory. Test modules are named `{module}_test.py`.

Example:
```
/src
  /service
    /a_service
      service.py
/test
  /service
    /a_service
      service_test.py
```

### Benefits
- Easy to locate tests for any source module.
- Simple CI configuration and test discovery.
- No risk of packaging tests with the application.
- Works with standard `unittest` and `pytest` discovery.
- Clear separation between production and test code.

### Costs
- Developers switch between `src/` and `test/` directories while editing.
- Very large projects may experience navigation overhead.

## Co-located test directory next to tested module

### Description
Place a `test/` directory next to the module or package being tested, e.g. `src/service/a_service/test/service_test.py`.

### Benefits
- Source and tests are visible together during development.
- Reduces navigation overhead in large projects.
- Makes ownership of tests obvious.

### Costs
- Requires explicit packaging exclusion to avoid installing tests.
- Slightly more complex CI discovery patterns.
- Can clutter the source tree.
- Relative imports or path adjustments may be needed.

# Decision rationale
The top-level mirrored layout is the safest default because it keeps production and test code separate, works out of the box with standard tooling, and avoids accidental packaging of tests. Co-location is a valid optimization for large projects where the development workflow benefits outweigh the packaging complexity, but it must be paired with explicit exclusion rules in `pyproject.toml`.
