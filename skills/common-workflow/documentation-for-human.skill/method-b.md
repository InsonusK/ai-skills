# Method B: `fetch_records`

Use this function when you need to retrieve stored records, optionally filtered by status and paginated.

## Signature

```python
fetch_records(status: str | None = None, offset: int = 0, page_size: int = 50) -> dict
```

## When to use it

- List active or archived records from the data store.
- Paginate through large result sets.

## Parameters

- `status` (`str\|None`, optional): filter by status. Allowed values: `"active"`, `"archived"`. Defaults to no filter.
- `offset` (`int`, optional): number of records to skip. Defaults to `0`.
- `page_size` (`int`, optional): records per page, from `1` to `100`. Defaults to `50`.

## Returns

A dictionary with three keys:

- `total` (`int`): total number of matching records.
- `records` (`list[dict]`): the current page of records.
- `next_offset` (`int\|None`): offset for the next page, or `None` if this is the last page.

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

## Common errors

- `ValueError`: `status` is not `"active"` or `"archived"`.
- `ValueError`: `offset` or `page_size` is out of range.
- `ConnectionError`: the data store is unreachable.

## See also

- [method-a.md](./method-a.md) for processing raw input data.
