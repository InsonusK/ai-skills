# Method A: `process_data`

Use this function when you need to clean and transform raw input data into a structured list of records.

## Signature

```python
process_data(source: str, limit: int = 100) -> list[dict]
```

## When to use it

- Convert a CSV, JSON, or remote feed into normalized records.
- Preview the first N records before running a full pipeline.

## Parameters

- `source` (`str`, required): path or URL of the input data.
- `limit` (`int`, optional): maximum number of records to return. Defaults to `100`.

## Returns

A list of dictionaries. Each dictionary represents one processed record.

## Example

```python
from mylib.core import process_data

records = process_data("data/input.csv", limit=10)
print(records)
```

Expected output:

```python
[
    {"id": 1, "value": 42},
    {"id": 2, "value": 43},
]
```

## Common errors

- `FileNotFoundError`: the file at `source` does not exist.
- `ValueError`: `limit` is negative or the file format is unsupported.

## See also

- [method-b.md](./method-b.md) for retrieving already-stored records.
