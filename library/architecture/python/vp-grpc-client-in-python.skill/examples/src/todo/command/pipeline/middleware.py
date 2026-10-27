# Source: skill base-plateau-in-python

"""The middleware of every command handler, given as one list: the Middleware
contract, and decorate, which wraps a typed handler with a list of them."""

from collections.abc import Awaitable, Callable, Sequence
from typing import Protocol, cast

from todo.command.pipeline.handler import Handler, HandlerFunc

type Next = Callable[[object], Awaitable[object]]
# Next calls the rest of the chain: the middleware inside the current one,
# then the handler. A middleware awaits it once to let the command through, or
# not at all to reject it.


class Middleware(Protocol):
    """One cross-cutting step around every command handler: logging, an
    access check, a unit of work. It is called with the command's name
    ("PlanItems"), the command itself, and call_next; it returns what
    call_next returned, or raises a KindError to reject the command.

    Each lives in a package of its own under todo.command.middleware,
    named {Name}Middleware - LoggingMiddleware(logger) - and is a class with
    this __call__, or a plain async function with these parameters.

    A Middleware is not generic, so middleware of different steps stand in
    one list that the composition root orders and the command registry
    (todo.command.build_handlers) applies to every handler. It sees the
    command as object; a middleware for some commands only selects them by a
    mark, a runtime-checkable Protocol the command satisfies:

        if isinstance(command, Restricted): ...

    It passes the command on to call_next unchanged.
    """

    async def __call__(self, name: str, command: object, call_next: Next) -> object: ...


def decorate[TCommand, TResult](
    name: str, handler: Handler[TCommand, TResult], middlewares: Sequence[Middleware]
) -> Handler[TCommand, TResult]:
    """Wraps a typed handler with the middleware, the first the outermost: it
    runs first on the way in and last on the way out. It is the one place
    where the typed handlers meet the untyped list of middleware: the command
    goes into the chain as object and comes back to the handler as TCommand;
    the result goes out as object and comes back as TResult.

    TCommand is the command type (PlanItemsCommand), TResult the result type
    (PlanItemsResult) of handler; the returned handler has the same types, so
    an entry point cannot tell a decorated handler from a plain one.
    """

    async def innermost(command: object) -> object:
        return await handler.handle(cast(TCommand, command))

    # Each middleware, from the last to the first, wraps the chain built so
    # far, so the first ends up outermost.
    chain: Next = innermost
    for middleware in reversed(middlewares):
        chain = _link(middleware, name, chain)

    async def handle(command: TCommand) -> TResult:
        return cast(TResult, await chain(command))

    return HandlerFunc(handle)


def _link(middleware: Middleware, name: str, call_next: Next) -> Next:
    async def call(command: object) -> object:
        return await middleware(name, command, call_next)

    return call
