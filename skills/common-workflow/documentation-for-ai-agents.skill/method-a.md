# Method A: `process_data`

Primary method that processes an input source and returns a list of records.

## Signature

```python
process_data(source: str, limit: int = 100) -> list[dict]
```

## Parameters

| Parameter | Type  | Required | Default | Description                          |
| --------- | ----- | -------- | ------- | ------------------------------------ |
| `source`  | `str` | yes      | —       | Path or URL of the input data.       |
| `limit`   | `int` | no       | `100`   | Maximum number of records to return. |

## Return value

- Type: `list[dict]`
- Each dictionary contains the processed fields from the input.

## Errors

- `FileNotFoundError` — `source` does not exist or is unreachable.
- `ValueError` — `limit` is negative or `source` has an unsupported format.

## Example

```python
from mylib.core import process_data

records = process_data(source="data/input.csv", limit=10)
print(records)
```

Expected output:

```python
[
    {"id": 1, "value": 42},
    {"id": 2, "value": 43},
]
```
