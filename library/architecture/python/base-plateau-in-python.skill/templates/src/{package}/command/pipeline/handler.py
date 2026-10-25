# Source: skill base-plateau-in-python

"""The one Handler contract every command handler satisfies."""

from collections.abc import Awaitable, Callable
from typing import Protocol


class Handler[TCommand, TResult](Protocol):
    """The one contract every command handler satisfies.

    TCommand is the command type - the input of one operation (for example
    PlanItemsCommand). TResult is the result type the handler returns on
    success (for example PlanItemsResult). Each handler fixes both:
    PlanItemsHandler is a Handler[PlanItemsCommand, PlanItemsResult].

    Because every handler has this one shape, code that is not about a
    specific command - an entry point that serves commands, a decorator that
    wraps them - is written once for Handler[TCommand, TResult] and works with
    any handler. The context of the call is the running task's: its
    cancellation, and its contextvars (the request id).
    """

    async def handle(self, command: TCommand) -> TResult:
        """Executes the command. Returns the result, or raises; a KindError
        of the kind invalid means the command was rejected by a business rule."""
        ...


class HandlerFunc[TCommand, TResult]:
    """Turns an ordinary async function into a Handler: the function is the
    handle method. A handler with its own dependencies is a class
    (PlanItemsHandler); HandlerFunc is for handlers built on the fly -
    decorate returns one wrapping the chain of decorators, and a test writes a
    stub handler in one line.
    """

    def __init__(self, function: Callable[[TCommand], Awaitable[TResult]]) -> None:
        self._function = function

    async def handle(self, command: TCommand) -> TResult:
        return await self._function(command)
