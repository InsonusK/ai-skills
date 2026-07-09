---
description: Create unit tests for a root-level source module
project_name: "{Project}"
name: "test/{Module}_test.py"
element_kind: functions
change_kind: create
---

# Goals
- Provide unit tests for a single root-level source module.
- Keep the test module aligned with the source module location.

# Core Principles
- One test module per source module.
- Test names describe behavior under test.
- Tests use the standard `unittest` module unless the project has chosen `pytest`.

# Structure

## Project Structure
```
/{Project}
  /src
    {Module}.py
  /test
    {Module}_test.py
```

## Directory and file roles
| Directory | File | Description |
| --------- | ---- | ----------- |
| /src | {Module}.py | Source module under test. |
| /test | {Module}_test.py | Tests for the root-level source module. |

# Implementation changes
```python
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

# Rules

## MUST
- Place the file at `test/{Module}_test.py` when the source is at `src/{Module}.py`.
- Import the module under test from the `src` package.
- Name the test class `{ModuleTitle}TestCase`.

## SHOULD
- Keep tests focused on the public API of the module.

## MUST NOT
- Import from the test module in production code.

# Anti-patterns
- **Put tests for multiple modules in one file**
  - Consequence: the file grows and becomes hard to navigate.
  - Instead: create one `{Module}_test.py` per source module.

# Check list
- [ ] `test/{Module}_test.py` exists.
- [ ] The test module imports `src.{Module}`.
- [ ] Test methods cover the public functions of the source module.
