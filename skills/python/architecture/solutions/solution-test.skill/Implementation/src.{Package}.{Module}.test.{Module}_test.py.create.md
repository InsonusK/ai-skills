---
description: Create co-located unit tests for a module in a large project
project_name: "{Project}"
name: "src/{Package}/{Module}/test/{Module}_test.py"
element_kind: functions
change_kind: create
---

# Goals
- Keep tests next to the source module in large projects to reduce navigation overhead.
- Ensure co-located tests are still excluded from the installed package.

# Core Principles
- Co-location is allowed only when tests are explicitly excluded from packaging.
- The same one-test-module-per-source-module rule applies.

# Structure

## Project Structure
```
/{Project}
  /src
    /{Package}
      /{Module}
        __init__.py
        {Module}.py
        /test
          __init__.py
          {Module}_test.py
```

## Directory and file roles
| Directory | File | Description |
| --------- | ---- | ----------- |
| /src/{Package}/{Module} | {Module}.py | Source module under test. |
| /src/{Package}/{Module}/test | {Module}_test.py | Co-located tests for the module. |

# Implementation changes
```python
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

# Anti-patterns
- **Co-locate tests and forget packaging exclusion**
  - Consequence: test code is installed alongside production code.
  - Instead: add `exclude = ["*test*"]` to `[tool.setuptools.packages.find]` in `pyproject.toml`.

# Check list
- [ ] `src/{Package}/{Module}/test/{Module}_test.py` exists.
- [ ] `src/{Package}/{Module}/test/__init__.py` exists.
- [ ] `pyproject.toml` excludes `*test*` from package discovery.
