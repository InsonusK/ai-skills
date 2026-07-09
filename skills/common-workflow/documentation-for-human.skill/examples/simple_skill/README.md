# mylib

A small Python library for processing raw data into records and retrieving stored records.

## Why

Use mylib when you need a lightweight way to clean local data files and query the resulting records without setting up a database.

## Installation

Requires Python 3.10 or later.

```bash
pip install mylib==1.2.3
```

See [docs/installation.md](docs/installation.md) for platform-specific notes.

## Quick start

```python
from mylib.core import process_data, fetch_records

records = process_data("data/input.csv", limit=10)
print(records)

result = fetch_records(status="active", page_size=5)
print(result)
```

## Documentation

- [API reference](docs/api/reference.md) — all public methods.
