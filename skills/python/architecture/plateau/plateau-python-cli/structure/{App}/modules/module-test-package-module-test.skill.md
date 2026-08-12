---
name: module-test-package-module-test
description: Create unit tests for a source module inside a package
domain: skill
type: template
plateau: plateau-python-cli
version: 20260710003814
tags:
  - skill/template/module
  - plateau/plateau-python-cli
  - stack/python
  - concern/architecture

created_by:
  - "[[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]]"
---

# Goal
- Provide unit tests for a source module located inside a package.
- Preserve the package hierarchy in the test directory.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/test.{Package}.{Module}_test.py.create.md|test.{Package}.{Module}_test.py.create]]

# Core Principles
- Apply ONE plateau template per class/module.
- Test directory structure mirrors source directory structure.
- One test module per source module.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/test.{Package}.{Module}_test.py.create.md|test.{Package}.{Module}_test.py.create]]

# Naming convention

| use case | element name pattern | element name | file name pattern | file name |
| -------- | -------------------- | ------------ | ----------------- | --------- |
| Package test module | `{Module}TestCase` | ProcessTestCase | test/{package}/{module}_test.py | test/core/process_test.py |

# Implementation

```python
# Skill: module-test-package-module-test
# Plateau: plateau-python-cli
# Version: 20260710003814

import unittest

from src.{Package}.{Module} import process


class {ModuleTitle}TestCase(unittest.TestCase):
    def test_process_with_valid_input_returns_expected_result(self) -> None:
        result = process("valid")
        self.assertEqual(result, "processed-valid")

    def test_process_with_empty_input_raises_value_error(self) -> None:
        with self.assertRaises(ValueError):
            process("")


if __name__ == "__main__":
    unittest.main()
```

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/test.{Package}.{Module}_test.py.create.md|test.{Package}.{Module}_test.py.create]]

# Rules

## MUST
- Place the file at `test/{Package}/{Module}_test.py` when the source is at `src/{Package}/{Module}.py`.
- Create `test/{Package}/__init__.py` so the test package mirrors the source package.
- Import the module under test from the `src.{Package}` package.

## SHOULD
- Keep the same relative depth between `src/` and `test/`.

## MUST NOT
- Flatten package structure inside `test/`.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/test.{Package}.{Module}_test.py.create.md|test.{Package}.{Module}_test.py.create]]

# Anti-patterns

- **Apply SEVERAL plateau template per class/module**
  - Consequence: conflicting responsibilities and inconsistent generated code.
  - Instead: apply exactly one plateau module template per file.
- **Place the test file directly under `test/` ignoring the package path**
  - Consequence: naming collisions and lost navigational mapping.
  - Instead: recreate the full `src/` hierarchy under `test/`.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/test.{Package}.{Module}_test.py.create.md|test.{Package}.{Module}_test.py.create]]

# Check list

- [ ] `test/{Package}/{Module}_test.py` exists.
- [ ] `test/{Package}/__init__.py` exists.
- [ ] The test path mirrors the source path `src/{Package}/{Module}.py`.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/test.{Package}.{Module}_test.py.create.md|test.{Package}.{Module}_test.py.create]]

# Unittest TestCases

- [ ] WHEN the tested function receives valid input THEN it returns the expected result.
- [ ] WHEN the tested function receives invalid input THEN it raises the expected exception or returns the expected error value.

__Applied solutions:__
- [[skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md|solution-test]] - [[skills/python/architecture/solutions/solution-test.skill/Implementation/test.{Package}.{Module}_test.py.create.md|test.{Package}.{Module}_test.py.create]]
