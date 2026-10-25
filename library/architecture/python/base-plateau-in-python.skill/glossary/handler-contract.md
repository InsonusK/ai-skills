# Handler contract

The **handler contract** (`command/pipeline/handler.py`) is the one shape every command handler has: `Handler[TCommand, TResult]`, a `typing.Protocol` with one method, `async def handle(self, command: TCommand) -> TResult`. Code that is not about one command — an entry point serving commands, a decorator wrapping them — is written once against it.

## Why it exists

Without one shape, every entry point and every cross-cutting step (logging, a unit of work, a metric) needs code per command. With it, `with_logging` wraps any handler and `startup.run` serves any command.

## How it works

| Part | Role |
|---|---|
| `Handler[TCommand, TResult]` | the protocol; `TCommand` is the input of one operation (`PlanItemsCommand`), `TResult` its result (`PlanItemsResult`). A class satisfies it by having the method — it names no base class |
| `HandlerFunc(function)` | turns an `async def` function into a handler: for decorators, which return one wrapping the next handler, and for stubs in tests |
| a decorator, `with_{step}(..., next_handler)` | returns a handler of the same `TCommand` and `TResult` that does its step around `next_handler.handle` |

The context of a call is the running asyncio task's: its cancellation and its `contextvars`, such as the request id.

```mermaid
flowchart LR
    E[entry point] -->|handle(command)| L[with_logging]
    L -->|handle(command)| H[PlanItemsHandler]
    H --> D[domain]
```

## Example

```python
# [handlers] in main.py - the variable is typed by the protocol, so a decorator can rewrap it
plan_items: Handler[PlanItemsCommand, PlanItemsResult] = PlanItemsHandler()
plan_items = with_logging("PlanItems", logger, plan_items)
```

```python
# a stub in a step module
async def fail(command: str) -> str:
    raise RuntimeError("store unavailable")

handler = HandlerFunc(fail)
```

## Related concepts

- [Entry runner](./entry-runner.md) — runs the entry points that call handlers.
- [Logging](./logging.md) — what `with_logging` writes.

## Sources

- `typing.Protocol`: https://docs.python.org/3/library/typing.html#typing.Protocol
- Type parameter syntax (PEP 695): https://docs.python.org/3/reference/compound_stmts.html#type-params
