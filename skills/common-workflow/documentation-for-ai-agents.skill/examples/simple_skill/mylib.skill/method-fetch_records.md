# `fetch_records`

Secondary method that retrieves stored records by filter criteria.

## Signature

```python
fetch_records(status: str | None = None, offset: int = 0, page_size: int = 50) -> dict
```

## Parameters

| Parameter   | Type          | Required | Default | Description                                |
| ----------- | ------------- | -------- | ------- | ------------------------------------------ |
| `status`    | `str \| None` | no       | `None`  | Filter by record status (`active`, `archived`). |
| `offset`    | `int`         | no       | `0`     | Number of records to skip.                 |
| `page_size` | `int`         | no       | `50`    | Maximum number of records per page (1–100). |

## Return value

- Type: `dict`
- Contains `total` (int), `records` (list[dict]), and `next_offset` (int or None).

## Errors

- `ValueError` — `status` is not one of the allowed values.
- `ValueError` — `offset` or `page_size` is negative, or `page_size` exceeds 100.
- `ConnectionError` — the records store is unreachable.

## Example

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
