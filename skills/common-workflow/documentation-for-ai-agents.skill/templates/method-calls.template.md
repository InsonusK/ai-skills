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

## Worked examples
See the single-skill example at [examples/simple_skill/mylib.skill/](../examples/simple_skill/mylib.skill/) (`process_data` — required parameter, list return; `fetch_records` — optional/nullable parameters, dict return with pagination) and the skill-group examples at [examples/complex_skill/](../examples/complex_skill/) for fully worked applications of these rules. Do not re-derive a separate example here; extend those examples or add a new method fragment in the same format instead.
