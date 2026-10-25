"""The command registry: the handler of every command, built and wrapped by
the decorators in one place."""

from collections.abc import Sequence
from dataclasses import dataclass

from todo.command.pipeline import Decorator, Handler, decorate
from todo.command.plan_items import PlanItemsCommand, PlanItemsHandler, PlanItemsResult


@dataclass(frozen=True)
class Handlers:
    """One field per command, each holding the command's handler already
    wrapped by every decorator. The composition root builds it once with
    build_handlers; an entry point takes from it the handlers it serves (the
    startup entry the one it runs, an API's resource the ones it routes)."""

    plan_items: Handler[PlanItemsCommand, PlanItemsResult]


def build_handlers(decorators: Sequence[Decorator]) -> Handlers:
    """Builds the handler of every command and wraps each with the
    decorators, the first the outermost (see decorate). The services a
    handler uses are keyword parameters here, after the decorators; the base
    plateau has none.

    A new command adds its field above and its line below - the composition
    root does not change.
    """
    return Handlers(
        plan_items=decorate("PlanItems", PlanItemsHandler(), decorators),
    )
