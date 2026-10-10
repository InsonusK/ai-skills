---
name: remove-solution-test-layout-in-python
description: Why plateau-python-cli no longer composes a test-layout solution and has no test structure skills
problem: The plateau composed solution-test-layout-in-python - a top-level test/ tree mirroring the sources - and carried three structure skills for test modules; that solution was removed when Python moved to features and tests beside the code, defined by the testing skills
decision: Remove the solution's content and the three test module skills; the plateau names the testing skills instead of restating them
tags:
  - plateau/plateau-python-cli
  - stack/python
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`solution-test-layout-in-python` was deleted on 2026-10-08: where Python features and tests live is now a rule of `cucumber-testing-in-python` (beside the code: `{package}/features/`, `{package}/test/`), and the settings that keep those tests out of the installed package come from `solution-conformance-testing-in-python`. The plateau had the removed solution in `created_by`, its principles, rules and check-list lines in the root and `package-app` skills, and three structure skills generated from its `Implementation/` files. Something has to replace or drop them.

# Selected variant
**Selected variant:** [[#Name the testing skills, keep no test structure in the plateau]]

# Searched variants

## Name the testing skills, keep no test structure in the plateau

**Selected.**

### Description
Strip everything that came only from the removed solution; delete `module-test-module-test`, `module-test-package-module-test` and `module-src-package-module-test-module-test`; add a `# Testing` section to the root skill that names `cucumber-testing-in-python` and `solution-conformance-testing-in-python`. `package-app`'s `pyproject.toml` sample keeps one exclusion line, from `solution-cli-packaging`: `exclude = ["*.test", "*.test.*"]`. The plateau has no child plateau, so nothing propagates further.

### Benefits
- One place says how a Python program is tested; the plateau cannot drift from it.
- The plateau stays about the CLI's layers and packaging.

### Costs
- A reader of the plateau alone does not see the test layout; the `# Testing` section sends them to the testing skills.

## Compose solution-conformance-testing-in-python into the plateau

### Description
Put the testing solution into `created_by` and generate structure skills for its features, step modules and kind scripts.

### Benefits
- The plateau's structure tree shows the tests.

### Costs
- The plateau restates the testing skills, and every change to them has to be propagated through `plateau-update-by-solutions`.
- A project built from the plateau would take the whole test tooling whether or not it applies the testing skills itself.
