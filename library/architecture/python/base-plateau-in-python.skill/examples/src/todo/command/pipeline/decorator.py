# Source: skill base-plateau-in-python

"""Decorators of every command handler, given as one list: the Decorator
contract, and decorate, which wraps a typed handler with a list of them."""

from collections.abc import Awaitable, Callable, Sequence
from typing import Protocol, cast

from todo.command.pipeline.handler import Handler, HandlerFunc

type Next = Callable[[object], Awaitable[object]]
# Next calls the rest of the chain: the decorators inside the current one,
# then the handler. A decorator awaits it once to let the command through, or
# not at all to reject it.


class Decorator(Protocol):
    """One cross-cutting step around every command handler: logging, an
    access check, a unit of work. It is called with the command's name
    ("PlanItems"), the command itself, and call_next; it returns what
    call_next returned, or raises a KindError to reject the command.

    A Decorator is not generic, so decorators of different steps stand in one
    list that the composition root orders and the command registry
    (todo.command.build_handlers) applies to every handler. It sees the
    command as object; a decorator for some commands only selects them by a
    mark, a runtime-checkable Protocol the command satisfies:

        if isinstance(command, Restricted): ...

    It passes the command on to call_next unchanged. A plain async function
    with these parameters is a Decorator.
    """

    async def __call__(self, name: str, command: object, call_next: Next) -> object: ...


def decorate[TCommand, TResult](
    name: str, handler: Handler[TCommand, TResult], decorators: Sequence[Decorator]
) -> Handler[TCommand, TResult]:
    """Wraps a typed handler with the decorators, the first the outermost: it
    runs first on the way in and last on the way out. It is the one place
    where the typed handlers meet the untyped list of decorators: the command
    goes into the chain as object and comes back to the handler as TCommand;
    the result goes out as object and comes back as TResult.

    TCommand is the command type (PlanItemsCommand), TResult the result type
    (PlanItemsResult) of handler; the returned handler has the same types, so
    an entry point cannot tell a decorated handler from a plain one.
    """

    async def innermost(command: object) -> object:
        return await handler.handle(cast(TCommand, command))

    # Each decorator, from the last to the first, wraps the chain built so
    # far, so the first ends up outermost.
    chain: Next = innermost
    for decorator in reversed(decorators):
        chain = _link(decorator, name, chain)

    async def handle(command: TCommand) -> TResult:
        return cast(TResult, await chain(command))

    return HandlerFunc(handle)


def _link(decorator: Decorator, name: str, call_next: Next) -> Next:
    async def call(command: object) -> object:
        return await decorator(name, command, call_next)

    return call
