# Handler contract

The **handler contract** (`command/pipeline/handler.py`) is the one shape every command handler has: `Handler[TCommand, TResult]`, a `typing.Protocol` with one method, `async def handle(self, command: TCommand) -> TResult`. Code that is not about one command — an entry point serving commands, a decorator wrapping them — is written once against it.

## Why it exists

Without one shape, every entry point and every cross-cutting step (logging, a unit of work, a metric) needs code per command. With it, `decorate` wraps any handler with the decorators and `startup.run` serves any command.

## How it works

| Part | Role |
|---|---|
| `Handler[TCommand, TResult]` | the protocol; `TCommand` is the input of one operation (`PlanItemsCommand`), `TResult` its result (`PlanItemsResult`). A class satisfies it by having the method — it names no base class |
| `HandlerFunc(function)` | turns an `async def` function into a handler: for `decorate`, which returns one wrapping the chain of decorators, and for stubs in tests |
| a decorator, `with_{step}(...)` → `Decorator` | an async function `(name, command, call_next) -> result` that does its step around `call_next`; not generic, so decorators stand in one list |
| `decorate(name, handler, decorators)` | wraps a typed handler with the list, the first the outermost, and returns a handler of the same `TCommand` and `TResult` |

The context of a call is the running asyncio task's: its cancellation and its `contextvars`, such as the request id.

```mermaid
flowchart LR
    E[entry point] -->|handle(command)| D0[decorate]
    D0 -->|name, command, call_next| L[with_logging]
    L -->|call_next(command)| H[PlanItemsHandler]
    H --> D[domain]
```

## Example

```python
# command/handlers.py - the registry's field is typed by the protocol, so the
# decorated handler and the plain one look the same to an entry point
plan_items: Handler[PlanItemsCommand, PlanItemsResult]
...
plan_items=decorate("PlanItems", PlanItemsHandler(), decorators),
```

```python
# a stub in a step module
async def fail(command: str) -> str:
    raise RuntimeError("store unavailable")

handler = HandlerFunc(fail)
```

## Related concepts

- [Command registry](./command-registry.md) — builds every handler and wraps it with the decorators.
- [Entry runner](./entry-runner.md) — runs the entry points that call handlers.
- [Logging](./logging.md) — what `with_logging` writes.

## Sources

- `typing.Protocol`: https://docs.python.org/3/library/typing.html#typing.Protocol
- Type parameter syntax (PEP 695): https://docs.python.org/3/reference/compound_stmts.html#type-params
