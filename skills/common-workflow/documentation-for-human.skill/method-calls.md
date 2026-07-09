# Method calls: general requirements for humans

These requirements apply to every function, command, endpoint, or class method documented for a human reader.

## Location
- Place high-level usage in `README.md` or `docs/usage.md`.
- Place detailed reference for each method in `docs/api/<method-name>.md` or a single `docs/api/reference.md`.

## MUST
- Explain what the method does and when to use it.
- Provide the signature or command in a copy-pasteable format.
- Describe each parameter in plain language:
  - name,
  - type,
  - whether it is required,
  - default value if optional,
  - what value the reader should pass.
- Describe the return value or response and what it represents.
- Describe common errors and how to fix them.
- Provide at least one complete, runnable example.

## SHOULD
- Show the example input, the command, and the expected output.
- Include a concrete use case that explains why the reader would call the method.

## MUST NOT
- List only types and signatures without explanation.
- Skip error handling or edge cases that humans are likely to hit.

## Example format

```markdown
## `process_data(source, limit=100)`

Use this function to clean and transform raw input data into a list of records.

### Parameters

- `source` (str, required): path or URL of the raw input file.
- `limit` (int, optional): maximum number of records to return. Defaults to 100.

### Returns

A list of dictionaries, one per processed record.

### Example

```python
from mylib.core import process_data

records = process_data("data/input.csv", limit=10)
print(records)
```

Output:

```python
[
    {"id": 1, "value": 42},
    {"id": 2, "value": 43},
]
```

### Common errors

- `FileNotFoundError`: check that `source` points to an existing file.
- `ValueError`: `limit` must be zero or positive.
```
