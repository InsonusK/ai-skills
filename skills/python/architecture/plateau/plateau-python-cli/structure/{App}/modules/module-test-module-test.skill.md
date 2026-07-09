---
name: module-test-module-test
description: Create unit tests for a root-level source module
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
- Provide unit tests for a single root-level source module.
- Keep the test module aligned with the source module location.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/test.{Module}_test.py.create.md|test.{Module}_test.py.create]]

# Core Principles
- Apply ONE plateau template per class/module.
- One test module per source module.
- Test names describe behavior under test.
- Tests use the standard `unittest` module unless the project has chosen `pytest`.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/test.{Module}_test.py.create.md|test.{Module}_test.py.create]]

# Naming convention

| use case | element name pattern | element name | file name pattern | file name |
| -------- | -------------------- | ------------ | ----------------- | --------- |
| Root-level test module | `{Module}TestCase` | AddTestCase | test/{module}_test.py | test/add_test.py |

# Implementation

```python
# Skill: module-test-module-test
# Plateau: plateau-python-cli
# Version: 20260710003814

import unittest

from src.{Module} import add


class {ModuleTitle}TestCase(unittest.TestCase):
    def test_add_with_positive_numbers_returns_sum(self) -> None:
        result = add(1, 2)
        self.assertEqual(result, 3)

    def test_add_with_zero_returns_other_operand(self) -> None:
        result = add(5, 0)
        self.assertEqual(result, 5)


if __name__ == "__main__":
    unittest.main()
```

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/test.{Module}_test.py.create.md|test.{Module}_test.py.create]]

# Rules

## MUST
- Place the file at `test/{Module}_test.py` when the source is at `src/{Module}.py`.
- Import the module under test from the `src` package.
- Name the test class `{ModuleTitle}TestCase`.

## SHOULD
- Keep tests focused on the public API of the module.

## MUST NOT
- Import from the test module in production code.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/test.{Module}_test.py.create.md|test.{Module}_test.py.create]]

# Anti-patterns

- **Apply SEVERAL plateau template per class/module**
  - Consequence: conflicting responsibilities and inconsistent generated code.
  - Instead: apply exactly one plateau module template per file.
- **Put tests for multiple modules in one file**
  - Consequence: the file grows and becomes hard to navigate.
  - Instead: create one `{Module}_test.py` per source module.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/test.{Module}_test.py.create.md|test.{Module}_test.py.create]]

# Check list

- [ ] `test/{Module}_test.py` exists.
- [ ] The test module imports `src.{Module}`.
- [ ] Test methods cover the public functions of the source module.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/test.{Module}_test.py.create.md|test.{Module}_test.py.create]]

# Unittest TestCases

- [ ] WHEN the tested function receives valid input THEN it returns the expected result.
- [ ] WHEN the tested function receives invalid input THEN it raises the expected exception or returns the expected error value.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/test.{Module}_test.py.create.md|test.{Module}_test.py.create]]
