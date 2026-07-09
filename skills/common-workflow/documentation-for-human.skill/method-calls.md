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

## Worked examples
See [method-a.md](./method-a.md) (`process_data` — required parameter, list return) and [method-b.md](./method-b.md) (`fetch_records` — optional/nullable parameters, dict return with pagination) for two fully worked examples of these rules applied. Do not re-derive a separate example here; extend those two files or add a new one in the same format instead.
