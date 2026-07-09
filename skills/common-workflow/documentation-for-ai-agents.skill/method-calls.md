# Method calls: general requirements

These requirements apply to every function, command, endpoint, or class method that another AI agent may call.

## MUST
- Provide the exact call signature or request shape.
  - Function: `def process_data(source: str, limit: int = 100) -> list[dict]`.
  - CLI: `mycli process --source <path> [--limit 100]`.
  - HTTP: `POST /api/v1/process { "source": "...", "limit": 100 }`.
- List every parameter:
  - name,
  - type,
  - whether it is required or optional,
  - default value for optional parameters,
  - valid value range or allowed values.
- Describe the return value or response:
  - type,
  - shape or schema,
  - example value.
- Describe errors and failure modes:
  - exception types or HTTP status codes,
  - what causes each error,
  - how the agent should handle or retry.
- Provide at least one minimal working example that can be executed after installation.

## SHOULD
- Show the example input and the expected output side by side.
- Mention performance or rate limits if they affect how the agent calls the method.

## MUST NOT
- Describe behavior without showing the exact call.
- Skip parameter types or default values.
- Skip error handling.

## Example format

```python
process_data(source: str, limit: int = 100) -> list[dict]
```

| Parameter | Type     | Required | Default | Description                          |
| --------- | -------- | -------- | ------- | ------------------------------------ |
| `source`  | `str`    | yes      | —       | Path or URL of the input data.       |
| `limit`   | `int`    | no       | `100`   | Maximum number of records to return. |

**Returns:** `list[dict]` — list of processed records.

**Errors:**
- `FileNotFoundError` — `source` does not exist.
- `ValueError` — `limit` is negative.
