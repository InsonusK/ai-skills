---
name: module-src-package-module-test-module-test
description: Create co-located unit tests for a module in a large project
domain: skill
type: template
plateau: plateau-python-cli
version: 20260710003814
tags:
  - skill/template/module
  - plateau/plateau-python-cli
created_by:
  - "[[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]]"
---

# Goal
- Keep tests next to the source module in large projects to reduce navigation overhead.
- Ensure co-located tests are still excluded from the installed package.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/src.{Package}.{Module}.test.{Module}_test.py.create.md|src.{Package}.{Module}.test.{Module}_test.py.create]]

# Core Principles
- Apply ONE plateau template per class/module.
- Co-location is allowed only when tests are explicitly excluded from packaging.
- The same one-test-module-per-source-module rule applies.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/src.{Package}.{Module}.test.{Module}_test.py.create.md|src.{Package}.{Module}.test.{Module}_test.py.create]]

# Naming convention

| use case | element name pattern | element name | file name pattern | file name |
| -------- | -------------------- | ------------ | ----------------- | --------- |
| Co-located test module | `{Module}TestCase` | ComputeTestCase | src/{package}/{module}/test/{module}_test.py | src/core/compute/test/compute_test.py |

# Implementation

```python
# Skill: module-src-package-module-test-module-test
# Plateau: plateau-python-cli
# Version: 20260710003814

import unittest

from ..{Module} import compute


class {ModuleTitle}TestCase(unittest.TestCase):
    def test_compute_with_valid_input_returns_result(self) -> None:
        result = compute(4)
        self.assertEqual(result, 16)

    def test_compute_with_negative_input_raises_value_error(self) -> None:
        with self.assertRaises(ValueError):
            compute(-1)


if __name__ == "__main__":
    unittest.main()
```

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/src.{Package}.{Module}.test.{Module}_test.py.create.md|src.{Package}.{Module}.test.{Module}_test.py.create]]

# Rules

## MUST
- Place the file at `src/{Package}/{Module}/test/{Module}_test.py`.
- Create `src/{Package}/{Module}/test/__init__.py`.
- Update `pyproject.toml` to exclude `*test*` directories from package discovery.
- Use relative imports or adjust `PYTHONPATH` so the test can import the module under test.

## SHOULD
- Use this layout only in large projects where the navigation benefit outweighs the packaging complexity.

## MUST NOT
- Adopt co-located tests without excluding them from the installed package.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/src.{Package}.{Module}.test.{Module}_test.py.create.md|src.{Package}.{Module}.test.{Module}_test.py.create]]

# Anti-patterns

- **Apply SEVERAL plateau template per class/module**
  - Consequence: conflicting responsibilities and inconsistent generated code.
  - Instead: apply exactly one plateau module template per file.
- **Co-locate tests and forget packaging exclusion**
  - Consequence: test code is installed alongside production code.
  - Instead: add `exclude = ["*test*"]` to `[tool.setuptools.packages.find]` in `pyproject.toml`.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/src.{Package}.{Module}.test.{Module}_test.py.create.md|src.{Package}.{Module}.test.{Module}_test.py.create]]

# Check list

- [ ] `src/{Package}/{Module}/test/{Module}_test.py` exists.
- [ ] `src/{Package}/{Module}/test/__init__.py` exists.
- [ ] `pyproject.toml` excludes `*test*` from package discovery.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/src.{Package}.{Module}.test.{Module}_test.py.create.md|src.{Package}.{Module}.test.{Module}_test.py.create]]

# Unittest TestCases

- [ ] WHEN the tested function receives valid input THEN it returns the expected result.
- [ ] WHEN the tested function receives invalid input THEN it raises the expected exception or returns the expected error value.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/src.{Package}.{Module}.test.{Module}_test.py.create.md|src.{Package}.{Module}.test.{Module}_test.py.create]]
