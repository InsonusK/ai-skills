---
name: test-location
description: Where a Python project keeps its feature files and its tests - beside the code or in root-level trees
problem: Python projects kept tests in a top-level test/ tree mirroring src/ and features in a root features/ tree, while the Go catalog keeps both beside the code; the root layout was chosen for its simpler packaging and discovery, without measuring what co-location costs
decision: features/ and test/ folders inside the package they belong to; the settings this needs are five lines of pyproject.toml
tags:
  - stack/python
  - concern/testing
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Decide where a Python project keeps `.feature` files and test modules. The former decision (the removed `solution-test-layout-in-python`) selected a top-level `test/` tree mirroring `src/`, with features in a root `features/` tree, and named four costs of co-location: explicit packaging exclusion, more complex CI discovery, clutter, and path adjustments. On 2026-10-08 those costs were measured on the example of `solution-conformance-testing-in-python`, to decide whether Python can follow the convention the Go catalog already has.

# Selected variant
**Selected variant:** [[#Features and tests beside the code]]

# Searched variants

## Features and tests beside the code

**Selected.**

### Description
A package holds its own `features/{rule}.feature` and `test/{rule}_steps_test.py`; a plain test is `test/{module}_test.py`. `test/` has no `__init__.py`.

### Benefits
- The specification is one folder away from the code, and a failing scenario names its package.
- One convention across stacks: the Go catalog keeps `{package}/features/` and `{package}/test/` the same way.
- The test kinds, the workflows and `make test-kind-*` do not change: they call `pytest` through `pyproject.toml`.

### Costs
Measured on the example; each is one line of `pyproject.toml`, delivered by `solution-conformance-testing-in-python`:
- Packaging: with `exclude = ["test*"]` both test modules were in the wheel. `exclude = ["*.test", "*.test.*"]` leaves `__init__.py` and the module only. A forgotten exclusion is silent — nothing fails, the tests ship.
- Collection: `testpaths` names the source root. Two packages with a test module of the same name fail collection ("import file mismatch") until `addopts = "--import-mode=importlib"`.
- Mutation: `mutmut` mutated the test module too — 6 mutants with no test, the score fell from 93.5% to 78.4% — until `do_not_mutate = ["*/test/*"]`.
- Coverage: unaffected while `test/` has no `__init__.py`; `omit = ["*/test/*"]` guards it.
- A CI path filter that separates code from test changes needs negated patterns, since tests now sit below the source root.

## Top-level test directory mirroring src, root features directory

### Description
`test/` at the repository root mirrors `src/`; `.feature` files and step modules sit in a root `features/` tree.

### Benefits
- Nothing of the tests is below the source root, so packaging, coverage and mutation need no exclusion.
- A path filter tells code from tests by the top-level folder.

### Costs
- The specification of a module is in another tree; the reader mirrors the path by hand.
- Python differs from the Go catalog, which forbids a root `features/` tree.
- The recommended exclusion `"*test*"` removed any production package with `test` in its name from the wheel — measured with a package named `latest`.
