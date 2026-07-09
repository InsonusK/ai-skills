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
See the single-reference-page example at [examples/simple_skill/docs/api/reference.md](../examples/simple_skill/docs/api/reference.md) and the page-group examples at [examples/complex_skill/docs/api/](../examples/complex_skill/docs/api/) for fully worked applications of these rules. Do not re-derive a separate example here; extend those examples or add a new method section in the same format instead.
