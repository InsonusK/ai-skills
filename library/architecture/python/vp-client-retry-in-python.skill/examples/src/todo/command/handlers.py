"""The command registry: the handler of every command, built and wrapped by
the command middleware in one place."""

from collections.abc import Sequence
from dataclasses import dataclass

from todo.command.pipeline import Handler, Middleware, decorate
from todo.command.plan_items import PlanItemsCommand, PlanItemsHandler, PlanItemsResult
from todo.command.plan_remote_items import (
    PlanRemoteItemsCommand,
    PlanRemoteItemsHandler,
    PlanRemoteItemsResult,
    RemotePlanner,
)


@dataclass(frozen=True)
class Handlers:
    """One field per command, each holding the command's handler already
    wrapped by every middleware. The composition root builds it once with
    build_handlers; an entry point takes from it the handlers it serves (the
    startup entry the one it runs, an API's resource the ones it routes)."""

    plan_items: Handler[PlanItemsCommand, PlanItemsResult]
    plan_remote_items: Handler[PlanRemoteItemsCommand, PlanRemoteItemsResult]


def build_handlers(middlewares: Sequence[Middleware], *, remote_planner: RemotePlanner) -> Handlers:
    """Builds the handler of every command and wraps each with the
    middleware, the first the outermost (see decorate). The services a
    handler uses are keyword parameters here, after the middleware: the
    remote planner of PlanRemoteItems.

    A new command adds its field above and its line below - the composition
    root does not change.
    """
    return Handlers(
        plan_items=decorate("PlanItems", PlanItemsHandler(), middlewares),
        plan_remote_items=decorate("PlanRemoteItems", PlanRemoteItemsHandler(remote_planner), middlewares),
    )
