# Source: skill vp-http-client-in-python

"""The client middleware around the attempts of one call."""

from collections.abc import Awaitable, Callable, Sequence
from dataclasses import dataclass
from typing import Protocol


@dataclass(frozen=True)
class Call:
    """One call of an operation of an API, as every middleware sees it."""

    api: str
    """How the log names the API: todo-api."""
    operation: str
    """The operation: "POST /v1/items/plan" over HTTP, the full method name
    "/todo.v1.ItemsService/PlanItems" over gRPC."""
    retryable: bool
    """True when the operation is marked safe to send again: a repeat does
    what one call does."""


type Attempt = Callable[[], Awaitable[None]]
# Attempt sends the call once: one request, bounded by {API}_TIMEOUT, logged
# in its two records, returning or raising a technical outcome.


class Middleware(Protocol):
    """One step around the attempts of a call. It awaits call_next once to
    pass the call on, more than once to repeat it, or raises to refuse it.

    Each lives in a package of its own under infrastructure.client.middleware,
    named {Name}Middleware, and is a class with this __call__, or a plain
    async function with these parameters.
    """

    async def __call__(self, call: Call, call_next: Attempt) -> None: ...


async def run(call: Call, middlewares: Sequence[Middleware], attempt: Attempt) -> None:
    """Sends call through middlewares, the first the outermost, to attempt."""
    chain = attempt
    for middleware in reversed(middlewares):
        chain = _link(middleware, call, chain)
    await chain()


def _link(middleware: Middleware, call: Call, call_next: Attempt) -> Attempt:
    async def linked() -> None:
        await middleware(call, call_next)

    return linked
