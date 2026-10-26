"""The command registry: the handler of every command, built and wrapped by
the command middleware in one place."""

from collections.abc import Sequence
from dataclasses import dataclass
from typing import Protocol

from todo.command.add_item import AddItemCommand, AddItemHandler, AddItemResult, ItemAdder
from todo.command.list_items import ItemReader, ListItemsCommand, ListItemsHandler, ListItemsResult
from todo.command.pipeline import Handler, Middleware, decorate
from todo.command.plan_items import PlanItemsCommand, PlanItemsHandler, PlanItemsResult


@dataclass(frozen=True)
class Handlers:
    """One field per command, each holding the command's handler already
    wrapped by every middleware. The composition root builds it once with
    build_handlers; an entry point takes from it the handlers it serves (the
    startup entry the one it runs, an API's resource the ones it routes)."""

    plan_items: Handler[PlanItemsCommand, PlanItemsResult]
    add_item: Handler[AddItemCommand, AddItemResult]
    list_items: Handler[ListItemsCommand, ListItemsResult]


class ItemLists(ItemAdder, ItemReader, Protocol):
    """What the handlers need of the item lists; service.ItemLists has it."""


def build_handlers(middlewares: Sequence[Middleware], *, lists: ItemLists) -> Handlers:
    """Builds the handler of every command and wraps each with the
    middleware, the first the outermost (see decorate). The services a
    handler uses are keyword parameters here, after the middleware.

    A new command adds its field above and its line below - the composition
    root does not change.
    """
    return Handlers(
        plan_items=decorate("PlanItems", PlanItemsHandler(), middlewares),
        add_item=decorate("AddItem", AddItemHandler(lists), middlewares),
        list_items=decorate("ListItems", ListItemsHandler(lists), middlewares),
    )
