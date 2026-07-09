# API reference

## `process_data(source, limit=100)`

Use this function to clean and transform raw input data into a structured list of records.

### Parameters

- `source` (`str`, required): path or URL of the input data.
- `limit` (`int`, optional): maximum number of records to return. Defaults to `100`.

### Returns

A list of dictionaries. Each dictionary represents one processed record.

### Example

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

### Common errors

- `FileNotFoundError`: the file at `source` does not exist.
- `ValueError`: `limit` is negative or the file format is unsupported.

## `fetch_records(status=None, offset=0, page_size=50)`

Use this function to retrieve stored records, optionally filtered by status and paginated.

### Parameters

- `status` (`str | None`, optional): filter by status. Allowed values: `"active"`, `"archived"`. Defaults to no filter.
- `offset` (`int`, optional): number of records to skip. Defaults to `0`.
- `page_size` (`int`, optional): records per page, from `1` to `100`. Defaults to `50`.

### Returns

A dictionary with three keys:

- `total` (`int`): total number of matching records.
- `records` (`list[dict]`): the current page of records.
- `next_offset` (`int | None`): offset for the next page, or `None` if this is the last page.

### Example

```python
from mylib.core import fetch_records

result = fetch_records(status="active", offset=0, page_size=10)
print(result)
```

Expected output:

```python
{
    "total": 2,
    "records": [
        {"id": 1, "status": "active"},
        {"id": 2, "status": "active"},
    ],
    "next_offset": None,
}
```

### Common errors

- `ValueError`: `status` is not `"active"` or `"archived"`.
- `ValueError`: `offset` or `page_size` is out of range.
- `ConnectionError`: the data store is unreachable.
