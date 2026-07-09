---
description: Create unit tests for a source module inside a package
project_name: "{Project}"
name: "test/{Package}/{Module}_test.py"
element_kind: functions
change_kind: create
---

# Goals
- Provide unit tests for a source module located inside a package.
- Preserve the package hierarchy in the test directory.

# Core Principles
- Test directory structure mirrors source directory structure.
- One test module per source module.

# Structure

## Project Structure
```
/{Project}
  /src
    /{Package}
      __init__.py
      {Module}.py
  /test
    /{Package}
      __init__.py
      {Module}_test.py
```

## Directory and file roles
| Directory | File | Description |
| --------- | ---- | ----------- |
| /src/{Package} | {Module}.py | Source module under test. |
| /test/{Package} | {Module}_test.py | Tests for the source module. |

# Implementation changes
```python
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

# Rules

## MUST
- Place the file at `test/{Package}/{Module}_test.py` when the source is at `src/{Package}/{Module}.py`.
- Create `test/{Package}/__init__.py` so the test package mirrors the source package.
- Import the module under test from the `src.{Package}` package.

## SHOULD
- Keep the same relative depth between `src/` and `test/`.

## MUST NOT
- Flatten package structure inside `test/`.

# Anti-patterns
- **Place the test file directly under `test/` ignoring the package path**
  - Consequence: naming collisions and lost navigational mapping.
  - Instead: recreate the full `src/` hierarchy under `test/`.

# Check list
- [ ] `test/{Package}/{Module}_test.py` exists.
- [ ] `test/{Package}/__init__.py` exists.
- [ ] The test path mirrors the source path `src/{Package}/{Module}.py`.
